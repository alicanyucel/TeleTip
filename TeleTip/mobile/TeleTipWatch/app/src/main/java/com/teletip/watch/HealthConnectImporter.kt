package com.teletip.watch

import android.content.Context
import androidx.health.connect.client.HealthConnectClient
import androidx.health.connect.client.request.ReadRecordsRequest
import androidx.health.connect.client.records.BloodGlucoseRecord
import androidx.health.connect.client.records.BloodPressureRecord
import androidx.health.connect.client.time.TimeRangeFilter
import java.time.Instant
import java.time.temporal.ChronoUnit
import java.util.UUID

class HealthConnectImporter(context: Context) {
	private val client = HealthConnectClient.getOrCreate(context)

	suspend fun readRecentMeasurements(deviceId: UUID): List<WatchMeasurement> {
		val end = Instant.now()
		val start = end.minus(7, ChronoUnit.DAYS)
		val range = TimeRangeFilter.between(start, end)
		val measurements = mutableListOf<WatchMeasurement>()

		readRecords<BloodPressureRecord>(range).forEach { record ->
			val source = record.metadata.dataOrigin.packageName
			measurements += WatchMeasurement(
				deviceId,
				WatchHealthType.BLOOD_PRESSURE,
				record.systolic.inMillimetersOfMercury,
				"mmHg_systolic",
				record.time,
				source,
				"${record.metadata.id}:systolic"
			)
			measurements += WatchMeasurement(
				deviceId,
				WatchHealthType.BLOOD_PRESSURE,
				record.diastolic.inMillimetersOfMercury,
				"mmHg_diastolic",
				record.time,
				source,
				"${record.metadata.id}:diastolic"
			)
		}

		readRecords<BloodGlucoseRecord>(range).forEach { record ->
			measurements += WatchMeasurement(
				deviceId,
				WatchHealthType.BLOOD_GLUCOSE,
				record.level.inMilligramsPerDeciliter,
				"mg/dL",
				record.time,
				record.metadata.dataOrigin.packageName,
				record.metadata.id
			)
		}
		return measurements.sortedBy { it.measuredAt }
	}

	private suspend inline fun <reified T : androidx.health.connect.client.records.Record> readRecords(
		timeRangeFilter: TimeRangeFilter
	): List<T> {
		val records = mutableListOf<T>()
		var pageToken: String? = null
		do {
			val response = client.readRecords(
				ReadRecordsRequest(
					recordType = T::class,
					timeRangeFilter = timeRangeFilter,
					pageSize = 500,
					pageToken = pageToken
				)
			)
			records += response.records
			pageToken = response.pageToken
		} while (pageToken != null)
		return records
	}

	companion object {
		const val PROVIDER_PACKAGE_NAME = "com.google.android.apps.healthdata"
	}
}
