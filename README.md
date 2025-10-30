# LMS (Learning Management System) - .NET 9

## 📋 Proje Hakkında

Bu proje, **Clean Architecture** prensiplerine uygun olarak geliştirilmiş, modern bir **Learning Management System (LMS)** uygulamasıdır.

### 🎯 Temel Özellikler

#### 1. Modüler Eğitim Yapısı
- **Modüller**: Ana eğitim kategorileri
- **Eğitimler (Trainings)**: Her modül altında birden fazla eğitim
- **Alt Başlıklar (SubTopics)**: Her eğitimin detaylı içerik parçaları

#### 2. Süre Kontrollü İlerleme Sistemi ⏱️
> **ÇOK ÖNEMLİ:** Sistemin en kritik özelliği!

- Her alt başlık için **MinimumDurationSeconds** belirlenir
- Kullanıcı bu minimum süreyi tamamlamadan sonraki alt başlığa geçemez
- İlerleme otomatik olarak kaydedilir
- Süre tamamlandığında otomatik olarak işaretlenir

**Örnek Senaryo:**
```
Alt Başlık 1: MinimumDuration = 300 saniye (5 dakika)
- Kullanıcı 250 saniye izledi → İlerleyemez
- Kullanıcı 300+ saniye izledi → Otomatik tamamlanır → Sonraki alt başlık açılır
```

#### 3. Sıralı İlerleme Sistemi 🔒
- Alt başlıklar **OrderIndex** sırasına göre açılır
- Önceki alt başlık tamamlanmadan sonrakine erişilemez
- İlk alt başlık (OrderIndex = 0) her zaman erişilebilir

**Örnek:**
```
Alt Başlık 1 (OrderIndex: 0) → Erişilebilir ✅
Alt Başlık 2 (OrderIndex: 1) → Kilitli 🔒 (Alt Başlık 1 tamamlanmalı)
Alt Başlık 3 (OrderIndex: 2) → Kilitli 🔒 (Alt Başlık 2 tamamlanmalı)
```

#### 4. Sertifika Sistemi 🎓

**Modül Sertifikası:**
- Modüldeki **TÜM** eğitimler tamamlanmalı
- Her eğitimdeki **TÜM** alt başlıklar tamamlanmalı
- Tamamlanmadan sertifika VERİLEMEZ!

**Eğitim Sertifikası:**
- Eğitimdeki **TÜM** alt başlıklar tamamlanmalı
- Her alt başlığın minimum süresi tamamlanmalı

**Sertifika Özellikleri:**
- Benzersiz sertifika numarası
- Doğrulama kodu
- Tarih bilgisi
- İptal edilebilir

## 🏗️ Mimari Yapı

### Clean Architecture Katmanları

```
📦 LMS-Project
├── 📂 Domain (İş Kuralları)
│   ├── Entities (Module, Training, SubTopic, User, Certificate, vb.)
│   ├── Interfaces (IRepository, IUnitOfWork)
│   └── Common (BaseEntity, Result)
│
├── 📂 Application (Use Cases)
│   ├── DTOs
│   ├── Features (CQRS - Commands & Queries)
│   ├── Interfaces (IProgressService, ICertificateService, vb.)
│   └── Behaviors (Validation, Permission, Logging)
│
├── 📂 Infrastructure (Dış Dünya)
│   ├── Persistence (DbContext, Repositories, Configurations)
│   └── Services (ProgressService, AuthService, JwtService, vb.)
│
└── 📂 API (Presentation)
    ├── Controllers
    └── Program.cs
```

### Teknolojiler ve Patterns

- **.NET 9**
- **Entity Framework Core 9.0**
- **MediatR** (CQRS Pattern)
- **FluentValidation**
- **JWT Authentication**
- **Repository Pattern**
- **Unit of Work Pattern**
- **Clean Architecture**
- **Dependency Injection**

## 🔑 API Endpoints

### Authentication
```http
POST /api/auth/login          # Giriş yap
POST /api/auth/register       # Kayıt ol
POST /api/auth/refresh-token  # Token yenile
POST /api/auth/logout         # Çıkış yap
POST /api/auth/change-password # Şifre değiştir
```

### Modules
```http
GET  /api/modules             # Tüm modülleri listele
GET  /api/modules/{id}        # Modül detayı (eğitimlerle birlikte)
```

### Trainings
```http
GET  /api/trainings/{id}      # Eğitim detayı (alt başlıklarla birlikte)
```

### Progress (İlerleme)
```http
POST /api/progress/update     # İlerleme güncelle (süre kontrolü ile)
GET  /api/progress/summary/{moduleId}  # Modül ilerleme özeti
GET  /api/progress/next-subtopic/{trainingId}  # Sonraki erişilebilir alt başlık
```

### Certificates
```http
POST /api/certificates/module/{moduleId}     # Modül sertifikası oluştur
POST /api/certificates/training/{trainingId} # Eğitim sertifikası oluştur
GET  /api/certificates/my-certificates       # Kullanıcının sertifikaları
GET  /api/certificates/verify/{code}         # Sertifika doğrula (Public)
```

## 🚀 Kurulum ve Çalıştırma

### 1. Gereksinimler
- .NET 9 SDK
- SQL Server (LocalDB veya Express)
- Visual Studio 2022 / VS Code / Rider

