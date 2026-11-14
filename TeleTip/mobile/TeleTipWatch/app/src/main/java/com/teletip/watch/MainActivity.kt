package com.teletip.watch

import android.app.Activity
import android.os.Bundle
import android.widget.LinearLayout
import android.widget.TextView

class MainActivity : Activity() {
	override fun onCreate(savedInstanceState: Bundle?) {
		super.onCreate(savedInstanceState)

		val status = TextView(this).apply {
			text = "TeleTip Watch hazır. Samsung Health izinleri ve MQTT ayarları bekleniyor."
			setPadding(24, 24, 24, 24)
		}

		setContentView(LinearLayout(this).apply {
			orientation = LinearLayout.VERTICAL
			addView(status)
		})
	}
}
