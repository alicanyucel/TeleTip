package com.teletip.watch

import android.content.Context
import org.eclipse.paho.android.service.MqttAndroidClient
import org.eclipse.paho.client.mqttv3.IMqttActionListener
import org.eclipse.paho.client.mqttv3.IMqttToken
import org.eclipse.paho.client.mqttv3.MqttConnectOptions
import org.eclipse.paho.client.mqttv3.MqttMessage
import java.util.UUID

class MqttTelemetryPublisher(
	context: Context,
	private val brokerUrl: String,
	private val tenantId: String,
	private val patientId: UUID,
	private val deviceId: UUID
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
		val payload = """
			{
			  "deviceId":"${measurement.deviceId}",
			  "type":"${measurement.type}",
			  "value":${measurement.value},
			  "unit":"${measurement.unit}",
			  "measuredAt":"${measurement.measuredAt}"
			}
		""".trimIndent()

		val message = MqttMessage(payload.toByteArray()).apply {
			qos = 1
			isRetained = false
		}

		client.publish(topic, message, null, object : IMqttActionListener {
			override fun onSuccess(asyncActionToken: IMqttToken?) = onResult(Result.success(Unit))
			override fun onFailure(asyncActionToken: IMqttToken?, exception: Throwable?) =
				onResult(Result.failure(exception ?: IllegalStateException("MQTT publish failed")))
		})
	}

	fun disconnect() {
		if (client.isConnected) client.disconnect()
	}
}
