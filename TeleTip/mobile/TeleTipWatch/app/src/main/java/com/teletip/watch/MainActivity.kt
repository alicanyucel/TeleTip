package com.teletip.watch

import android.Manifest
import android.content.Intent
import android.content.pm.PackageManager
import android.os.Build
import android.os.Bundle
import android.text.InputType
import android.view.ViewGroup
import android.widget.Button
import android.widget.EditText
import android.widget.LinearLayout
import android.widget.ScrollView
import android.widget.TextView
import androidx.activity.ComponentActivity
import androidx.activity.result.ActivityResultLauncher
import androidx.activity.result.contract.ActivityResultContracts
import androidx.core.content.ContextCompat
import androidx.health.connect.client.HealthConnectClient
import androidx.health.connect.client.permission.HealthPermission
import androidx.health.connect.client.PermissionController
import androidx.health.connect.client.records.BloodGlucoseRecord
import androidx.health.connect.client.records.BloodPressureRecord
import androidx.lifecycle.lifecycleScope
import androidx.security.crypto.EncryptedSharedPreferences
import androidx.security.crypto.MasterKey
import java.util.UUID
import kotlinx.coroutines.launch

class MainActivity : ComponentActivity() {
	private lateinit var brokerInput: EditText
	private lateinit var tenantInput: EditText
	private lateinit var patientInput: EditText
	private lateinit var usernameInput: EditText
	private lateinit var passwordInput: EditText
	private lateinit var status: TextView
	private lateinit var healthConnectPermissionLauncher: ActivityResultLauncher<Set<String>>
	private lateinit var foregroundPermissionLauncher: ActivityResultLauncher<Array<String>>
	private lateinit var backgroundPermissionLauncher: ActivityResultLauncher<String>
	private var publisher: MqttTelemetryPublisher? = null
	private var deviceId: UUID = UUID.randomUUID()
	private val preferences by lazy {
		val masterKey = MasterKey.Builder(this).setKeyScheme(MasterKey.KeyScheme.AES256_GCM).build()
		EncryptedSharedPreferences.create(
			this,
			"teletip-watch-secure",
			masterKey,
			EncryptedSharedPreferences.PrefKeyEncryptionScheme.AES256_SIV,
			EncryptedSharedPreferences.PrefValueEncryptionScheme.AES256_GCM
		)
	}

	override fun onCreate(savedInstanceState: Bundle?) {
		super.onCreate(savedInstanceState)
		deviceId = runCatching { UUID.fromString(preferences.getString("deviceId", null)) }
			.getOrElse { UUID.randomUUID().also { preferences.edit().putString("deviceId", it.toString()).apply() } }
		healthConnectPermissionLauncher = registerForActivityResult(
			PermissionController.createRequestPermissionResultContract()
		) { granted ->
			val required = healthConnectPermissions()
			if (granted.containsAll(required)) importHealthConnectRecords()
			else showStatus("Health Connect tansiyon ve glikoz okuma izinleri verilmedi.")
		}
		foregroundPermissionLauncher = registerForActivityResult(ActivityResultContracts.RequestMultiplePermissions()) { grants ->
			if (grants.isNotEmpty() && grants.values.all { it }) requestBackgroundSensorPermissionOrStart()
			else showStatus("Uzun ölçüm için nabız, adım ve bildirim izinleri gereklidir.")
		}
		backgroundPermissionLauncher = registerForActivityResult(ActivityResultContracts.RequestPermission()) { granted ->
			if (granted) startMonitoringService()
			else showStatus("Arka plan sensör izni verilmedi; uzun oturum başlatılamadı.")
		}
		buildScreen()
		loadConfiguration()
	}

	private fun buildScreen() {
		val content = LinearLayout(this).apply {
			orientation = LinearLayout.VERTICAL
			setPadding(20, 20, 20, 20)
		}
		content.addView(TextView(this).apply {
			text = "TeleTip Watch - Hasta ve Ölçüm Ayarları"
			textSize = 18f
		})
		brokerInput = addInput(content, "MQTT broker (ssl://sunucu:8883 önerilir)")
		tenantInput = addInput(content, "Tenant ID (örn. tenant-1)")
		patientInput = addInput(content, "Hasta UUID")
		usernameInput = addInput(content, "MQTT kullanıcı adı")
		passwordInput = addInput(content, "MQTT şifresi", InputType.TYPE_CLASS_TEXT or InputType.TYPE_TEXT_VARIATION_PASSWORD)

		content.addView(Button(this).apply {
			text = "Ayarları kaydet"
			setOnClickListener { saveConfiguration() }
		})
		content.addView(Button(this).apply {
			text = "MQTT bağlantısını test et"
			setOnClickListener { testMqttConnection() }
		})
		content.addView(Button(this).apply {
			text = "Nabız ve adım ölçümünü başlat"
			setOnClickListener { requestMonitoringPermissions() }
		})
		content.addView(Button(this).apply {
			text = "Arka plan ölçümünü durdur"
			setOnClickListener {
				startService(Intent(this@MainActivity, HealthMonitoringService::class.java).setAction(HealthMonitoringService.ACTION_STOP))
				showStatus("Ölçüm servisine durdurma isteği gönderildi.")
			}
		})
		content.addView(Button(this).apply {
			text = "Health Connect tansiyon/glikoz verilerini aktar"
			setOnClickListener { requestHealthConnectPermissions() }
		})
		status = TextView(this).apply {
			text = "Broker, tenant ve hasta UUID bilgilerini kaydedin. Arka plan oturumu sürekli bildirim gösterir."
			setPadding(0, 12, 0, 12)
		}
		content.addView(status)
		setContentView(ScrollView(this).apply {
			addView(content, ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT)
		})
	}

