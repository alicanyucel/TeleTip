package com.teletip.watch

import android.app.Activity
import android.os.Bundle
import android.widget.TextView

class PermissionsRationaleActivity : Activity() {
	override fun onCreate(savedInstanceState: Bundle?) {
		super.onCreate(savedInstanceState)
		setContentView(TextView(this).apply {
			text = "TeleTip, yalnızca seçtiğiniz Health Connect tansiyon ve glikoz kayıtlarını MQTT üzerinden yapılandırdığınız sağlık hizmetine aktarır. Kayıtlarınız reklam veya profil oluşturma amacıyla kullanılmaz. Aktarımı uygulama ayarlarından başlatabilir ve Health Connect izinlerini istediğiniz zaman geri alabilirsiniz."
			textSize = 16f
			setPadding(24, 24, 24, 24)
		})
	}
}
