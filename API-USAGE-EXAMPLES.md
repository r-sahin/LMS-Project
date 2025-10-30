# LMS API Kullanım Örnekleri

## 🔐 Authentication (Kimlik Doğrulama)

### 1. Kayıt Ol (Register)

```http
POST /api/auth/register
Content-Type: application/json

{
  "userName": "ahmet.yilmaz",
  "email": "ahmet@example.com",
  "password": "Aa123456",
  "confirmPassword": "Aa123456",
  "firstName": "Ahmet",
  "lastName": "Yılmaz",
  "phoneNumber": "05551234567"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Kayıt başarılı. Giriş yapabilirsiniz.",
  "data": null,
  "errors": []
}
```

### 2. Giriş Yap (Login)

```http
POST /api/auth/login
Content-Type: application/json

{
  "userNameOrEmail": "ahmet.yilmaz",
  "password": "Aa123456"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Giriş başarılı.",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "xMzE2fQ7wXF8O9vB...",
    "expiresAt": "2025-10-18T15:30:00Z",
    "user": {
      "id": "guid",
      "userName": "ahmet.yilmaz",
      "email": "ahmet@example.com",
      "firstName": "Ahmet",
      "lastName": "Yılmaz",
      "fullName": "Ahmet Yılmaz",
      "isActive": true,
      "roles": ["User"],
      "permissions": ["ViewModules", "ViewTrainings"]
    }
  },
  "errors": []
}
```

### 3. Token Yenile (Refresh Token)

```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "xMzE2fQ7wXF8O9vB..."
}
```

### 4. Çıkış Yap (Logout)

```http
POST /api/auth/logout
Authorization: Bearer {access_token}
```

## 📚 Modules (Modüller)

### 1. Tüm Modülleri Listele

```http
GET /api/modules
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Modüller başarıyla getirildi.",
  "data": [
    {
      "id": "guid-1",
      "name": ".NET Core Eğitimi",
      "description": ".NET Core ile backend geliştirme",
      "orderIndex": 0,
      "isActive": true,
      "imagePath": "/images/dotnet.png",
      "estimatedDurationMinutes": 1200,
      "totalTrainings": 5,
      "completionPercentage": 40.5,
      "isCompleted": false
    }
  ]
}
```

### 2. Modül Detayı (Eğitimlerle Birlikte)

```http
GET /api/modules/{moduleId}
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Modül başarıyla getirildi.",
  "data": {
    "id": "guid",
    "name": ".NET Core Eğitimi",
    "description": "...",
    "trainings": [
      {
        "id": "training-guid",
        "name": "C# Temelleri",
        "orderIndex": 0,
        "totalSubTopics": 10,
        "completedSubTopics": 3,
        "completionPercentage": 30.0,
        "isCompleted": false
      }
    ]
  }
}
```

## 🎓 Trainings (Eğitimler)

### 1. Eğitim Detayı (Alt Başlıklarla Birlikte)

```http
GET /api/trainings/{trainingId}
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Eğitim başarıyla getirildi.",
  "data": {
    "id": "training-guid",
    "name": "C# Temelleri",
    "description": "...",
    "subTopics": [
      {
        "id": "subtopic-1",
        "name": "Değişkenler ve Veri Tipleri",
        "orderIndex": 0,
        "minimumDurationSeconds": 300,
        "currentDurationSeconds": 250,
        "isCompleted": false,
        "isLocked": false,
        "htmlFilePath": "/content/subtopic1.html"
      },
      {
        "id": "subtopic-2",
        "name": "Operatörler",
        "orderIndex": 1,
        "minimumDurationSeconds": 300,
        "currentDurationSeconds": 0,
        "isCompleted": false,
        "isLocked": true,
        "htmlFilePath": "/content/subtopic2.html"
      }
    ]
  }
}
```

## ⏱️ Progress (İlerleme) - SİSTEMİN KALBİ!

### 1. İlerleme Güncelle (Update Progress)

> **ÖNEMLİ:** Bu endpoint süre kontrolü yapar!

