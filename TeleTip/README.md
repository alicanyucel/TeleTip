# TeleTip

[![CI/CD](https://github.com/alicanyucel/TeleTip/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/alicanyucel/TeleTip/actions/workflows/ci-cd.yml)
[![codecov](https://codecov.io/gh/alicanyucel/TeleTip/branch/master/graph/badge.svg)](https://codecov.io/gh/alicanyucel/TeleTip)

TeleTip, yapay zekâ destekli uzaktan sağlık hizmetleri için geliştirilen bir .NET Web API projesidir. Sistem; hasta ve doktor yönetimi, görüntülü görüşme, gerçek zamanlı chat, IoT sağlık verileri, AI analiz modeli ve blockchain üzerinde sağlık kaydı doğrulama altyapısı içerir.

## İçindekiler

- [Teknolojiler](#teknolojiler)
- [Mimari](#mimari)
- [Gereksinimler](#gereksinimler)
- [Kurulum](#kurulum)
- [Veritabanı ve Code First](#veritabanı-ve-code-first)
- [Uygulamayı Çalıştırma](#uygulamayı-çalıştırma)
- [Swagger Kullanımı](#swagger-kullanımı)
- [API Kullanımı](#api-kullanımı)
- [Görüntülü Görüşme ve Chat](#görüntülü-görüşme-ve-chat)
- [Blockchain](#blockchain)
- [IoT ve AI](#iot-ve-ai)
- [Güvenlik](#güvenlik)
- [Proje Yapısı](#proje-yapısı)

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
- Blockchain için Node.js ve npm
- Kamera ve mikrofon erişimi için HTTPS veya localhost

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

Redis ve RabbitMQ kullanılıyorsa Docker servislerini başlatabilirsiniz:

```powershell
docker compose up -d redis rabbitmq
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

GET  /api/telecare/chat-messages
POST /api/telecare/chat-messages
```

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
4. Bir sekmede `Doktor`, diğer sekmede `Hasta` seçin.
5. Geçerli JWT token girin.
6. Kamera ve mikrofon izinlerini verin.

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

Üretim ortamında farklı ağlar için TURN server kullanılması önerilir.

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

IoT modeli aşağıdaki cihazları destekler:

- Akıllı saat
- Tansiyon ölçer
- Glikometre

IoT entity ve sağlık ölçümü modeli hazırdır. Gerçek Bluetooth, MQTT, Apple Health veya Google Fit entegrasyonu ayrıca yapılmalıdır.

AI modeli aşağıdaki işlemleri destekleyecek şekilde hazırlandı:

- Genel analiz
- Risk tespiti
- Doktor uyarısı
- Risk skoru

Gerçek AI analizi için Azure OpenAI, OpenAI veya başka bir model sağlayıcısının ayrıca bağlanması gerekir.

## Güvenlik

- JWT authentication
- Controller seviyesinde `[Authorize]`
- SignalR hub seviyesinde `[Authorize]`
- Tenant bazlı global query filter
- Hassas sağlık verisini blockchain’e göndermeme
- Private key’leri User Secrets/environment variable ile saklama
- HTTPS kullanımı

Production ortamında ayrıca şu önlemler alınmalıdır:

- Gerçek ve güçlü JWT secret kullanımı
- Refresh token mekanizması
- Doctor/Patient role-based authorization
- TURN server güvenliği
- Rate limiting ayarlarının gözden geçirilmesi
- CORS adreslerinin sadece güvenilir domain’lerle sınırlandırılması
- Secret manager kullanımı

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
- AI analiz entity modeli
- Redis ve RabbitMQ altyapısı
- Health checks

Geliştirilmesi gereken özellikler:

- Gerçek AI sağlayıcısı entegrasyonu
- Gerçek IoT cihaz bağlantıları
- TURN server
- Doctor/Patient role-based authorization
- Chat geçmişi için filtreleme ve sayfalama
- Production blockchain ağına deployment
