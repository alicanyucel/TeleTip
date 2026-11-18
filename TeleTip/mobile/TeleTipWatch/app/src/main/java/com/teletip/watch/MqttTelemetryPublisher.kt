package com.teletip.watch

import android.content.Context
import org.eclipse.paho.android.service.MqttAndroidClient
import org.eclipse.paho.client.mqttv3.IMqttActionListener
import org.eclipse.paho.client.mqttv3.IMqttToken
import org.eclipse.paho.client.mqttv3.MqttConnectOptions
import org.eclipse.paho.client.mqttv3.MqttMessage
import org.json.JSONObject
import java.util.UUID

class MqttTelemetryPublisher(
	context: Context,
	private val brokerUrl: String,
	private val tenantId: String,
	private val patientId: UUID,
	private val deviceId: UUID,
	private val username: String = "",
	private val password: String = ""
) {
	private val client = MqttAndroidClient(
		context,
		brokerUrl,
		"teletip-watch-${deviceId.toString().take(8)}"
	)

	fun connect(onResult: (Result<Unit>) -> Unit) {
		val options = MqttConnectOptions().apply {
			isCleanSession = true
			isAutomaticReconnect = true
			if (username.isNotBlank()) {
				userName = username
				this.password = this@MqttTelemetryPublisher.password.toCharArray()
			}
		}

		client.connect(options, null, object : IMqttActionListener {
			override fun onSuccess(asyncActionToken: IMqttToken?) = onResult(Result.success(Unit))
			override fun onFailure(asyncActionToken: IMqttToken?, exception: Throwable?) =
				onResult(Result.failure(exception ?: IllegalStateException("MQTT connection failed")))
		})
	}

	fun publish(measurement: WatchMeasurement, onResult: (Result<Unit>) -> Unit = {}) {
		if (!client.isConnected) {
			onResult(Result.failure(IllegalStateException("MQTT client is not connected")))
			return
		}

		val topic = "teletip/tenants/$tenantId/patients/$patientId/measurements"
		val type = when (measurement.type) {
			WatchHealthType.HEART_RATE -> "HeartRate"
			WatchHealthType.BLOOD_PRESSURE -> "BloodPressure"
			WatchHealthType.BLOOD_GLUCOSE -> "BloodGlucose"
			WatchHealthType.OXYGEN_SATURATION -> "OxygenSaturation"
			WatchHealthType.BODY_TEMPERATURE -> "BodyTemperature"
			WatchHealthType.STEPS -> "Steps"
		}
		val payload = JSONObject()
			.put("deviceId", measurement.deviceId.toString())
			.put("type", type)
			.put("value", measurement.value)
			.put("unit", measurement.unit)
			.put("measuredAt", measurement.measuredAt.toString())
			.put("source", measurement.source)
			.apply { measurement.recordId?.let { put("recordId", it) } }
			.toString()

		val message = MqttMessage(payload.toByteArray(Charsets.UTF_8)).apply {
			qos = 1
			isRetained = false
		}

		try {
			client.publish(topic, message, null, object : IMqttActionListener {
				override fun onSuccess(asyncActionToken: IMqttToken?) = onResult(Result.success(Unit))
				override fun onFailure(asyncActionToken: IMqttToken?, exception: Throwable?) =
					onResult(Result.failure(exception ?: IllegalStateException("MQTT publish failed")))
			})
		} catch (exception: Exception) {
			onResult(Result.failure(exception))
		}
	}

	fun disconnect() {
		if (client.isConnected) client.disconnect()
	}
}