	private fun addInput(parent: LinearLayout, hintText: String, inputType: Int = InputType.TYPE_CLASS_TEXT): EditText =
		EditText(this).apply {
			hint = hintText
			this.inputType = inputType
			maxLines = 1
			parent.addView(this, ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT)
		}

	private fun loadConfiguration() {
		brokerInput.setText(preferences.getString("broker", ""))
		tenantInput.setText(preferences.getString("tenant", ""))
		patientInput.setText(preferences.getString("patient", ""))
		usernameInput.setText(preferences.getString("username", ""))
		passwordInput.setText(preferences.getString("password", ""))
	}

	private fun saveConfiguration(): Boolean {
		val broker = brokerInput.text.toString().trim()
		val tenant = tenantInput.text.toString().trim()
		val patient = patientInput.text.toString().trim()
		if (broker.isBlank() || !tenant.matches(Regex("[A-Za-z0-9_-]+")) || runCatching { UUID.fromString(patient) }.isFailure) {
			showStatus("Broker adresi, geçerli tenant ID ve hasta UUID'si gerekli.")
			return false
		}
		preferences.edit()
			.putString("broker", broker)
			.putString("tenant", tenant)
			.putString("patient", patient)
			.putString("username", usernameInput.text.toString().trim())
			.putString("password", passwordInput.text.toString())
			.apply()
		showStatus("Hasta ve tenant ayarları şifreli olarak kaydedildi.")
		return true
	}

	private fun testMqttConnection() {
		if (!saveConfiguration()) return
		publisher?.disconnect()
		val config = currentConfiguration() ?: return
		val testPublisher = MqttTelemetryPublisher(
			applicationContext, config.broker, config.tenant, config.patient, deviceId, config.username, config.password
		)
		publisher = testPublisher
		showStatus("MQTT broker bağlantısı test ediliyor...")
		try {
			testPublisher.connect { result ->
				runOnUiThread {
					result.fold(
						onSuccess = {
							showStatus("MQTT bağlantısı başarılı; canlı sensör testi için ölçüm oturumunu başlatın.")
							testPublisher.disconnect()
						},
						onFailure = { error -> showStatus("MQTT bağlantısı başarısız: ${error.message}") }
					)
				}
			}
		} catch (exception: Exception) {
			showStatus("MQTT bağlantı testi başarısız: ${exception.message ?: exception.javaClass.simpleName}")
		}
	}

	private fun requestMonitoringPermissions() {
		if (!saveConfiguration()) return
		val foregroundPermissions = mutableListOf<String>()
		val heartRatePermission = if (Build.VERSION.SDK_INT >= 36) {
			"android.permission.health.READ_HEART_RATE"
		} else {
			Manifest.permission.BODY_SENSORS
		}
		if (checkSelfPermission(heartRatePermission) != PackageManager.PERMISSION_GRANTED) foregroundPermissions += heartRatePermission
		if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q &&
			checkSelfPermission(Manifest.permission.ACTIVITY_RECOGNITION) != PackageManager.PERMISSION_GRANTED
		) foregroundPermissions += Manifest.permission.ACTIVITY_RECOGNITION
		if (Build.VERSION.SDK_INT >= 33 &&
			checkSelfPermission(Manifest.permission.POST_NOTIFICATIONS) != PackageManager.PERMISSION_GRANTED
		) foregroundPermissions += Manifest.permission.POST_NOTIFICATIONS