### 2. Veritabanı Oluşturma

```bash
# LMS-Project.API klasörüne gidin
cd src/LMS-Project.API

# Migration oluşturun
dotnet ef migrations add InitialCreate --project ../LMS-Project.Infrastructure

# Veritabanını oluşturun
dotnet ef database update --project ../LMS-Project.Infrastructure
```

### 3. Appsettings Yapılandırması

`appsettings.json` dosyasında:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LMS_DB;Trusted_Connection=True;..."
  },
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "LMS-API",
    "Audience": "LMS-Client"
  }
}
```

### 4. Uygulamayı Çalıştırın

```bash
dotnet run --project src/LMS-Project.API
```

Tarayıcıda açın: `https://localhost:7XXX/swagger`

## 📊 Veritabanı Şeması

### Temel Tablolar

**Modules**
- Id, Name, Description, OrderIndex, IsActive, EstimatedDurationMinutes

**Trainings**
- Id, ModuleId, Name, TotalDurationSeconds, OrderIndex, IsActive

**SubTopics**
- Id, TrainingId, Name, **MinimumDurationSeconds**, OrderIndex, ZipFilePath, HtmlFilePath

**UserProgress** (İlerleme Tablosu)
- UserId, SubTopicId, **DurationSeconds**, **IsCompleted**, LastAccessedDate

**UserTraining**
- UserId, TrainingId, IsCompleted, CompletionPercentage

**UserModule**
- UserId, ModuleId, IsCompleted, CompletionPercentage

**Certificates**
- UserId, ModuleId/TrainingId, CertificateNumber, VerificationCode, IssuedDate

## 🔐 Güvenlik

### JWT Authentication
- Access Token: 1 saat geçerlilik
- Refresh Token: 7 gün geçerlilik
- Rol ve izin tabanlı yetkilendirme

### Password Security
- PBKDF2 algoritması ile hashleme
- Salt kullanımı
- Timing attack koruması

## 🎯 Örnek Kullanım Senaryosu

### 1. Kullanıcı Kaydı ve Giriş
```http
POST /api/auth/register
{
  "userName": "ahmet",
  "email": "ahmet@example.com",
  "password": "Aa123456",
  "firstName": "Ahmet",
  "lastName": "Yılmaz"
}

POST /api/auth/login
{
  "userNameOrEmail": "ahmet",
  "password": "Aa123456"
}
```

### 2. Modülleri Listele
```http
GET /api/modules
Authorization: Bearer {token}
```

### 3. Eğitime Başla
```http
GET /api/trainings/{trainingId}
# Dönen response'da alt başlıklar IsLocked bilgisiyle gelir
```

### 4. İlerleme Kaydet
```http
POST /api/progress/update
{
  "subTopicId": "guid",
  "durationSeconds": 150
}
# Eğer minimum süre dolmadıysa → İlerleme kaydedilir ama tamamlanmaz
# Eğer minimum süre dolduysa → Otomatik tamamlanır, sonraki açılır
```

### 5. Sertifika Al
```http
POST /api/certificates/training/{trainingId}
# TÜM alt başlıklar tamamlanmışsa → Sertifika oluşturulur
# Eksik varsa → Hata döner
```

## 🧪 Test Senaryoları

### Süre Kontrolü Testi
1. Alt başlık minimum süresi: 300 saniye
2. İlk istek: 100 saniye → İlerleme kaydedilir, tamamlanmaz
3. İkinci istek: 150 saniye → Toplam 250 saniye, tamamlanmaz
4. Üçüncü istek: 100 saniye → Toplam 350 saniye, **tamamlanır!**

### Sıralı İlerleme Testi
1. Alt Başlık 1 (OrderIndex: 0) → Erişilebilir
2. Alt Başlık 2'ye erişim denemesi → **Hata!** (Alt Başlık 1 tamamlanmalı)
3. Alt Başlık 1'i tamamla
4. Alt Başlık 2 → Artık erişilebilir!

## 📝 Önemli Notlar

### 🚨 Kritik Kurallar

1. **Minimum Süre Tamamlanmadan İlerlenemez**
   - Her alt başlıkta mutlaka MinimumDurationSeconds kontrolü yapılır
   - Kullanıcı bu süreyi doldurmadan sonraki içeriğe geçemez

2. **Sıralı İlerleme Zorunludur**
   - Alt başlıklar OrderIndex sırasına göre açılır
   - Önceki tamamlanmadan sonrakine erişilemez

3. **Sertifika İçin Tam Tamamlama Şarttır**
   - Modül sertifikası: TÜM eğitimler + TÜM alt başlıklar
   - Eğitim sertifikası: TÜM alt başlıklar
   - Tek bir eksik bile olsa sertifika VERİLEMEZ!

4. **Soft Delete Kullanılır**
   - Veriler fiziksel olarak silinmez
   - IsDeleted flag'i kullanılır
   - Global query filter otomatik uygulanır

## 🤝 Katkıda Bulunma

Bu proje, modern .NET ve Clean Architecture best practice'lerine göre geliştirilmiştir.

## 📄 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

---

**Geliştirici Notu:** Bu sistem, süre kontrollü ve sıralı ilerleme gerektiren tüm e-öğrenme platformları için temel alınabilir. SCORM uyumlu içerik desteği eklenebilir.
