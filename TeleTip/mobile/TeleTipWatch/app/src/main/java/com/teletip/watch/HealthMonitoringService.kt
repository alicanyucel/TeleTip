package com.teletip.watch

import android.app.Notification
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.app.Service
import android.content.Intent
import android.os.Build
import android.os.IBinder
import android.os.SystemClock
import androidx.core.app.NotificationCompat
import androidx.core.content.ContextCompat
import androidx.health.services.client.ExerciseUpdateCallback
import androidx.health.services.client.HealthServices
import androidx.health.services.client.data.DataPointContainer
import androidx.health.services.client.data.DataType
import androidx.health.services.client.data.ExerciseConfig
import androidx.health.services.client.data.ExerciseLapSummary
import androidx.health.services.client.data.ExerciseType
import androidx.health.services.client.data.ExerciseUpdate
import androidx.health.services.client.data.ExerciseState
import androidx.health.services.client.data.Availability
import androidx.health.services.client.data.DataType as HealthDataType
import java.time.Instant
import java.util.UUID
import java.util.concurrent.ExecutionException

class HealthMonitoringService : Service() {
	private val callbackExecutor by lazy { ContextCompat.getMainExecutor(this) }
	private var publisher: MqttTelemetryPublisher? = null
	private var exerciseCallback: ExerciseUpdateCallback? = null
	private var deviceId: UUID = UUID.randomUUID()
	private var lastHeartRate: Double? = null
	private var lastSteps: Double? = null
	private var isStopping = false
	private val exerciseClient by lazy { HealthServices.getClient(this).exerciseClient }

	override fun onBind(intent: Intent?): IBinder? = null

	override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
		if (intent?.action == ACTION_STOP) {
			stopMonitoring()
			return START_NOT_STICKY
		}
		if (intent?.action != ACTION_START) return START_NOT_STICKY

		startForeground(NOTIFICATION_ID, createNotification("Bağlantılar hazırlanıyor"))
		val brokerUrl = intent.getStringExtra(EXTRA_BROKER).orEmpty()
		val tenantId = intent.getStringExtra(EXTRA_TENANT).orEmpty()
		val patientId = runCatching { UUID.fromString(intent.getStringExtra(EXTRA_PATIENT)) }.getOrNull()
		deviceId = runCatching { UUID.fromString(intent.getStringExtra(EXTRA_DEVICE)) }.getOrDefault(UUID.randomUUID())
		if (brokerUrl.isBlank() || tenantId.isBlank() || patientId == null) {
			stopMonitoring("Hasta ve broker ayarları geçersiz.")
			return START_NOT_STICKY
		}

