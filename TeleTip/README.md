# TeleTip

[![CI/CD](https://github.com/alicanyucel/TeleTip/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/alicanyucel/TeleTip/actions/workflows/ci-cd.yml)
[![codecov](https://codecov.io/gh/alicanyucel/TeleTip/branch/master/graph/badge.svg)](https://codecov.io/gh/alicanyucel/TeleTip)

TeleTip, yapay zekâ destekli uzaktan sağlık hizmetleri için geliştirilen bir .NET Web API projesidir. Sistem; hasta ve doktor yönetimi, görüntülü görüşme, gerçek zamanlı chat, IoT sağlık verileri, AI analiz modeli ve blockchain üzerinde sağlık kaydı doğrulama altyapısı içerir.

Projenin amacı; hastaların sağlık verilerini güvenli şekilde toplamak, doktorların bu verilere erişmesini kolaylaştırmak ve uzaktan sağlık hizmetlerini tek bir platformda sunmaktır.

## İçindekiler

- [Teknolojiler](#teknolojiler)
- [Mimari](#mimari)
- [Gereksinimler](#gereksinimler)
- [Linux ve Gömülü Linux Dağıtımı](#linux-ve-gömülü-linux-dağıtımı)
- [Kurulum](#kurulum)
- [Veritabanı ve Code First](#veritabanı-ve-code-first)
- [Uygulamayı Çalıştırma](#uygulamayı-çalıştırma)
- [Swagger Kullanımı](#swagger-kullanımı)
- [API Kullanımı](#api-kullanımı)
- [Görüntülü Görüşme ve Chat](#görüntülü-görüşme-ve-chat)
- [Blockchain](#blockchain)
- [IoT ve AI](#iot-ve-ai)
- [Samsung Galaxy Watch 8](#samsung-galaxy-watch-8)
- [Güvenlik](#güvenlik)
- [KVKK ve GDPR](#kvkk-ve-gdpr)
- [Proje Yapısı](#proje-yapısı)
- [Test ve Codecov Coverage](#test-ve-codecov-coverage)

## Teknolojiler

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10
- SQL Server
- MediatR ve CQRS
- SignalR
- WebRTC
- JWT Authentication ve Authorization
- Swagger / Swashbuckle
- Nethereum Web3
- Ethereum/EVM uyumlu blockchain
- Hardhat local blockchain
- Redis
- RabbitMQ ve MassTransit
- Serilog
- OpenTelemetry
- Health Checks
- Docker Compose
- MQTT / Mosquitto
- Wear OS ve Kotlin
- Samsung Health entegrasyon sınırı

## Mimari

Çözüm aşağıdaki katmanlardan oluşur:

- **TeleTip.Domain**: Entity’ler, domain kuralları, result modeli ve repository abstraction’ları.
- **TeleTip.Application**: CQRS command/query’leri, handler’lar, DTO’lar ve uygulama abstraction’ları.
- **TeleTip.Infrastructure**: EF Core DbContext, repository’ler, SQL Server, Redis, RabbitMQ ve blockchain entegrasyonları.
- **TeleTip.WebAPI**: Controller’lar, SignalR hub’ı, Swagger, JWT ve WebRTC istemcisi.
- **TeleTip.Tests**: Test projesi.

## Gereksinimler

- Visual Studio 2026 veya .NET 10 SDK
- SQL Server Express veya SQL Server
- İsteğe bağlı: Docker Desktop
- Wear OS uygulaması için Android Studio, Android SDK ve JDK 17
- Blockchain için Node.js ve npm
- Kamera ve mikrofon erişimi için HTTPS veya localhost

## Linux ve Gömülü Linux Dağıtımı

Web API, .NET 10 Linux container'ı olarak `linux/amd64` ve `linux/arm64` üzerinde çalışacak şekilde paketlenmiştir. ARM64, 64-bit Raspberry Pi OS ve benzeri gömülü Linux cihazları için uygundur. Cihazın Docker Engine ve Docker Compose eklentisini çalıştırabilmesi ve yeterli belleğe/depolama alanına sahip olması gerekir.

> SQL Server Linux container'ı ARM64 üzerinde desteklenmediğinden, gömülü cihaz dağıtımında SQL Server'ı x64 Linux sunucuda veya başka bir erişilebilir SQL Server ortamında çalıştırın. Linux masaüstü MAUI uygulama hedefi bu projede bulunmuyor; cihazlar MQTT üzerinden TeleTip API'ye veri gönderebilir.

Linux ARM64 cihazında `TeleTip` klasöründe örnek ortam dosyasını kopyalayın ve gerçek değerlerle güncelleyin:

```bash
cp .env.example .env
```

`.env` içindeki SQL Server bağlantısı uzak veritabanınıza erişmeli; RabbitMQ ve JWT secret değerlerini de değiştirin:

```dotenv
TELETIP_SQLSERVER_CONNECTION="Server=db.example.local;Database=TeleTip;User Id=teletip;Password=replace-with-a-secret;Encrypt=True;TrustServerCertificate=True"
RABBITMQ_DEFAULT_PASS="replace-with-a-secret"
TELETIP_JWT_SECRET="replace-with-at-least-64-random-characters"
```

Embedded compose dosyası API, Redis, RabbitMQ ve MQTT broker'ı cihazda başlatır; API'nin veritabanı bağlantısını `.env` dosyasından alır:

```bash
docker compose -f docker-compose.embedded.yml up --build -d
```

API `http://<cihaz-adresi>:5000` adresinde, MQTT broker `1883` portunda dinler. Mosquitto'nun örnek yapılandırması kimlik doğrulamasızdır; sağlık verileri içeren production dağıtımında broker erişimini TLS, kimlik doğrulama ve topic yetkilendirmesiyle koruyun.

Linux x64 veya ARM64 için çok mimarili imaj oluşturup registry'ye yayımlamak için Buildx kullanın:

```bash
docker buildx build --platform linux/amd64,linux/arm64 -t <registry>/teletip-api:latest --push .
```

## Kurulum

Repository’yi klonlayın:

```powershell
git clone https://github.com/alicanyucel/TeleTip.git
cd TeleTip
```

NuGet paketlerini yükleyin ve solution’ı derleyin:

```powershell
dotnet restore
dotnet build
```

## Yapılandırma

SQL Server bağlantısı aşağıdaki dosyada bulunur:

```text
src/TeleTip.WebAPI/appsettings.json
```

Örnek:

```json
{
  "ConnectionStrings": {
	"SqlServer": "Data Source=DESKTOP-L6NJT48\\SQLEXPRESS;Initial Catalog=TeleTip;Integrated Security=True;Trust Server Certificate=True",
	"Redis": "localhost:6379",
	"RabbitMQ": "amqp://guest:guest@localhost:5672"
  }
}
```

Production ortamında connection string ve secret değerlerini `appsettings.json` yerine environment variable, User Secrets veya güvenli bir secret manager ile sağlayın.

## Veritabanı ve Code First

Proje EF Core Code First kullanır. Uygulama başlarken bekleyen migration’lar otomatik uygulanır.

Migration oluşturmak için:

```powershell
dotnet ef migrations add MigrationName `
  --project .\src\TeleTip.Infrastructure\TeleTip.Infrastructure.csproj `
  --startup-project .\src\TeleTip.WebAPI\TeleTip.WebAPI.csproj `
  --output-dir Persistence/Migrations
```

Migration’ı elle uygulamak için:

```powershell
dotnet ef database update `
  --project .\src\TeleTip.Infrastructure\TeleTip.Infrastructure.csproj `
  --startup-project .\src\TeleTip.WebAPI\TeleTip.WebAPI.csproj
```

Ana entity’ler:

- Patient
- Doctor
- TelemedicineConsultation
- IoTDevice
- HealthMeasurement
- Prescription
- MedicalReport
- AiAnalysis
- Notification
- ChatMessage

## Uygulamayı Çalıştırma

```powershell
dotnet run --project .\src\TeleTip.WebAPI\TeleTip.WebAPI.csproj
```

Adresler:

```text
HTTPS: https://localhost:7139
HTTP:  http://localhost:5270
Swagger: https://localhost:7139/swagger
```

Redis, RabbitMQ ve MQTT broker kullanılıyorsa Docker servislerini başlatabilirsiniz:

```powershell
docker compose up -d redis rabbitmq mosquitto
```

## Swagger Kullanımı

1. `https://localhost:7139/swagger` adresini açın.
2. Sağ üstteki **Authorize** butonuna tıklayın.
3. JWT token’ı girin.
4. **Authorize** butonuna basın.

Swagger Bearer header’ını otomatik oluşturur:

```http
Authorization: Bearer <JWT_TOKEN>
```

Token olmadan korumalı endpoint’ler `401 Unauthorized` döndürür.

Doctor/Patient yetkilendirmesinde JWT aşağıdaki claim’leri içermelidir:

```json
{
  "sub": "doktor-veya-hasta-entity-guid",
  "role": "Doctor"
}
```

`role` değeri `Doctor` veya `Patient` olmalı; `sub` ilgili doktor/hasta kaydının GUID kimliğiyle eşleşmelidir. JWT üretimi uygulama dışında yapılıyorsa bu claim’leri token’a ekleyin. Görüşme ve chat erişimi ayrıca kullanıcının ilgili danışmadaki doktor veya hasta olarak atanmış olmasını gerektirir; istemciden gönderilen rol bilgisi kullanılmaz.

| İşlem | Doktor | Hasta |
|---|---|---|
| Hasta listesini görüntüleme | Yalnızca atanmış hastalar | İzin verilmez |
| Hasta detayını görüntüleme | Atanmış hastalar | Yalnızca kendi kaydı |
| Görüşme ve chat geçmişi | Yalnızca katılımcısı olduğu görüşmeler | Yalnızca katılımcısı olduğu görüşmeler |
| Reçete/rapor oluşturma | Kendi kimliğiyle, atanmış hasta için | İzin verilmez |
| Cihaz ve ölçüm işlemleri | Atanmış hasta için | Yalnızca kendi kaydı için |

Diğer hasta sağlık verileri de kullanıcı kimliğine ve hasta-doktor atamasına göre filtrelenir. İzin verilmeyen veya kullanıcıya atanmamış görüşme/kayıtlar için ilgili endpoint kaydı döndürmez.

## API Kullanımı

Ana API adresi:

```text
/api/telecare
```

Örnek endpoint’ler:

```text
GET  /api/telecare/patients
GET  /api/telecare/patients/{id}
POST /api/telecare/patients

GET  /api/telecare/doctors
GET  /api/telecare/doctors/{id}
POST /api/telecare/doctors

GET  /api/telecare/consultations
GET  /api/telecare/consultations/{id}
POST /api/telecare/consultations

GET  /api/telecare/devices
POST /api/telecare/devices

GET  /api/telecare/measurements
POST /api/telecare/measurements

GET  /api/telecare/prescriptions
POST /api/telecare/prescriptions

GET  /api/telecare/reports
POST /api/telecare/reports

GET  /api/telecare/analyses
POST /api/telecare/analyses

GET  /api/telecare/notifications
POST /api/telecare/notifications

GET  /api/telecare/chat-messages?consultationId={guid}&page=1&pageSize=50
GET  /api/telecare/chat-messages/{messageId}
POST /api/telecare/chat-messages
GET  /api/telecare/ice-servers?consultationId={guid}
```

Chat geçmişi yalnızca görüşmenin atanmış doktoru veya hastası tarafından sorgulanabilir. `senderRole` (`Doctor`/`Patient`), `sentAfter` ve `sentBefore` (UTC ISO 8601) filtreleri desteklenir. `page` 1’den başlar; `pageSize` 1–100 arasındadır. Sonuçlar gönderim zamanına göre sıralanır ve `items`, `page`, `pageSize`, `totalCount` alanlarını içerir.

Örnek:

```text
GET /api/telecare/chat-messages?consultationId={guid}&senderRole=Doctor&sentAfter=2026-01-01T00:00:00Z&page=1&pageSize=20
```

Chat mesajı oluştururken yalnızca `consultationId` ve `message` gönderilir; gönderen kimliği ve rolü JWT’den alınır.

Mesaj gönderme isteği:

```http
POST /api/telecare/chat-messages
Content-Type: application/json
Authorization: Bearer <JWT>
```

```json
{
  "consultationId": "00000000-0000-0000-0000-000000000001",
  "message": "Merhaba, ölçüm sonuçlarımı paylaşmak istiyorum."
}
```

Geçmiş sorgusu `items`, `page`, `pageSize` ve `totalCount` alanları olan bir sayfa döndürür. Tarih filtreleri UTC ISO 8601 biçiminde gönderilmelidir; aralık başlangıç ve bitiş değerleri dahil edilir.

Örnek hasta oluşturma isteği:

```json
{
  "firstName": "Ali",
  "lastName": "Yılmaz",
  "dateOfBirth": "1990-01-15",
  "phoneNumber": "+90 555 000 00 00",
  "email": "ali@example.com"
}
```

## Görüntülü Görüşme ve Chat

Görüntülü görüşme test sayfası:

```text
https://localhost:7139/consultation.html
```

Gerekli işlemler:

1. API’yi çalıştırın.
2. İki farklı tarayıcı sekmesinde görüşme sayfasını açın.
3. Her iki sekmede aynı consultation GUID kullanın.
4. Atanmış doktorun ve hastanın kendi JWT token’ını girin. Token’daki `sub` ilgili doktor/hasta GUID’si, `role` ise `Doctor` veya `Patient` olmalıdır.
5. Kamera ve mikrofon izinlerini verin. Katılımcı rolü istemci seçiminden değil, doğrulanmış JWT claim’lerinden belirlenir.

SignalR hub adresi:

```text
/hubs/consultation
```

WebRTC medya akışı peer-to-peer çalışır. SignalR yalnızca signaling işlemlerini yapar:

- Offer
- Answer
- ICE Candidate
- Görüşmeye katılma/ayrılma
- Chat mesajları

### TURN server yapılandırması

Uygulama, TURN sunucusu ayarlandığında istemciye kısa ömürlü kimlik bilgileri sunar. TURN REST API/shared-secret doğrulamasını destekleyen Coturn benzeri bir sunucu kullanın; uzun ömürlü statik TURN kullanıcı adı/parolası kullanılmaz. Değerleri kaynak koda veya `appsettings.json` dosyasına koymayın:

```powershell
$env:Turn__Urls__0 = "turn:turn.example.com:3478"
$env:Turn__Urls__1 = "turns:turn.example.com:5349"
$env:Turn__SharedSecret = "TURN sunucusuyla paylaşılan gizli anahtar"
$env:Turn__CredentialTtlMinutes = "60"
```

Coturn tarafında `use-auth-secret` ve aynı shared secret yapılandırılmalıdır. API’deki `GET /api/telecare/ice-servers?consultationId={guid}` endpoint’i yalnızca ilgili görüşmenin doktoru/hastası için ICE sunucularını ve süresi dolan HMAC-SHA1 TURN REST kimlik bilgilerini döndürür. Kullanıcı adı, son kullanma Unix zamanını ve kullanıcı GUID’sini içerir; TURN sunucusu Coturn’un REST secret doğrulama yöntemiyle aynı secret’ı kullanmalıdır. `Turn:Urls` boş bırakılırsa TURN sunucusu yanıt listesine eklenmez. TURN ayarı yapılmadığında örnek istemci STUN ile çalışmayı dener; farklı ağlarda bağlantı garantisi vermez. Üretimde TLS, güvenlik duvarı, TURN kota/izleme ayarları ve düzenli secret rotasyonu ayrıca yapılandırılmalıdır. Secret’ı terminal geçmişine yazmayın; deployment secret store veya güvenli ortam değişkeni kullanın.

Görüşme sırasında chat mesajları veritabanına kaydedilir. Örnek sayfa, gönderen rolü ve zaman aralığı filtreleriyle geçmişi sayfalı olarak yükleyebilir.

## Blockchain

Blockchain entegrasyonu, sağlık kayıtlarının doğrulanabilir hash kaydını tutar. Hassas hasta verisi blockchain’e gönderilmez.

Yalnızca aşağıdaki bilgiler kullanılır:

- SHA-256 kayıt hash’i
- Kayıt kimliği hash’i
- Transaction hash’i
- Block number
- Kayıt zamanı

### Local Hardhat ağı

```powershell
cd blockchain
npm install
npm run node
```

Başka bir terminalde sözleşmeyi deploy edin:

```powershell
cd blockchain
npm run deploy:local
```

Contract adresini User Secrets ile kaydedin:

```powershell
dotnet user-secrets init --project .\src\TeleTip.WebAPI\TeleTip.WebAPI.csproj

dotnet user-secrets set "Blockchain:ContractAddress" "0x..." `
  --project .\src\TeleTip.WebAPI\TeleTip.WebAPI.csproj

dotnet user-secrets set "Blockchain:PrivateKey" "0x..." `
  --project .\src\TeleTip.WebAPI\TeleTip.WebAPI.csproj
```

Blockchain ayarları:

```json
{
  "Blockchain": {
	"RpcUrl": "http://127.0.0.1:8545",
	"ChainId": 31337,
	"ContractAddress": "",
	"PrivateKey": ""
  }
}
```

Private key’i kesinlikle Git repository’sine göndermeyin.

## IoT ve AI

TeleTip, sağlık ölçümlerini `HealthMeasurement` modeli üzerinden saklar. Ölçüm kaynağı; akıllı saat, tansiyon cihazı, glikometre veya başka bir tıbbi cihaz olabilir.

Galaxy Watch 8 entegrasyonu için Wear OS Kotlin uygulaması ve MQTT tabanlı backend tüketicisi eklenmiştir. MQTT mesajları doğrulandıktan sonra tenant ve hasta bilgileriyle birlikte SQL Server’daki `HealthMeasurements` tablosuna kaydedilir.

Ollama üzerinden çalışan yerel AI analiz motoru; hastanın son sağlık ölçümlerini (en fazla 100 kayıt) analiz eder ve sonucu veritabanındaki `AiAnalysis` kaydına ekler. `POST /api/telecare/analyses/generate` yalnızca doktor rolüne açıktır; doktorun hastaya atanmış olması ve isteğe bağlı görüşmenin aynı doktor/hastaya ait olması doğrulanır. İstek gövdesi:

```json
{
  "patientId": "00000000-0000-0000-0000-000000000002",
  "consultationId": null,
  "type": "General"
}
```

Desteklenen analiz türleri `General`, `RiskDetection` ve `DoctorAlert` şeklindedir. Başarı yanıtı analiz kaydının kimliğini döndürür; analiz özeti ve modelin 0–1 aralığındaki risk dikkati göstergesi `GET /api/telecare/analyses/{id}` ile alınabilir. Doktor erişimi aynı hasta kapsamı kurallarına tabidir.

Yerel API çalışırken Ollama'yı `http://localhost:11434` adresinde başlatın, ardından modeli indirin ve API'yi çalıştırın:

```powershell
ollama pull qwen2.5:7b
dotnet run --project .\src\TeleTip.WebAPI\TeleTip.WebAPI.csproj
```

Farklı bir model veya Ollama adresi için `Ollama__Model` ve `Ollama__BaseUrl` ortam değişkenlerini kullanabilirsiniz. Compose ile isteğe bağlı AI servisini başlatmak için proje kökünde:

```powershell
docker compose --profile ai up -d
docker exec teletip-ollama ollama pull qwen2.5:7b
```

Compose yapılandırması API'yi `http://ollama:11434` adresine yönlendirir; model indirildikten sonra analiz endpoint'i kullanılabilir. Modelin indirilmesi ve çalıştırılması ciddi disk/RAM/CPU/GPU kaynağı gerektirebilir; servis varsayılan Compose profilinde kapalıdır.

Bu özellik klinik olarak doğrulanmış bir tanı veya puanlama sistemi değildir. Model çıktısı yalnızca karar desteği amaçlıdır ve doktor değerlendirmesinin yerine geçmez. Yalnızca atanmış doktorlar analiz başlatabilir; sağlık verileri yapılandırdığınız Ollama sunucusuna gönderilir. Sağlık verisi mevzuatı, erişim güvenliği ve model sağlayıcısı/veri saklama koşulları üretim kullanımından önce ayrıca değerlendirilmelidir.

## Samsung Galaxy Watch 8 ve MQTT

Wear OS başlangıç uygulaması şu konumdadır:

```text
mobile/TeleTipWatch
```

Galaxy Watch 8 uygulaması Health Services üzerinden canlı nabız alır. Android Studio'dan `mobile/TeleTipWatch` projesini açıp `app` modülünü saate kurun. Saati Wi-Fi üzerinden erişebilen bir MQTT broker'a bağlayın; geliştirmede broker adresi `tcp://<broker-LAN-IP>:1883`, TLS broker'da `ssl://<broker-host>:8883` olabilir. Uygulamada broker adresini, tenant ID ve hasta UUID'sini girip önce MQTT bağlantısını kurun, ardından nabız ölçümünü başlatın. Android'in sensör izni sorusunu onaylayın ve canlı ölçüm süresince uygulamayı açık tutun. Ölçüm, her nabız örneğinde backend'e yayınlanır.

> `127.0.0.1` veya `localhost` saatten erişilebilen broker adresi değildir; broker'ın LAN adresini kullanın. MQTT broker erişim bilgileri uygulama ekranından girilebilir. Sağlık verisini gerçek ortamda yalnızca TLS ve kimlik doğrulamalı bir broker üzerinden gönderin.

Backend, Mosquitto MQTT broker üzerinden aşağıdaki topic filtresini dinler:

```text
teletip/tenants/+/patients/+/measurements
```

Saat veya ara cihazın yayınlaması gereken topic formatı:

```text
teletip/tenants/{tenantId}/patients/{patientId}/measurements
```

Örnek MQTT payload:

```json
{
  "deviceId": "00000000-0000-0000-0000-000000000001",
  "type": "HeartRate",
  "value": 72,
  "unit": "bpm",
  "measuredAt": "2026-01-01T12:00:00Z",
  "source": "Samsung Galaxy Watch 8"
}
```

MQTT yapılandırması:

```json
{
  "Mqtt": {
	"Host": "127.0.0.1",
	"Port": 1883,
	"ClientId": "teletip-api",
	"Username": "",
	"Password": "",
	"TopicFilter": "teletip/tenants/+/patients/+/measurements"
  }
}
```

Local broker’ı başlatmak ve API’yi çalıştırmak için:

```powershell
docker compose up -d mosquitto
dotnet run --project .\src\TeleTip.WebAPI\TeleTip.WebAPI.csproj
```

MQTT tüketicisi aşağıdaki dosyada bulunur:

```text
src/TeleTip.Infrastructure/Messaging/Mqtt/GalaxyWatchMqttConsumer.cs
```

### Linux IoT gateway

Linux tabanlı bir cihaz (ör. Raspberry Pi veya Linux gateway), yerel sensör uygulamasından aldığı ölçümü aşağıdaki MQTT istemcisiyle TeleTip’e gönderebilir. Bu entegrasyon MQTT aktarımını sağlar; belirli sensörlerin Linux sürücüsü veya donanım bağlantısı projeye dahil değildir. Sensör uygulaması ölçüm okuduğunda istemciyi çağırmalıdır.

İstemci dosyaları `linux/iot-gateway` altındadır. Linux cihazda Python 3 ve erişilebilir bir MQTT broker gerekir:

```bash
cd linux/iot-gateway
python3 -m venv .venv
. .venv/bin/activate
pip install -r requirements.txt
```

Broker adresini ve varsa kimlik bilgilerini ortam değişkenleriyle ayarlayın:

```bash
export TELETIP_MQTT_HOST=192.168.1.10
export TELETIP_MQTT_PORT=1883
# Gerekliyse broker hesabı:
export TELETIP_MQTT_USERNAME=linux-device
export TELETIP_MQTT_PASSWORD='your-device-password'
# TLS kullanılıyorsa broker CA sertifikası:
export TELETIP_MQTT_TLS_CA=/etc/ssl/certs/mqtt-ca.crt
```

Örnek ölçüm gönderimi:

```bash
python teletip_mqtt_publisher.py \
  --tenant tenant-1 \
  --patient 00000000-0000-0000-0000-000000000002 \
  --device 00000000-0000-0000-0000-000000000003 \
  --type HeartRate \
  --value 72 \
  --unit bpm
```

İstemci UUID biçimindeki hasta ve cihaz kimliklerini doğrular, ölçüm değerini MQTT QoS 1 ile yayınlar ve broker onayı gelmezse hata döndürür. Desteklenen ölçüm türleri: `HeartRate`, `BloodPressure`, `BloodGlucose`, `OxygenSaturation`, `BodyTemperature`, `Steps`. Tarih verilmezse UTC zamanı kullanılır; cihazın ölçüm zamanını belirtmek için `--measured-at 2026-11-16T14:30:00Z` kullanılabilir.

Docker Compose ile geliştirici ortamında broker’ı başlatmak için proje kökünde `docker compose up -d mosquitto` çalıştırın. Mesajları veritabanına kaydetmek için TeleTip API’nin de çalışıyor olması gerekir; host üzerinde çalışan API için MQTT host ayarı `127.0.0.1` olmalı, container içindeki API ise Compose’un `mosquitto` servis adını kullanır. Linux cihazdan erişim için `TELETIP_MQTT_HOST` değerine Docker’ın çalıştığı makinenin LAN IP adresini yazın. Mevcut geliştirme Mosquitto yapılandırması anonim erişime açıktır; internete veya güvenilmeyen bir ağa açmayın. Gerçek sağlık verisi taşıyan üretim ortamında TLS, cihaz başına kullanıcı/şifre ve topic yetkilendirmesi zorunludur.

### Sağlık verisi ve cihaz kısıtları

- Uzun ölçüm oturumu kullanıcı tarafından başlatılır; `ExerciseClient` bir health foreground service içinde çalışır, devam eden bildirimi gösterir ve nabız ile cihaz destekliyorsa adımları MQTT QoS 1 ile gönderir. Gerçek arka plan ve Galaxy Watch 8 doğrulaması bağlı cihaz, izin verilmiş broker ve çalışan API gerektirir.
- Adımlar `Steps` olarak kaydedilir. Bu enum'a eklenen yeni değer mevcut ölçüm türlerinin sayısal değerlerini değiştirmez.
- Tansiyon ve glikoz Galaxy Watch sensörlerinden doğrudan okunmaz. Uygulama, cihazın Health Connect ile eşitlediği son 7 günlük `BloodPressureRecord` ve `BloodGlucoseRecord` verilerini, kullanıcı izniyle alabilir.
- Health Connect yoksa veya cihaz sağlayıcıya kayıt aktarmıyorsa dış cihaz verisi alınamaz; tansiyon/glikoz cihazı ve telefon uygulamasının Health Connect senkronizasyonunu desteklemesi gerekir. Tansiyon sistolik/diastolik değerleri backend'in mevcut tek değerli modelinde `mmHg_systolic` ve `mmHg_diastolic` birimleriyle aynı ölçüm türüne ayrı kayıtlar olarak gönderilir.
- Health Connect verisinin `source` alanında kaynak uygulama paketi MQTT payload'ına eklenir. Mevcut backend entity'si bu alanı kalıcı olarak saklamaz. İçe aktarılan Health Connect kayıt kimlikleri cihazda tutulur; tekrar içe aktarma aynı kaydı yeniden göndermez.
- Hasta, tenant, broker ve MQTT kullanıcı bilgileri uygulama ayarlarında saklanır; parola Android Keystore tabanlı şifreli tercihlerde tutulur. Gerçek sağlık verisi taşıyan production broker'da TLS, kullanıcı doğrulama, cihaz başına kimlik bilgileri ve topic yetkilendirmesi zorunludur.

Wear OS uygulamasında kullanılan temel izinler:

```text
android.permission.INTERNET
android.permission.BODY_SENSORS / android.permission.health.READ_HEART_RATE
android.permission.BODY_SENSORS_BACKGROUND (API 33-35, uzun oturum için)
android.permission.FOREGROUND_SERVICE_HEALTH
android.permission.ACTIVITY_RECOGNITION
Health Connect READ_BLOOD_PRESSURE ve READ_BLOOD_GLUCOSE (kullanıcıdan çalışma anında istenir)
```

#### Galaxy Watch 8 uçtan uca doğrulama

1. `mobile/TeleTipWatch` projesini Android Studio'da açın ve `:app:assembleDebug` görevini çalıştırın. APK `mobile/TeleTipWatch/app/build/outputs/apk/debug/app-debug.apk` konumuna yazılır.
2. APK'yı Wear OS 6 yüklü gerçek Galaxy Watch 8'e kurun. API ve MQTT broker'ı saatin erişebildiği bir ağ adresine açın; uygulamaya geçerli tenant ID, hasta UUID'si, broker ve gerekiyorsa kullanıcı bilgilerini girip **Ayarları kaydet**'e basın.
3. **MQTT bağlantısını test et** başarılı olmadan ölçümü başlatmayın. İlk çalıştırmada nabız, aktivite tanıma, bildirim ve arka plan sensör izinlerini verin.
4. **Nabız ve adım ölçümünü başlat** seçin. Sürekli bildirimin görünmesini doğrulayın, saati ekranı kapalı şekilde en az birkaç dakika kullanın ve ölçüm sırasında nabız sensörünün veri üretmesi için saati bileğe takın.
5. Broker'da `teletip/tenants/<tenant>/patients/<hasta-uuid>/measurements` konusunu izleyin (`mosquitto_sub -h <broker> -t 'teletip/tenants/+/patients/+/measurements' -v`). `HeartRate` ve cihaz adım verisini sağlıyorsa `Steps` JSON mesajlarının geldiğini; API/DB tarafında hasta ve cihazla ilişkilendirildiğini doğrulayın.
6. Bildirimdeki **Durdur** eylemini kullanıp yeni mesajların kesildiğini doğrulayın. Ölçüm sürerken saati yeniden başlatma veya bağlantıyı kesme gibi uzun süreli kararlılık kontrollerini ayrıca gerçekleştirin.
7. Harici tansiyon/glikoz cihazı önce kendi uygulamasıyla Health Connect'e veri yazmalıdır. Sağlayıcı saatte mevcutsa uygulamadaki Health Connect aktarımını seçin, izin verin ve broker/API'de `BloodPressure` (sistolik ve diyastolik ayrı satırlar) veya `BloodGlucose` ölçümlerini doğrulayın. Wear OS cihazında Health Connect yoksa veri saatin kendisinden okunamaz; üretici SDK'sı veya telefon eşlikçi uygulaması gerekir.

> Bu kaynak ortamında Galaxy Watch 8 bağlı olmadığından gerçek sensör, ekran kapalı uzun oturum ve broker/API uçtan uca doğrulaması çalıştırılamadı. Derleme başarısı cihaz testi yerine geçmez.

> Geliştirme ortamındaki Mosquitto yapılandırması anonymous bağlantıya izin verir. Production ortamında TLS, kullanıcı doğrulama, cihaz bazlı kimlik bilgileri ve topic yetkilendirmesi zorunlu tutulmalıdır.

## Güvenlik

Test projesinde Doctor/Patient JWT claim doğrulaması, TURN yapılandırması ve API erişim kontrolleri için otomatik güvenlik regresyon testleri bulunur. Yerel HTTP entegrasyon testleri kimlik doğrulamamış chat erişimini, hastanın hasta listesini veya başka bir hastanın kaydını istemesini, doktorun atanmadığı görüşmenin chat geçmişine erişmesini ve sayfalı geçmişin görüşme kapsamını doğrular. Test auth şeması ve bellek içi veritabanı kullanılır; production JWT sağlayıcısını veya gerçek bir dağıtımı sınamaz.

Testleri çalıştırmak için:

```powershell
dotnet test .\tests\TeleTip.Tests\TeleTip.Tests.csproj --filter FullyQualifiedName~TeleCareSecurityIntegrationTests
```

Bu otomatik kontroller bağımsız penetrasyon testi değildir ve gerçek deployment üzerinde güvenlik değerlendirmesinin yerine geçmez. Production öncesi bağımsız, yetkili ve kapsamı yazılı bir penetrasyon testi yapılmalı; bulgular giderilip yeniden test edilmelidir.

- JWT authentication
- Controller seviyesinde `[Authorize]`
- SignalR hub seviyesinde `[Authorize]`
- Doktor/hasta rol politikaları ve JWT kimlik claim’leri
- Görüşme katılımcısı doğrulaması; chat geçmişi ve mesaj gönderiminde görüşme bazlı erişim kontrolü
- Tenant bazlı global query filter
- Hassas sağlık verisini blockchain’e göndermeme
- Private key’leri User Secrets/environment variable ile saklama
- HTTPS kullanımı

Production ortamında ayrıca şu önlemler alınmalıdır:

- Gerçek ve güçlü JWT secret kullanımı
- Refresh token mekanizması
- TURN sunucusunu production ortamında kurup TLS, kota ve erişim kurallarıyla güvenli yapılandırma
- Production öncesi yetkili ve kapsamı belirlenmiş bağımsız penetrasyon testi; bulguların giderilmesi ve yeniden test edilmesi
- Rate limiting ayarlarının gözden geçirilmesi
- CORS adreslerinin sadece güvenilir domain’lerle sınırlandırılması
- Secret manager kullanımı

Doktor/hasta rol politikaları ve görüşme katılımcısı bazlı erişim kontrolleri uygulanmıştır. Production’a geçmeden önce JWT sağlayıcısının `sub` ve `role` claim’lerini doğru ürettiği doğrulanmalıdır.

## KVKK ve GDPR

TeleTip sağlık verisi işleyebilecek şekilde tasarlanmıştır. Sağlık verileri özel nitelikli kişisel veri kapsamına girebildiğinden, bu bölüm bir uyum beyanı veya hukuki görüş değildir. README’de mevzuattan söz edilmesi, uygulamanın ya da işletmecisinin KVKK veya GDPR’ye uygun olduğu anlamına gelmez. Türkiye’de gerçek kullanıcı verisiyle kullanımdan önce veri sorumlusu rolündeki kurumun; kullanım amacı, veri akışları, hukuki işleme şartları ve hizmet modeline göre uzman hukuk danışmanlığı alması gerekir.

### Hangi mevzuat uygulanabilir?

- Türkiye’de kişisel veri işlenmesi bakımından 6698 sayılı Kişisel Verilerin Korunması Kanunu (KVKK) ve ilgili ikincil düzenlemeler değerlendirilmelidir. Sağlık verileri için KVKK’nın özel nitelikli kişisel verilere ilişkin güncel hükümleri ve sağlık hizmetlerine özgü mevzuat ayrıca incelenmelidir.
- GDPR Türkiye’deki her projeye otomatik olarak uygulanmaz. AB’de bir kuruluş bulunması veya AB’deki kişilere mal/hizmet sunulması ya da davranışlarının izlenmesi gibi GDPR’nin bölgesel kapsam koşulları varsa ayrıca uygulanabilir. Uygulanabilirlik somut iş modeline göre belirlenmelidir.
- TeleTip’in sağlık alanında kullanılması halinde KVKK/GDPR dışında sağlık hizmeti, uzaktan sağlık hizmeti ve tıbbi cihaz yazılımı mevzuatı gibi kurallar da gündeme gelebilir; bu README bunları değerlendirmez.

### Canlı kullanımdan önce tamamlanması gerekenler

1. **Roller, amaç ve hukuki şart:** Her veri akışı için veri sorumlusu/veri işleyen rolleri, amaç, veri kategorisi, alıcılar ve geçerli hukuki işleme şartı belirlenmelidir. Açık rıza her işleme için varsayılan veya tek hukuki dayanak değildir; gerekli olduğu yerde rıza belirli, bilgilendirilmiş ve geri alınabilir olmalıdır. Sağlık verisi işleme şartları güncel kanun metni ve somut sağlık hizmeti modeli üzerinden hukuk danışmanıyla doğrulanmalıdır.
2. **Aydınlatma ve hak talepleri:** Hasta, doktor ve cihaz kullanıcısı için anlaşılır aydınlatma metinleri; veri sahibinin başvuru, erişim, düzeltme, silme/yok etme veya kısıtlama taleplerini değerlendirecek doğrulanmış süreç ve sorumlu iletişim kanalı oluşturulmalıdır. GDPR’nin uygulandığı durumlarda GDPR kapsamındaki haklar ve başvuru süreleri de ele alınmalıdır.
3. **Saklama ve imha:** Her veri türü için mevzuata ve sağlık kayıtlarının saklama yükümlülüklerine uygun süre belirlenmeli; süre sonunda silme, yok etme veya anonimleştirme ve yedeklerden çıkarma prosedürleri tanımlanmalıdır. Hesabın silinmesi tıbbi kayıt saklama yükümlülüklerini kendiliğinden ortadan kaldırmaz.
4. **Güvenlik ve kayıtlar:** Yetki en az ayrıcalıkla sınırlandırılmalı; aktarım ve saklama şifrelemesi, anahtar yönetimi, erişim denetim kayıtları, yedekleme, zafiyet yönetimi ve olay müdahalesi üretim ortamında uygulanıp test edilmelidir. Log ve izleme sistemlerine hasta adı, sağlık ölçümü, MQTT payload’ı, token veya sır yazılmamalıdır.
5. **Tedarikçiler ve yurt dışı aktarım:** Bulut, hata izleme/telemetri, mesajlaşma, yedekleme, görüntülü görüşme ve blockchain sağlayıcılarının işlediği veriler ve veri konumları envanterlenmelidir. KVKK’nın güncel yurt dışına aktarım hükümleri ve GDPR uygulanıyorsa GDPR Bölüm V kapsamındaki şartlar, her aktarım için ayrıca değerlendirilmelidir.
6. **Etki ve bildirim süreçleri:** İşleme faaliyetine göre veri koruma etki değerlendirmesi ihtiyacı, kayıt yükümlülükleri (KVKK kapsamında VERBİS yükümlülüğü dahil, uygulanıyorsa), ihlal tespiti ve ilgili makamlara/kişilere bildirim prosedürleri belirlenmelidir. GDPR kapsamındaki ihlal bildirim süreleri ve istisnaları ayrıca gözetilmelidir.
7. **Blockchain:** Hash veya takma adlı kayıt, başka bilgilerle kişiyle ilişkilendirilebiliyorsa kişisel veri niteliğini koruyabilir. Sağlık verisi veya kişiyi belirlenebilir kılan veriler blockchain’e yazılmamalı; her türlü zincir üstü kayıt öncesi silme, saklama, yurt dışı aktarım ve geri döndürülemezlik etkileri hukuk ve güvenlik incelemesinden geçirilmelidir.

### TeleTip’in mevcut durumu

Bu repository bir yazılım temelidir; tek başına kurumsal uyum programı değildir. Kurumun veri sorumlusu bilgileri, amaç/hukuki şart envanteri, yayımlanmış aydınlatma ve gerekli rıza akışları, saklama-imha takvimi, veri sahibi talepleri, ihlal müdahalesi, tedarikçi sözleşmeleri ve yurt dışı aktarım değerlendirmesi bu dokümantasyondan sağlanmaz. Üretim ortamı, kullanıcı arayüzü, cihazlar ve dış hizmetlerle birlikte ayrıca tasarlanıp doğrulanmalıdır. Geliştirme Mosquitto yapılandırmasının anonim erişime açık olması nedeniyle gerçek sağlık verisiyle güvenli/uyumlu üretim kullanımı için uygun değildir.

### Resmî kaynaklar

- [6698 sayılı KVKK — Mevzuat Bilgi Sistemi](https://www.mevzuat.gov.tr/mevzuatmetin/1.5.6698.pdf)
- [Kişisel Verileri Koruma Kurumu — özel nitelikli kişisel verilerin işlenmesi](https://www.kvkk.gov.tr/)
- [GDPR (Regulation (EU) 2016/679) — EUR-Lex](https://eur-lex.europa.eu/eli/reg/2016/679/oj)

## Proje Yapısı

```text
TeleTip/
├── src/
│   ├── TeleTip.Domain/
│   ├── TeleTip.Application/
│   ├── TeleTip.Infrastructure/
│   └── TeleTip.WebAPI/
├── tests/
│   └── TeleTip.Tests/
├── blockchain/
│   ├── contracts/
│   ├── scripts/
│   ├── hardhat.config.cjs
│   └── package.json
├── docker-compose.yml
└── TeleTip.slnx
```

## Derleme ve Test

Solution’ı derlemek için:

```powershell
dotnet build
```

Testleri çalıştırmak için:

```powershell
dotnet test
```

## Test ve Codecov Coverage

Projede xUnit, FluentAssertions ve NSubstitute kullanılır. Unit testler domain entity’lerini, CQRS command/query handler’larını, validasyonları ve repository etkileşimlerini kapsar.

Testleri coverage raporuyla çalıştırmak için:

```powershell
dotnet test .\tests\TeleTip.Tests\TeleTip.Tests.csproj `
  --collect:"XPlat Code Coverage" `
  --results-directory .\TestResults
```

Komut sonucunda Cobertura formatında `coverage.cobertura.xml` dosyası oluşturulur. GitHub Actions bu dosyayı Codecov’a otomatik yükler.

Coverage ayarları `codecov.yml` dosyasında bulunur:

- Project coverage hedefi: `%70`
- Patch coverage hedefi: `%70`
- Migration, test, generated, blockchain ve `wwwroot` dosyaları kapsam dışıdır.

GitHub Actions workflow dosyası:

```text
.github/workflows/ci-cd.yml
```

Codecov upload işlemi için GitHub repository secrets bölümüne aşağıdaki secret eklenmelidir:

```text
CODECOV_TOKEN
```

Coverage raporu:

```text
https://codecov.io/gh/alicanyucel/TeleTip
```

## Teknopark / Teknokent Proje Tanıtımı

### Projenin Amacı

TeleTip; hastaların, doktorların ve sağlık kuruluşlarının uzaktan sağlık hizmetlerini güvenli ve düzenli şekilde yürütebilmesi amacıyla geliştirilen dijital sağlık platformudur. Proje; hasta ve doktor yönetimi, görüntülü görüşme, gerçek zamanlı mesajlaşma, IoT sağlık verileri, yapay zekâ destekli analiz ve blockchain tabanlı kayıt doğrulama altyapılarını tek bir sistemde birleştirir.

TeleTip’in amacı yalnızca görüntülü görüşme sağlamak değildir. Hastaya ait sağlık ölçümlerinin farklı cihazlardan alınması, merkezi olarak saklanması, doktor tarafından incelenmesi ve gelecekte yapay zekâ ile analiz edilmesi hedeflenmektedir. Böylece özellikle kronik hastaların, yaşlıların ve düzenli takip gerektiren kişilerin sağlık süreçleri uzaktan izlenebilir hale getirilebilir.

### Çözülmek İstenen Problem

Geleneksel sağlık süreçlerinde evde yapılan ölçümler çoğu zaman manuel olarak kaydedilir veya doktora geç ulaştırılır. Hasta bilgileri, görüşme kayıtları, ölçümler, reçeteler ve raporlar farklı ortamlarda tutulabildiği için hastanın güncel durumunu bütünsel olarak değerlendirmek zorlaşır.

TeleTip aşağıdaki sorunları azaltmayı hedefler:

- Evde yapılan sağlık ölçümlerinin düzenli takip edilememesi.
- Doktorun hastanın güncel verilerine zamanında ulaşamaması.
- Hasta ve doktor iletişiminin farklı kanallara dağılması.
- Sağlık verilerinin farklı sistemlerde tutulması.
- Anormal ölçümlerin ve riskli değişimlerin geç fark edilmesi.
- Sağlık kuruluşlarının uzaktan takip süreçlerini merkezi olarak yönetememesi.
- Kırsal bölgelerde veya ulaşımın zor olduğu durumlarda sağlık hizmetine erişim güçlüğü.

### Hedef Kullanıcılar

**Hastalar:** Kişisel sağlık bilgilerini, ölçümlerini, reçetelerini ve raporlarını takip edebilir; doktorlarıyla görüntülü görüşme ve mesajlaşma yapabilir.

**Doktorlar:** Hastaların güncel sağlık ölçümlerini inceleyebilir, uzaktan görüşme yapabilir, mesajlaşabilir ve hastanın takip sürecini yönetebilir.

**Sağlık kuruluşları:** Hasta ve doktor süreçlerini kurum bazında yönetebilir, IoT cihazlarından gelen verileri merkezi olarak izleyebilir ve dijital sağlık hizmetleri sunabilir.

**Ar-Ge ekipleri:** Yetkilendirilmiş ve anonimleştirilmiş verilerle yapay zekâ, giyilebilir cihaz ve uzaktan hasta izleme senaryoları geliştirebilir.

### Temel Modüller

1. **Hasta ve doktor yönetimi:** Hasta, doktor ve sağlık kuruluşu kayıtlarının yönetilmesi.
2. **Telekonsültasyon:** Hasta ve doktorun uzaktan görüntülü görüşme yapması.
3. **Gerçek zamanlı chat:** SignalR ile anlık mesajlaşma ve görüşme iletişimi.
4. **WebRTC görüşme altyapısı:** Ses ve görüntünün peer-to-peer iletilmesi; SignalR’ın signaling işlemlerini yürütmesi.
5. **Sağlık ölçümleri:** Kalp atışı, tansiyon, glikoz, oksijen, sıcaklık ve benzeri verilerin standart modelde saklanması.
6. **IoT cihaz yönetimi:** Akıllı saat, tansiyon cihazı, glikometre ve benzeri cihazların sisteme bağlanabilmesi.
7. **Yapay zekâ analiz altyapısı:** Risk tespiti, trend analizi, uyarı ve risk skoru üretimine uygun yapı.
8. **Reçete, rapor ve bildirim yönetimi:** Sağlık sürecine ait ek kayıtların yönetilmesi.
9. **Blockchain doğrulama:** Sağlık verisinin kendisini paylaşmadan kayıt bütünlüğünün hash ile doğrulanması.

### Samsung Galaxy Watch 8 IoT Entegrasyonu

TeleTip’in Galaxy Watch 8 entegrasyonu, Wear OS üzerinde çalışan Kotlin uygulaması ile MQTT tabanlı backend tüketicisinden oluşur. Hedeflenen veri akışı şöyledir:

```text
Galaxy Watch 8 sensörleri
		↓
Samsung Health / Health Services SDK
		↓
Wear OS Kotlin uygulaması
		↓
MQTT broker / Mosquitto
		↓
TeleTip MQTT consumer
		↓
HealthMeasurements tablosu
		↓
Doktor ve sağlık uygulamaları
```

MQTT topic formatı:

```text
teletip/tenants/{tenantId}/patients/{patientId}/measurements
```

Örnek veri:

```json
{
  "deviceId": "00000000-0000-0000-0000-000000000001",
  "type": "HeartRate",
  "value": 72,
  "unit": "bpm",
  "measuredAt": "2026-01-01T12:00:00Z",
  "source": "Samsung Galaxy Watch 8"
}
```

Backend tarafındaki `GalaxyWatchMqttConsumer`, MQTT mesajının topic bilgisini doğrular, tenant ve hasta kimliğini ayırır, payload formatını kontrol eder ve geçerli ölçümü SQL Server’daki `HealthMeasurements` tablosuna kaydeder.

Wear OS başlangıç uygulaması:

```text
mobile/TeleTipWatch
```

MQTT backend kodu:

```text
src/TeleTip.Infrastructure/Messaging/Mqtt/GalaxyWatchMqttConsumer.cs
```

Local MQTT broker’ı başlatmak için:

```powershell
docker compose up -d mosquitto
```

### Sensör Verileri ve Teknik Kısıtlar

Wear OS uygulaması, kullanıcının başlattığı uzun süreli ölçüm oturumunda Health Services `ExerciseClient` kullanır. Sağlık foreground service'i ekran kapalıyken çalışmayı sürdürür, devam eden bildirim ve durdurma eylemi sunar; desteklenen nabız ve adım verilerini MQTT ile gönderir. Sensör türü ve kullanılabilirliği saat modeline ve Health Services yeteneklerine bağlıdır.

- Hasta/tenant yapılandırma ekranında broker, tenant ID, hasta UUID'si ve MQTT kimlik bilgileri ayarlanır; ayarlar cihazda saklanır ve parola şifreli tercihlerle korunur.
- Uygulama, broker bağlantısını test edebilir; README'deki Galaxy Watch 8 kontrol listesi gerçek saatten broker ve API'ye doğrulama adımlarını açıklar.
- Tansiyon ve glikoz saatten doğrudan ölçülmez. Kullanıcının Health Connect'e eşitleyen destekli harici cihazı/uygulaması varsa son 7 günlük kayıtlar istenen izinlerle içe aktarılabilir.
- Health Connect sağlayıcısı Wear OS cihazında bulunmuyorsa saatten bu kayıtlar okunamaz; harici cihazın telefon uygulaması veya üretici entegrasyonu gerekir.
- Tansiyon Health Connect ölçümündeki sistolik ve diyastolik değerleri backend'e ayrı `BloodPressure` kayıtları olarak iletilir.
- Health Services canlı ölçüm API'si geçmiş Samsung Health verilerini okumaz; harici tansiyon/glikoz kayıtları ayrı Health Connect akışından gelir.

### Teknik Mimari

**TeleTip.Domain:** Entity’ler, domain kuralları, result/error modelleri ve repository abstraction’ları.

**TeleTip.Application:** CQRS command/query yapıları, MediatR handler’ları, DTO’lar ve validasyonlar.

**TeleTip.Infrastructure:** EF Core, SQL Server, MQTTnet, Mosquitto bağlantısı, Redis, RabbitMQ, MassTransit ve blockchain entegrasyonları.

**TeleTip.WebAPI:** ASP.NET Core controller’ları, JWT, Swagger, SignalR hub’ları, WebRTC signaling ve health check endpoint’leri.

**TeleTip.Tests:** Domain, validasyon, handler ve repository testleri.

### Güvenlik ve Gizlilik

TeleTip sağlık verileri işlediği için güvenlik ve gizlilik proje tasarımının temel unsurlarıdır. JWT authentication, authorization, tenant bazlı veri ayrımı, HTTPS, MQTT payload doğrulaması ve hassas verilerin blockchain’e gönderilmemesi temel güvenlik yaklaşımını oluşturur.

Production ortamında MQTT için TLS, kullanıcı doğrulama, cihaz bazlı kimlik bilgileri, topic yetkilendirmesi ve sertifika yönetimi kullanılmalıdır. Geliştirme ortamındaki anonymous Mosquitto yapılandırması production için uygun değildir. Secret, JWT anahtarı ve blockchain private key değerleri source control’e yazılmamalı; User Secrets, environment variable veya güvenli secret manager kullanılmalıdır.

### Yenilikçi Yönler ve Katkı

- Telekonsültasyon, chat, IoT ve sağlık ölçümlerinin tek platformda birleştirilmesi.
- Giyilebilir cihaz verilerinin doktor takip süreçlerine aktarılabilmesi.
- Sürekli telemetri için MQTT tabanlı ve düşük bant genişliği kullanan iletişim.
- Tenant bazlı, çok kurumlu mimariye uygunluk.
- Yapay zekâ karar destek servislerine bağlanabilecek standart sağlık verisi modeli.
- Hassas veriyi blockchain’e yazmadan kayıt bütünlüğü doğrulama.
- Katmanlı mimari ile farklı cihaz ve servislerin sonradan eklenebilmesi.

### Geliştirme Yol Haritası

Wear OS tarafında uzun süreli foreground sağlık oturumu, desteklenen adım verilerinin MQTT aktarımı, hasta/tenant yapılandırması ve Health Connect üzerinden harici tansiyon/glikoz kayıtlarını içe aktarma akışları uygulanmıştır. Sonraki adım, bu özellikleri gerçek Galaxy Watch 8 ve desteklenen harici cihazlarla uçtan uca doğrulamaktır.

Orta vadede resmi cihaz/üretici entegrasyonları, yapay zekâ sağlayıcısı, anomali tespiti ve doktor bildirimleri hedeflenmektedir.

Uzun vadede bulut tabanlı deployment, yüksek erişilebilirlik, kurumsal kimlik yönetimi, resmi medikal cihaz entegrasyonları, mevzuat ve sertifikasyon süreçleri planlanmaktadır.

### Mevcut Durum

Hazır olan başlıca altyapılar:

- .NET 10 Web API ve EF Core Code First.
- SQL Server veri altyapısı.
- CQRS, MediatR, JWT ve Swagger.
- SignalR chat ve WebRTC signaling.
- HealthMeasurement ve IoT device domain modelleri.
- MQTT backend consumer ve Mosquitto Docker yapılandırması.
- Wear OS Kotlin başlangıç uygulaması.
- Samsung Galaxy Watch 8 MQTT telemetrisi ve broker bağlantı testi.
- Health Services ile kullanıcı başlatmalı uzun süreli arka plan nabız/adım ölçüm servisi.
- Health Connect üzerinden izinli harici tansiyon ve glikoz kayıtlarını MQTT'ye aktarma.
- Wear OS hasta/tenant/broker yapılandırma ekranı; parola şifreli olarak saklanır.
- Redis, RabbitMQ, blockchain ve health check altyapıları.
- Unit test ve Codecov entegrasyonu.

Gerçek Galaxy Watch 8 üzerinde sensör/MQTT uçtan uca ve uzun süreli çalışma testi henüz bu geliştirme ortamında yapılmadı; README'deki test adımlarına göre cihazda doğrulanmalıdır. Harici tansiyon/glikoz aktarımı, cihaz ve eşlikçi uygulamanın Health Connect'e kayıt sağlamasına bağlıdır. Production MQTT için TLS, cihaz kimlik doğrulaması ve topic yetkilendirmesi ayrıca yapılandırılmalıdır. Ollama motoru teknik olarak entegre edilmiştir; klinik doğrulama, model değerlendirmesi ve mevzuat uygunluğu ayrıca gereklidir.

### Sonuç

TeleTip; uzaktan sağlık hizmetleri, giyilebilir cihazlar, IoT veri toplama, görüntülü görüşme, gerçek zamanlı iletişim, yapay zekâ ve blockchain doğrulama teknolojilerini bir araya getiren genişletilebilir bir dijital sağlık platformudur.

Proje, doktor kararının yerine geçmeyi değil; sağlık profesyonellerinin hastaya ait güncel ve anlamlı verilere daha hızlı ulaşmasını sağlayan bir karar destek ve uzaktan takip altyapısı oluşturmayı amaçlar. Gerçek sensör verilerinin kullanıma alınması; ilgili SDK erişimi, cihaz izinleri, medikal cihaz uyumluluğu, güvenlik kontrolleri ve gerekli mevzuat süreçlerinin tamamlanmasına bağlıdır.

## Durum

Hazır olan özellikler:

- .NET 10 Web API
- EF Core Code First
- SQL Server migration
- CQRS
- Swagger ve JWT Authorize
- SignalR chat
- WebRTC signaling
- Blockchain hash notarization
- IoT entity modeli
- Ollama tabanlı yerel AI analiz motoru ve doktor erişim kontrollü analiz API'si
- Redis ve RabbitMQ altyapısı
- Health checks
- MQTT tabanlı Galaxy Watch 8 telemetrisi ve uygulama içi broker bağlantı testi
- Health Services ile nabız ve cihaz destekliyorsa adım verisini MQTT'ye aktaran, bildirimli uzun süreli foreground ölçüm servisi
- Kullanıcı izniyle Health Connect'ten harici tansiyon/glikoz kayıtlarını MQTT'ye aktarma
- Wear OS hasta/tenant/broker yapılandırma ekranı ve şifreli parola saklama
- Doctor/Patient rol tabanlı yetkilendirme ve atanmış görüşme/hasta bazlı erişim kontrolleri
- Chat geçmişinde görüşme, gönderen rolü ve tarih aralığı filtreleri ile sayfalama
- TURN sunucusu için yapılandırma ve kısa ömürlü kimlik bilgisi üretimi

Doğrulanması veya tamamlanması gereken özellikler:

- Gerçek Galaxy Watch 8 üzerinde sensör/MQTT uçtan uca ve uzun süreli arka plan çalışma testi (cihaz bağlı olmadığı için henüz yapılmadı)
- Health Connect'e veri yazan destekli harici tansiyon/glikoz cihazlarıyla gerçek cihaz doğrulaması
- Production MQTT için TLS ve cihaz kimlik doğrulaması
- Production ortamında TURN sunucusunun kurulumu, TLS ve güvenlik ayarları
- Production öncesi bağımsız penetrasyon testi ve kritik/yüksek bulguların giderilmesi
- Production blockchain ağına deployment