		if (foregroundPermissions.isNotEmpty()) {
			foregroundPermissionLauncher.launch(foregroundPermissions.toTypedArray())
			return
		}
		requestBackgroundSensorPermissionOrStart()
	}

	private fun requestBackgroundSensorPermissionOrStart() {
		if (Build.VERSION.SDK_INT in 33..35 &&
			checkSelfPermission(Manifest.permission.BODY_SENSORS_BACKGROUND) != PackageManager.PERMISSION_GRANTED
		) {
			backgroundPermissionLauncher.launch(Manifest.permission.BODY_SENSORS_BACKGROUND)
			return
		}
		startMonitoringService()
	}

	private fun startMonitoringService() {
		val config = currentConfiguration() ?: return
		val intent = Intent(this, HealthMonitoringService::class.java)
			.setAction(HealthMonitoringService.ACTION_START)
			.putExtra(HealthMonitoringService.EXTRA_BROKER, config.broker)
			.putExtra(HealthMonitoringService.EXTRA_TENANT, config.tenant)
			.putExtra(HealthMonitoringService.EXTRA_PATIENT, config.patient.toString())
			.putExtra(HealthMonitoringService.EXTRA_DEVICE, deviceId.toString())
			.putExtra(HealthMonitoringService.EXTRA_USERNAME, config.username)
			.putExtra(HealthMonitoringService.EXTRA_PASSWORD, config.password)
		ContextCompat.startForegroundService(this, intent)
		showStatus("Arka plan sağlık servisi başlatılıyor; kalıcı bildirimi kontrol edin.")
	}

	private fun requestHealthConnectPermissions() {
		if (!saveConfiguration()) return
		val status = HealthConnectClient.getSdkStatus(this, HealthConnectImporter.PROVIDER_PACKAGE_NAME)
		if (status != HealthConnectClient.SDK_AVAILABLE) {
			showStatus("Bu cihazda Health Connect kullanılamıyor. Harici tansiyon/glikoz cihazı verileri telefon eşlikçi uygulamasında Health Connect'e eşitlenmelidir.")
			return
		}
		healthConnectPermissionLauncher.launch(healthConnectPermissions())
	}

	private fun healthConnectPermissions(): Set<String> = setOf(
		HealthPermission.getReadPermission(BloodPressureRecord::class),
		HealthPermission.getReadPermission(BloodGlucoseRecord::class)
	)

	private fun importHealthConnectRecords() {
		val config = currentConfiguration() ?: return
		lifecycleScope.launch {
			try {
				val measurements = HealthConnectImporter(this@MainActivity).readRecentMeasurements(deviceId)
				val sentIds = preferences.getStringSet("healthConnectSentIds", emptySet()).orEmpty().toMutableSet()
				val pending = measurements.filter { it.recordId !in sentIds }
				if (pending.isEmpty()) {
					showStatus("Son 7 günde aktarılacak yeni tansiyon veya glikoz kaydı bulunamadı.")
					return@launch
				}
				val importPublisher = MqttTelemetryPublisher(
					applicationContext, config.broker, config.tenant, config.patient, deviceId, config.username, config.password
				)
				importPublisher.connect { result ->
					result.fold(
						onSuccess = {
							lifecycleScope.launch {
								try {
									pending.forEachIndexed { index, measurement ->
										publishAwaitingAck(importPublisher, measurement)
										measurement.recordId?.let { sentIds += it }
										preferences.edit().putStringSet("healthConnectSentIds", sentIds.toList().takeLast(MAX_SENT_RECORD_IDS).toSet()).apply()
										showStatus("Health Connect kaydı gönderildi (${index + 1}/${pending.size}).")
									}
									importPublisher.disconnect()
								} catch (exception: Exception) {
									importPublisher.disconnect()
									showStatus("Health Connect aktarımı başarısız oldu: ${exception.message}")
								}
							}
						},
						onFailure = { error ->
							importPublisher.disconnect()
							showStatus("Health Connect verisi için MQTT bağlantısı kurulamadı: ${error.message}")
						}
					)
				}
			} catch (exception: Exception) {
				showStatus("Health Connect kayıtları okunamadı: ${exception.message}")
			}
		}
	}

	private suspend fun publishAwaitingAck(publisher: MqttTelemetryPublisher, measurement: WatchMeasurement) =
		kotlin.coroutines.suspendCoroutine<Unit> { continuation ->
			publisher.publish(measurement) { result -> continuation.resumeWith(result) }
		}

	private fun currentConfiguration(): WatchConfiguration? {
		val patient = runCatching { UUID.fromString(patientInput.text.toString().trim()) }.getOrNull()
		val broker = brokerInput.text.toString().trim()
		val tenant = tenantInput.text.toString().trim()
		if (patient == null || broker.isBlank() || !tenant.matches(Regex("[A-Za-z0-9_-]+"))) {
			showStatus("Önce geçerli broker, tenant ve hasta UUID ayarlarını kaydedin.")
			return null
		}
		return WatchConfiguration(broker, tenant, patient, usernameInput.text.toString().trim(), passwordInput.text.toString())
	}


	private fun showStatus(message: String) {
		if (!::status.isInitialized || isFinishing || isDestroyed) return
		runOnUiThread { status.text = message }
	}

	override fun onDestroy() {
		publisher?.disconnect()
		super.onDestroy()
	}

	private data class WatchConfiguration(
		val broker: String,
		val tenant: String,
		val patient: UUID,
		val username: String,
		val password: String
	)

	private companion object {
		const val MAX_SENT_RECORD_IDS = 10_000
	}
}