		publisher?.disconnect()
		publisher = MqttTelemetryPublisher(
			applicationContext,
			brokerUrl,
			tenantId,
			patientId,
			deviceId,
			intent.getStringExtra(EXTRA_USERNAME).orEmpty(),
			intent.getStringExtra(EXTRA_PASSWORD).orEmpty()
		)
		publisher?.connect { result ->
			result.fold(
				onSuccess = { startExercise() },
				onFailure = { error -> stopMonitoring("MQTT bağlantısı kurulamadı: ${error.message}") }
			)
		}
		return START_NOT_STICKY
	}

	private fun startExercise() {
		val callback = object : ExerciseUpdateCallback {
			override fun onExerciseUpdateReceived(update: ExerciseUpdate) {
				if (update.exerciseStateInfo.state == ExerciseState.ENDED) {
					stopMonitoring("Sağlık oturumu Health Services tarafından sonlandırıldı.")
					return
				}
				publishMetrics(update.latestMetrics)
			}

			override fun onLapSummaryReceived(lapSummary: ExerciseLapSummary) = Unit

			override fun onAvailabilityChanged(dataType: HealthDataType<*, *>, availability: Availability) {
				updateNotification("Sensör: $availability")
			}

			override fun onRegistered() {
				updateNotification("Health Services ölçüm oturumu etkin")
			}

			override fun onRegistrationFailed(throwable: Throwable) {
				stopMonitoring("Health Services oturumu başlatılamadı: ${throwable.message}")
			}
		}
		exerciseCallback = callback
		exerciseClient.setUpdateCallback(callbackExecutor, callback)

		val capabilitiesFuture = exerciseClient.getCapabilitiesAsync()
		capabilitiesFuture.addListener({
			try {
				val capabilities = capabilitiesFuture.get()
				val exerciseCapabilities = capabilities.getExerciseTypeCapabilities(ExerciseType.WORKOUT)
				val supported = exerciseCapabilities.supportedDataTypes
				val requested = setOf(DataType.HEART_RATE_BPM, DataType.STEPS).intersect(supported)
				if (DataType.HEART_RATE_BPM !in requested) {
					stopMonitoring("Bu saat WORKOUT oturumunda canlı nabız verisini desteklemiyor.")
					return@addListener
				}
				val config = ExerciseConfig.builder(ExerciseType.WORKOUT)
					.setDataTypes(requested)
					.setIsAutoPauseAndResumeEnabled(false)
					.setIsGpsEnabled(false)
					.build()
				val startExerciseFuture = exerciseClient.startExerciseAsync(config)
				startExerciseFuture.addListener({
					try {
						startExerciseFuture.get()
						updateNotification(if (DataType.STEPS in requested) "Nabız ve adım takibi çalışıyor" else "Nabız takibi çalışıyor; adım sensörü desteklenmiyor")
					} catch (exception: Exception) {
						val cause = (exception as? ExecutionException)?.cause ?: exception
						stopMonitoring("Sağlık oturumu başlatılamadı: ${cause.message}")
					}
				}, callbackExecutor)
			} catch (exception: Exception) {
				val cause = (exception as? ExecutionException)?.cause ?: exception
				stopMonitoring("Health Services başlatılamadı: ${cause.message}")
			}
		}, callbackExecutor)
	}

	private fun publishMetrics(metrics: DataPointContainer) {
		val bootInstant = Instant.ofEpochMilli(System.currentTimeMillis() - SystemClock.elapsedRealtime())
		metrics.getData(DataType.HEART_RATE_BPM).forEach { point ->
			val bpm = point.value.toDouble()
			if (bpm.isFinite() && bpm > 0 && bpm != lastHeartRate) {
				lastHeartRate = bpm
				publishMeasurement(
					WatchMeasurement(deviceId, WatchHealthType.HEART_RATE, bpm, "bpm", point.getTimeInstant(bootInstant))
				)
			}
		}
		metrics.getData(DataType.STEPS).forEach { point ->
			val steps = point.value.toDouble()
			if (steps.isFinite() && steps > 0 && steps != lastSteps) {
				lastSteps = steps
				publishMeasurement(
					WatchMeasurement(deviceId, WatchHealthType.STEPS, steps, "count", Instant.now())
				)
			}
		}
	}

	private fun publishMeasurement(measurement: WatchMeasurement) {
		publisher?.publish(measurement) { result ->
			result.fold(
				onSuccess = { updateNotification("Son veri: ${measurement.type} ${measurement.value}") },
				onFailure = { error -> updateNotification("MQTT gönderim hatası: ${error.message}") }
			)
		}
	}

	private fun stopMonitoring(message: String = "Ölçüm durduruldu.") {
		if (isStopping) return
		isStopping = true
		updateNotification(message)
		val callback = exerciseCallback
		exerciseCallback = null
		if (callback != null) {
			val endExerciseFuture = try {
				exerciseClient.endExerciseAsync()
			} catch (exception: Exception) {
				updateNotification("Health Services oturumu kapatılamadı: ${exception.message}")
				finishService()
				return
			}
			endExerciseFuture.addListener({
				try {
					endExerciseFuture.get()
				} catch (exception: Exception) {
					updateNotification("Health Services oturumu kapatılırken hata: ${exception.message}")
				}
				try {
					val clearCallbackFuture = exerciseClient.clearUpdateCallbackAsync(callback)
					clearCallbackFuture.addListener({
						try {
							clearCallbackFuture.get()
						} catch (exception: Exception) {
							updateNotification("Health Services callback kapatılamadı: ${exception.message}")
						} finally {
							finishService()
						}
					}, callbackExecutor)
				} catch (exception: Exception) {
					updateNotification("Health Services callback kapatılamadı: ${exception.message}")
					finishService()
				}
			}, callbackExecutor)
		} else {
			finishService()
		}
	}

	private fun finishService() {
		publisher?.disconnect()
		publisher = null
		stopForeground(STOP_FOREGROUND_REMOVE)
		stopSelf()
	}

	private fun createNotification(message: String): Notification {
		if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
			val manager = getSystemService(NotificationManager::class.java)
			manager.createNotificationChannel(
				NotificationChannel(CHANNEL_ID, "TeleTip sağlık takibi", NotificationManager.IMPORTANCE_LOW)
			)
		}
		val stopIntent = PendingIntent.getService(
			this,
			STOP_REQUEST_CODE,
			Intent(this, HealthMonitoringService::class.java).setAction(ACTION_STOP),
			PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE
		)
		return NotificationCompat.Builder(this, CHANNEL_ID)
			.setSmallIcon(android.R.drawable.ic_menu_info_details)
			.setContentTitle("TeleTip sağlık takibi")
			.setContentText(message)
			.setOngoing(true)
			.addAction(0, "Durdur", stopIntent)
			.build()
	}

	private fun updateNotification(message: String) {
		getSystemService(NotificationManager::class.java).notify(NOTIFICATION_ID, createNotification(message))
	}

	override fun onDestroy() {
		publisher?.disconnect()
		super.onDestroy()
	}

	companion object {
		const val ACTION_START = "com.teletip.watch.action.START_MONITORING"
		const val ACTION_STOP = "com.teletip.watch.action.STOP_MONITORING"
		const val EXTRA_BROKER = "broker"
		const val EXTRA_TENANT = "tenant"
		const val EXTRA_PATIENT = "patient"
		const val EXTRA_DEVICE = "device"
		const val EXTRA_USERNAME = "username"
		const val EXTRA_PASSWORD = "password"
		private const val CHANNEL_ID = "teletip-health-monitoring"
		private const val NOTIFICATION_ID = 1002
		private const val STOP_REQUEST_CODE = 1003
	}
}
