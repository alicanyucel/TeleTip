import json
import unittest
from argparse import Namespace

from teletip_mqtt_publisher import make_message


class MakeMessageTests(unittest.TestCase):
	def make_args(self, **overrides):
		values = {
			"tenant": "tenant-1",
			"patient": "00000000-0000-0000-0000-000000000002",
			"device": "00000000-0000-0000-0000-000000000003",
			"measurement_type": "HeartRate",
			"value": 72.0,
			"unit": "bpm",
			"measured_at": None,
			"source": "Linux IoT Gateway",
		}
		values.update(overrides)
		return Namespace(**values)

	def test_builds_backend_compatible_topic_and_payload(self):
		topic, body = make_message(self.make_args())

		self.assertEqual(
			"teletip/tenants/tenant-1/patients/00000000-0000-0000-0000-000000000002/measurements",
			topic,
		)
		message = json.loads(body)
		self.assertEqual(
			{
				"deviceId": "00000000-0000-0000-0000-000000000003",
				"type": "HeartRate",
				"value": 72.0,
				"unit": "bpm",
				"source": "Linux IoT Gateway",
			},
			{key: value for key, value in message.items() if key != "measuredAt"},
		)
		self.assertTrue(message["measuredAt"].endswith("Z"))

	def test_converts_timestamp_to_utc(self):
		_, body = make_message(self.make_args(measured_at="2026-11-16T09:30:00-05:00"))

		self.assertEqual("2026-11-16T14:30:00Z", json.loads(body)["measuredAt"])

	def test_rejects_topic_injection_and_non_finite_measurements(self):
		with self.assertRaises(ValueError):
			make_message(self.make_args(tenant="tenant/#"))
		with self.assertRaises(ValueError):
			make_message(self.make_args(value=float("nan")))


if __name__ == "__main__":
	unittest.main()
