plugins {
	id("com.android.application")
	id("org.jetbrains.kotlin.android")
}

android {
	namespace = "com.teletip.watch"
	compileSdk = 36

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
	implementation("androidx.activity:activity-ktx:1.10.0")
	implementation("androidx.core:core-ktx:1.15.0")
	implementation("androidx.health:health-services-client:1.1.0")
	implementation("androidx.health.connect:connect-client:1.1.0")
	implementation("androidx.lifecycle:lifecycle-runtime-ktx:2.8.7")
	implementation("androidx.security:security-crypto:1.1.0")
	implementation("com.google.guava:guava:33.3.1-android")
	implementation("org.eclipse.paho:org.eclipse.paho.android.service:1.1.1")
	implementation("org.eclipse.paho:org.eclipse.paho.client.mqttv3:1.2.5")
}