```http
POST /api/progress/update
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "subTopicId": "subtopic-1-guid",
  "durationSeconds": 150
}
```

**Senaryo 1: Minimum süre henüz dolmadı**

Request: `durationSeconds: 150` (Minimum: 300)

Response:
```json
{
  "isSuccess": true,
  "message": "İlerleme kaydedildi.",
  "data": {
    "subTopicId": "subtopic-1-guid",
    "subTopicName": "Değişkenler ve Veri Tipleri",
    "durationSeconds": 150,
    "minimumDurationSeconds": 300,
    "isCompleted": false,
    "completionPercentage": 50.0,
    "isLocked": false
  }
}
```

**Senaryo 2: Minimum süre tamamlandı (2. istek)**

Request: `durationSeconds: 200` (Toplam: 350 >= 300)

Response:
```json
{
  "isSuccess": true,
  "message": "İlerleme kaydedildi.",
  "data": {
    "subTopicId": "subtopic-1-guid",
    "subTopicName": "Değişkenler ve Veri Tipleri",
    "durationSeconds": 350,
    "minimumDurationSeconds": 300,
    "isCompleted": true,
    "completedDate": "2025-10-18T14:30:00Z",
    "completionPercentage": 100.0
  }
}
```

**Senaryo 3: Sıralı ilerleme hatası**

Alt Başlık 1 tamamlanmadan Alt Başlık 2'yi güncellemeye çalışma:

Response:
```json
{
  "isSuccess": false,
  "message": "Önceki alt başlığı tamamlamadan bu alt başlığa erişemezsiniz.",
  "errors": ["Önceki alt başlığı tamamlamadan bu alt başlığa erişemezsiniz."]
}
```

### 2. Sonraki Erişilebilir Alt Başlığı Getir

```http
GET /api/progress/next-subtopic/{trainingId}
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Sonraki alt başlık bulundu.",
  "data": {
    "id": "subtopic-2-guid",
    "name": "Operatörler",
    "orderIndex": 1,
    "minimumDurationSeconds": 300,
    "isCompleted": false,
    "isLocked": false,
    "htmlFilePath": "/content/subtopic2.html"
  }
}
```

### 3. Modül İlerleme Özeti

```http
GET /api/progress/summary/{moduleId}
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "moduleId": "module-guid",
    "moduleName": ".NET Core Eğitimi",
    "totalTrainings": 5,
    "completedTrainings": 2,
    "totalSubTopics": 50,
    "completedSubTopics": 25,
    "moduleCompletionPercentage": 50.0,
    "trainings": [
      {
        "trainingId": "training-1",
        "trainingName": "C# Temelleri",
        "totalSubTopics": 10,
        "completedSubTopics": 10,
        "completionPercentage": 100.0,
        "isCompleted": true
      }
    ]
  }
}
```

## 🎓 Certificates (Sertifikalar)

### 1. Eğitim Sertifikası Oluştur

> **ÖNEMLİ:** Eğitimdeki TÜM alt başlıklar tamamlanmış olmalı!

```http
POST /api/certificates/training/{trainingId}
Authorization: Bearer {access_token}
```

**Başarılı Response:**
```json
{
  "isSuccess": true,
  "message": "Eğitim sertifikası başarıyla oluşturuldu.",
  "data": {
    "id": "cert-guid",
    "certificateNumber": "TRN-20251018143000-5432",
    "certificateType": "Training",
    "trainingName": "C# Temelleri",
    "userFullName": "Ahmet Yılmaz",
    "issuedDate": "2025-10-18T14:30:00Z",
    "verificationCode": "A1B2C3D4E5F6",
    "pdfFilePath": "/certificates/TRN-20251018143000-5432.pdf"
  }
}
```

**Hata Response (Tamamlanmamış):**
```json
{
  "isSuccess": false,
  "message": "Eğitim henüz tamamlanmadı. Sertifika oluşturulamaz.",
  "errors": ["Eğitim henüz tamamlanmadı. Sertifika oluşturulamaz."]
}
```

