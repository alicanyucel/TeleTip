plugins {
	id("com.android.application")
	id("org.jetbrains.kotlin.android")
}

android {
	namespace = "com.teletip.watch"
	compileSdk = 35

	defaultConfig {
		applicationId = "com.teletip.watch"
		minSdk = 30
		targetSdk = 35
		versionCode = 1
		versionName = "1.0"
	}
}

kotlin {
	jvmToolchain(17)
}

dependencies {
	implementation("androidx.core:core-ktx:1.15.0")
	implementation("androidx.health:health-services-client:1.1.0")
	implementation("com.google.guava:guava:33.3.1-android")
	implementation("org.eclipse.paho:org.eclipse.paho.android.service:1.1.1")
	implementation("org.eclipse.paho:org.eclipse.paho.client.mqttv3:1.2.5")
}
