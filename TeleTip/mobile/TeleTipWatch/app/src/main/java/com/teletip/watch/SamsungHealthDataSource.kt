package com.teletip.watch

import java.time.Instant
import java.util.UUID

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
	val measuredAt: Instant = Instant.now()
)

interface SamsungHealthDataSource {
	suspend fun isAvailable(type: WatchHealthType): Boolean
	suspend fun readLatest(type: WatchHealthType): WatchMeasurement?
}

/**
 * Samsung Health SDK adapter boundary.
 *
 * The implementation must be completed with the Samsung Health SDK approved for
 * the target account/device. Blood pressure requires calibration, and glucose
 * requires a supported external CGM; neither is assumed to be a direct watch
 * sensor reading.
 */
class SamsungHealthSdkDataSource : SamsungHealthDataSource {
	override suspend fun isAvailable(type: WatchHealthType): Boolean = false

	override suspend fun readLatest(type: WatchHealthType): WatchMeasurement? = null
}
