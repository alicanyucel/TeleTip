package com.teletip.watch

import android.content.Context
import android.os.SystemClock
import androidx.core.content.ContextCompat
import androidx.health.services.client.HealthServices
import androidx.health.services.client.MeasureCallback
import androidx.health.services.client.MeasureClient
import androidx.health.services.client.data.Availability
import androidx.health.services.client.data.DataPointContainer
import androidx.health.services.client.data.DataType
import androidx.health.services.client.data.DeltaDataType
import java.time.Instant
import java.util.UUID
import java.util.concurrent.ExecutionException

enum class WatchHealthType {
	HEART_RATE,
	BLOOD_PRESSURE,
	BLOOD_GLUCOSE,
	OXYGEN_SATURATION,
	BODY_TEMPERATURE,
	STEPS
}

data class WatchMeasurement(
	val deviceId: UUID,
	val type: WatchHealthType,
	val value: Double,
	val unit: String,
	val measuredAt: Instant = Instant.now(),
	val source: String = "Samsung Galaxy Watch",
	val recordId: String? = null
)

interface SamsungHealthDataSource {
	fun startHeartRateUpdates(
		deviceId: UUID,
		onMeasurement: (WatchMeasurement) -> Unit,
		onStatus: (String) -> Unit
	)

	fun stopHeartRateUpdates(onStatus: (String) -> Unit = {})
}

class SamsungHealthSdkDataSource(context: Context) : SamsungHealthDataSource {
	private val appContext = context.applicationContext
	private val measureClient: MeasureClient = HealthServices.getClient(appContext).measureClient
	private val mainExecutor = ContextCompat.getMainExecutor(appContext)
	private var activeCallback: MeasureCallback? = null

	override fun startHeartRateUpdates(
		deviceId: UUID,
		onMeasurement: (WatchMeasurement) -> Unit,
		onStatus: (String) -> Unit
	) {
		if (activeCallback != null) {
			onStatus("Nabız ölçümü zaten çalışıyor.")
			return
		}

		val callback = object : MeasureCallback {
			override fun onDataReceived(data: DataPointContainer) {
				data.getData(DataType.HEART_RATE_BPM).forEach { point ->
					val value = point.value.toDouble()
					if (value.isFinite() && value > 0) {
						val bootInstant = Instant.ofEpochMilli(
							System.currentTimeMillis() - SystemClock.elapsedRealtime()
						)
						onMeasurement(
							WatchMeasurement(
								deviceId = deviceId,
								type = WatchHealthType.HEART_RATE,
								value = value,
								unit = "bpm",
								measuredAt = point.getTimeInstant(bootInstant)
							)
						)
					}
				}
			}

			override fun onAvailabilityChanged(dataType: DeltaDataType<*, *>, availability: Availability) {
				onStatus("Nabız sensörü: $availability")
			}

			override fun onRegistered() {
				activeCallback = this
				onStatus("Nabız ölçümü başladı. Saat uygulamasını açık tutun.")
			}

			override fun onRegistrationFailed(throwable: Throwable) {
				activeCallback = null
				onStatus("Nabız ölçümü başlatılamadı: ${throwable.message ?: "Health Services hatası"}")
			}
		}

		activeCallback = callback
		val capabilitiesFuture = measureClient.getCapabilitiesAsync()
		capabilitiesFuture.addListener({
			try {
				val capabilities = capabilitiesFuture.get()
				if (activeCallback !== callback) return@addListener
				if (DataType.HEART_RATE_BPM !in capabilities.supportedDataTypesMeasure) {
					activeCallback = null
					onStatus("Bu saat Health Services üzerinden canlı nabız ölçümünü desteklemiyor.")
					return@addListener
				}

				measureClient.registerMeasureCallback(DataType.HEART_RATE_BPM, callback)
			} catch (exception: Exception) {
				if (activeCallback === callback) activeCallback = null
				val cause = (exception as? ExecutionException)?.cause ?: exception
				onStatus("Nabız sensörü başlatılamadı: ${cause.message ?: cause.javaClass.simpleName}")
			}
		}, mainExecutor)
	}

	override fun stopHeartRateUpdates(onStatus: (String) -> Unit) {
		val callback = activeCallback ?: return
		activeCallback = null
		val unregisterFuture = measureClient.unregisterMeasureCallbackAsync(
			DataType.HEART_RATE_BPM,
			callback
		)
		unregisterFuture.addListener({
			try {
				unregisterFuture.get()
				onStatus("Nabız ölçümü durduruldu.")
			} catch (exception: Exception) {
				val cause = (exception as? ExecutionException)?.cause ?: exception
				onStatus("Nabız ölçümü durdurulurken hata oluştu: ${cause.message ?: cause.javaClass.simpleName}")
			}
		}, mainExecutor)
	}
}
