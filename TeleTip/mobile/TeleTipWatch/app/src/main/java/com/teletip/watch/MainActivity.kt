package com.teletip.watch

import android.Manifest
import android.app.Activity
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
import java.util.UUID

class MainActivity : Activity() {
	private lateinit var brokerInput: EditText
	private lateinit var tenantInput: EditText
	private lateinit var patientInput: EditText
	private lateinit var usernameInput: EditText
	private lateinit var passwordInput: EditText
	private lateinit var status: TextView
	private lateinit var dataSource: SamsungHealthDataSource
	private var publisher: MqttTelemetryPublisher? = null
	private var mqttConnected = false
	private var deviceId: UUID = UUID.randomUUID()
	private val preferences by lazy { getSharedPreferences("teletip-watch", MODE_PRIVATE) }

	override fun onCreate(savedInstanceState: Bundle?) {
		super.onCreate(savedInstanceState)
		deviceId = runCatching {
			UUID.fromString(preferences.getString("deviceId", null))
		}.getOrElse {
			UUID.randomUUID().also { preferences.edit().putString("deviceId", it.toString()).apply() }
		}
		dataSource = SamsungHealthSdkDataSource(this)
		buildScreen()
	}

	private fun buildScreen() {
		val content = LinearLayout(this).apply {
			orientation = LinearLayout.VERTICAL
			setPadding(20, 20, 20, 20)
		}

		content.addView(TextView(this).apply {
			text = "TeleTip Watch - Nabız"
			textSize = 18f
		})
		brokerInput = addInput(content, "MQTT broker (tcp://IP:1883 veya ssl://sunucu:8883)")
		tenantInput = addInput(content, "Tenant ID (örn. tenant-1)")
		patientInput = addInput(content, "Hasta UUID")
		usernameInput = addInput(content, "MQTT kullanıcı adı (isteğe bağlı)")
		passwordInput = addInput(content, "MQTT şifresi (isteğe bağlı)", InputType.TYPE_CLASS_TEXT or InputType.TYPE_TEXT_VARIATION_PASSWORD)

		content.addView(Button(this).apply {
			text = "MQTT'ye bağlan"
			setOnClickListener { connectMqtt() }
		})
		content.addView(Button(this).apply {
			text = "Nabız ölçümünü başlat"
			setOnClickListener { startHeartRate() }
		})
		content.addView(Button(this).apply {
			text = "Ölçümü durdur"
			setOnClickListener { dataSource.stopHeartRateUpdates(::showStatus) }
		})
		status = TextView(this).apply {
			text = "Saat ve telefon aynı ağda olmalı. Broker adresini ve hasta bilgilerini girin."
			setPadding(0, 12, 0, 12)
		}
		content.addView(status)

		setContentView(ScrollView(this).apply {
			addView(content, ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT)
		})
	}

	private fun addInput(parent: LinearLayout, hintText: String, inputType: Int = InputType.TYPE_CLASS_TEXT): EditText {
		return EditText(this).apply {
			hint = hintText
			this.inputType = inputType
			maxLines = 1
			parent.addView(this, ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT)
		}
	}

	private fun connectMqtt() {
		val brokerUrl = brokerInput.text.toString().trim()
		val tenantId = tenantInput.text.toString().trim()
		val patientId = runCatching { UUID.fromString(patientInput.text.toString().trim()) }.getOrNull()
		if (brokerUrl.isBlank() || !tenantId.matches(Regex("[A-Za-z0-9_-]+")) || patientId == null) {
			showStatus("Broker adresi, tenant ID ve geçerli hasta UUID'si gerekli.")
			return
		}

		publisher?.disconnect()
		mqttConnected = false
		val newPublisher = MqttTelemetryPublisher(
			applicationContext,
			brokerUrl,
			tenantId,
			patientId,
			deviceId,
			usernameInput.text.toString().trim(),
			passwordInput.text.toString()
		)
		publisher = newPublisher
		showStatus("MQTT broker'a bağlanılıyor...")
		try {
			newPublisher.connect { result ->
				runOnUiThread {
					if (publisher !== newPublisher) return@runOnUiThread
					result.fold(
						onSuccess = {
							mqttConnected = true
							showStatus("MQTT bağlantısı kuruldu. Şimdi nabız ölçümünü başlatın.")
						},
						onFailure = { error -> showStatus("MQTT bağlantı hatası: ${error.message}") }
					)
				}
			}
		} catch (exception: Exception) {
			showStatus("MQTT bağlantı hatası: ${exception.message ?: exception.javaClass.simpleName}")
		}
	}

	private fun startHeartRate() {
		if (!mqttConnected) {
			showStatus("Önce MQTT broker'a bağlanın.")
			return
		}

		val permission = if (Build.VERSION.SDK_INT >= 36) {
			"android.permission.health.READ_HEART_RATE"
		} else {
			Manifest.permission.BODY_SENSORS
		}
		if (checkSelfPermission(permission) != PackageManager.PERMISSION_GRANTED) {
			requestPermissions(arrayOf(permission), HEART_RATE_PERMISSION_REQUEST)
			return
		}
		registerHeartRateListener()
	}

	private fun registerHeartRateListener() {
		dataSource.startHeartRateUpdates(deviceId, { measurement ->
			publisher?.publish(measurement) { result ->
				runOnUiThread {
					result.fold(
						onSuccess = { showStatus("Nabız: ${measurement.value.toInt()} bpm — sunucuya gönderildi.") },
						onFailure = { error -> showStatus("Ölçüm alındı ancak gönderilemedi: ${error.message}") }
					)
				}
			}
		}, ::showStatus)
	}

	override fun onRequestPermissionsResult(requestCode: Int, permissions: Array<out String>, grantResults: IntArray) {
		super.onRequestPermissionsResult(requestCode, permissions, grantResults)
		if (requestCode == HEART_RATE_PERMISSION_REQUEST) {
			if (grantResults.firstOrNull() == PackageManager.PERMISSION_GRANTED) {
				registerHeartRateListener()
			} else {
				showStatus("Nabız verisi için sensör izni gerekli.")
			}
		}
	}

	private fun showStatus(message: String) {
		if (isFinishing || isDestroyed) return
		runOnUiThread { status.text = message }
	}

	override fun onDestroy() {
		dataSource.stopHeartRateUpdates()
		publisher?.disconnect()
		super.onDestroy()
	}

	private companion object {
		const val HEART_RATE_PERMISSION_REQUEST = 1001
	}
}
