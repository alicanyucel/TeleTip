#!/usr/bin/env python3
"""Publish a Linux gateway health measurement to TeleTip over MQTT."""

import argparse
import json
import math
import os
import sys
import uuid
from datetime import datetime, timezone

import paho.mqtt.client as mqtt

MEASUREMENT_TYPES = (
	"HeartRate",
	"BloodPressure",
	"BloodGlucose",
	"OxygenSaturation",
	"BodyTemperature",
)


def parse_args():
	parser = argparse.ArgumentParser(description=__doc__)
	parser.add_argument("--host", default=os.getenv("TELETIP_MQTT_HOST", "127.0.0.1"))
	parser.add_argument("--port", type=int, default=int(os.getenv("TELETIP_MQTT_PORT", "1883")))
	parser.add_argument("--tenant", required=True, help="TeleTip tenant ID")
	parser.add_argument("--patient", required=True, help="Patient UUID")
	parser.add_argument("--device", required=True, help="Sensor/gateway UUID")
	parser.add_argument("--type", required=True, choices=MEASUREMENT_TYPES, dest="measurement_type")
	parser.add_argument("--value", required=True, type=float)
	parser.add_argument("--unit", required=True)
	parser.add_argument("--measured-at", help="ISO-8601 timestamp; defaults to current UTC time")
	parser.add_argument("--source", default="Linux IoT Gateway")
	parser.add_argument("--username", default=os.getenv("TELETIP_MQTT_USERNAME"))
	parser.add_argument("--password", default=os.getenv("TELETIP_MQTT_PASSWORD"))
	parser.add_argument(
		"--tls-ca",
		default=os.getenv("TELETIP_MQTT_TLS_CA"),
		help="CA certificate path; enables TLS when provided",
	)
	return parser.parse_args()


def make_message(args):
	if not args.tenant or any(char in args.tenant for char in "/+#"):
		raise ValueError("Tenant ID must be a non-empty MQTT topic segment.")
	if not math.isfinite(args.value) or args.value < 0:
		raise ValueError("Measurement value must be finite and non-negative.")
	if not args.unit.strip():
		raise ValueError("Measurement unit cannot be empty.")

	patient_id = uuid.UUID(args.patient)
	device_id = uuid.UUID(args.device)
	if patient_id.int == 0 or device_id.int == 0:
		raise ValueError("Patient and device UUIDs must not be empty.")

	if args.measured_at:
		timestamp_text = args.measured_at
		if timestamp_text.endswith("Z"):
			timestamp_text = timestamp_text[:-1] + "+00:00"
		measured_at = datetime.fromisoformat(timestamp_text)
		if measured_at.tzinfo is None:
			raise ValueError("Measurement timestamp must include a timezone.")
		measured_at = measured_at.astimezone(timezone.utc)
	else:
		measured_at = datetime.now(timezone.utc)

	topic = f"teletip/tenants/{args.tenant}/patients/{patient_id}/measurements"
	payload = {
		"deviceId": str(device_id),
		"type": args.measurement_type,
		"value": args.value,
		"unit": args.unit.strip(),
		"measuredAt": measured_at.isoformat(timespec="seconds").replace("+00:00", "Z"),
		"source": args.source,
	}
	return topic, json.dumps(payload, separators=(",", ":"))


def main():
	args = parse_args()
	try:
		topic, payload = make_message(args)
		client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2)
		if args.username:
			client.username_pw_set(args.username, args.password or "")
		if args.tls_ca:
			client.tls_set(ca_certs=args.tls_ca)

		client.connect(args.host, args.port, keepalive=30)
		client.loop_start()
		try:
			result = client.publish(topic, payload, qos=1)
			if result.rc != mqtt.MQTT_ERR_SUCCESS:
				raise RuntimeError(f"MQTT publish failed with result code {result.rc}.")
			result.wait_for_publish(timeout=10)
			if not result.is_published():
				raise TimeoutError("MQTT broker did not acknowledge the measurement.")
		finally:
			client.disconnect()
			client.loop_stop()

		print(f"Measurement published to {topic}")
	except (OSError, ValueError, RuntimeError, TimeoutError, mqtt.MQTTException) as error:
		print(f"Failed to publish measurement: {error}", file=sys.stderr)
		return 1
	return 0


if __name__ == "__main__":
	sys.exit(main())