### 2. Modül Sertifikası Oluştur

> **ÇOK ÖNEMLİ:** Modüldeki TÜM eğitimler ve TÜM alt başlıklar tamamlanmış olmalı!

```http
POST /api/certificates/module/{moduleId}
Authorization: Bearer {access_token}
```

### 3. Kullanıcının Sertifikalarını Listele

```http
GET /api/certificates/my-certificates
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": "cert-1",
      "certificateType": "Training",
      "certificateNumber": "TRN-20251018143000-5432",
      "entityName": "C# Temelleri",
      "issuedDate": "2025-10-18T14:30:00Z",
      "isRevoked": false
    },
    {
      "id": "cert-2",
      "certificateType": "Module",
      "certificateNumber": "MOD-20251018150000-8765",
      "entityName": ".NET Core Eğitimi",
      "issuedDate": "2025-10-18T15:00:00Z",
      "isRevoked": false
    }
  ]
}
```

### 4. Sertifika Doğrula (Public Endpoint - Token Gerektirmez)

```http
GET /api/certificates/verify/{verificationCode}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "certificateNumber": "TRN-20251018143000-5432",
    "userFullName": "Ahmet Yılmaz",
    "trainingName": "C# Temelleri",
    "issuedDate": "2025-10-18T14:30:00Z",
    "isRevoked": false,
    "verificationCode": "A1B2C3D4E5F6"
  }
}
```

## 🔄 Tam Akış Örneği

### Senaryo: Kullanıcı bir eğitimi tamamlıyor ve sertifika alıyor

```bash
# 1. Kayıt ol
POST /api/auth/register
{ "userName": "ali", "email": "ali@test.com", "password": "Aa123456", ... }

# 2. Giriş yap
POST /api/auth/login
{ "userNameOrEmail": "ali", "password": "Aa123456" }
# Response: { "accessToken": "..." }

# 3. Modülleri listele
GET /api/modules
Authorization: Bearer {token}

# 4. Bir eğitim seç
GET /api/trainings/{trainingId}
Authorization: Bearer {token}

# 5. İlk alt başlığı izle (300 saniye gerekli)
POST /api/progress/update
{ "subTopicId": "sub1", "durationSeconds": 150 }
# Response: isCompleted: false (henüz 150 saniye)

POST /api/progress/update
{ "subTopicId": "sub1", "durationSeconds": 200 }
# Response: isCompleted: true (toplam 350 >= 300) ✅

# 6. İkinci alt başlık artık açıldı
POST /api/progress/update
{ "subTopicId": "sub2", "durationSeconds": 300 }
# Response: isCompleted: true ✅

# 7. ... Tüm alt başlıkları tamamla ...

# 8. Sertifika al (TÜM alt başlıklar tamamlandı)
POST /api/certificates/training/{trainingId}
# Response: Sertifika oluşturuldu! 🎓
```

## ⚠️ Önemli Hatırlatmalar

1. **Tüm authenticated endpoint'ler için Authorization header gereklidir:**
   ```
   Authorization: Bearer {access_token}
   ```

2. **Minimum süre kontrolü çok hassastır:**
   - Alt başlık minimum süresi = 300 saniye
   - Kullanıcı 299 saniye izlese bile → Tamamlanmaz
   - 300+ saniye → Otomatik tamamlanır

3. **Sıralı ilerleme zorunludur:**
   - OrderIndex 0 → Her zaman erişilebilir
   - OrderIndex 1 → Sadece 0 tamamlandıysa
   - OrderIndex 2 → Sadece 1 tamamlandıysa

4. **Sertifika için tam tamamlama şarttır:**
   - 1 bile alt başlık eksikse → Sertifika VERİLMEZ
   - TÜM alt başlıklar 100% → Sertifika oluşturulur

5. **Validation hataları 400 Bad Request döner:**
   ```json
   {
     "isSuccess": false,
     "message": "Validasyon hatası",
     "errors": ["Şifre en az 6 karakter olmalıdır."]
   }
   ```
