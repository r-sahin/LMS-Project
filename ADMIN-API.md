# 🔐 Admin API Endpoints Dokümantasyonu

## 📋 Genel Bakış

Bu dokümanda **sadece Admin rolüne** özel CRUD endpointleri bulunmaktadır.

**Yetkilendirme:** Tüm endpointler `[Authorize(Roles = "Admin")]` ile korunmuştur.

---

## 1️⃣ Modül Yönetimi (AdminModulesController)

### Base URL: `/api/admin/adminmodules`

### 1.1 Yeni Modül Oluştur
```http
POST /api/admin/adminmodules
Content-Type: multipart/form-data
Authorization: Bearer {admin_token}
```

**Request (FormData):**
```
Name: string (required)
Description: string (optional)
EstimatedDurationMinutes: int (required)
ImageFile: IFormFile (optional)
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Modül başarıyla oluşturuldu.",
  "data": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

---

### 1.2 Modül Güncelle
```http
PUT /api/admin/adminmodules/{id}
Content-Type: multipart/form-data
Authorization: Bearer {admin_token}
```

**Request (FormData):**
```
Name: string (required)
Description: string (optional)
EstimatedDurationMinutes: int (required)
IsActive: bool (required)
ImageFile: IFormFile (optional) - Yeni resim yüklenirse eski silinir
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Modül başarıyla güncellendi."
}
```

---

### 1.3 Modül Sil (Soft Delete)
```http
DELETE /api/admin/adminmodules/{id}
Authorization: Bearer {admin_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Modül başarıyla silindi."
}
```

**Not:**
- Soft Delete yapılır (IsDeleted = true)
- İlişkili dosyalar fiziksel olarak silinir
- Modülün resmi silinir

---

### 1.4 Modülleri Yeniden Sırala
```http
POST /api/admin/adminmodules/reorder
Content-Type: application/json
Authorization: Bearer {admin_token}
```

**Request Body:**
```json
{
  "modules": [
    { "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "orderIndex": 0 },
    { "id": "7ba85f64-5717-4562-b3fc-2c963f66afa7", "orderIndex": 1 },
    { "id": "9ca85f64-5717-4562-b3fc-2c963f66afa8", "orderIndex": 2 }
  ]
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Modüller başarıyla yeniden sıralandı."
}
```

---

## 2️⃣ Eğitim Yönetimi (AdminTrainingsController)

### Base URL: `/api/admin/admintrainings`

### 2.1 Yeni Eğitim Oluştur
```http
POST /api/admin/admintrainings
Content-Type: multipart/form-data
Authorization: Bearer {admin_token}
```

**Request (FormData):**
```
ModuleId: Guid (required)
Name: string (required)
Description: string (optional)
TotalDurationSeconds: int (required)
ThumbnailFile: IFormFile (optional)
VideoIntroFile: IFormFile (optional)
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Eğitim başarıyla oluşturuldu.",
  "data": "4fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

---

### 2.2 Eğitim Güncelle
```http
PUT /api/admin/admintrainings/{id}
Content-Type: multipart/form-data
Authorization: Bearer {admin_token}
```

**Request (FormData):**
```
Name: string (required)
Description: string (optional)
TotalDurationSeconds: int (required)
IsActive: bool (required)
ThumbnailFile: IFormFile (optional)
VideoIntroFile: IFormFile (optional)
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Eğitim başarıyla güncellendi."
}
```

**Not:** Yeni dosya yüklenirse eski dosyalar otomatik silinir.

---

### 2.3 Eğitim Sil (Soft Delete)
```http
DELETE /api/admin/admintrainings/{id}
Authorization: Bearer {admin_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Eğitim başarıyla silindi."
}
```

**Not:**
- Soft Delete (IsDeleted = true)
- Tüm ilişkili dosyalar fiziksel olarak silinir
- Thumbnail ve VideoIntro silinir

---

### 2.4 Eğitimleri Yeniden Sırala
```http
POST /api/admin/admintrainings/reorder
Content-Type: application/json
Authorization: Bearer {admin_token}
```

**Request Body:**
```json
{
  "trainings": [
    { "id": "4fa85f64-5717-4562-b3fc-2c963f66afa6", "orderIndex": 0 },
    { "id": "5fa85f64-5717-4562-b3fc-2c963f66afa7", "orderIndex": 1 }
  ]
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Eğitimler başarıyla yeniden sıralandı."
}
```

---

## 3️⃣ Alt Başlık Yönetimi (AdminSubTopicsController)

### Base URL: `/api/admin/adminsubtopics`

### 3.1 Yeni Alt Başlık Oluştur
```http
POST /api/admin/adminsubtopics
Content-Type: multipart/form-data
Authorization: Bearer {admin_token}
```

**Request (FormData):**
```
TrainingId: Guid (required)
Name: string (required, max 200)
Description: string (optional)
MinimumDurationSeconds: int (required, > 0)
IsMandatory: bool (required)
ZipFile: IFormFile (required) - HTML içeriği içeren ZIP
ThumbnailFile: IFormFile (optional)
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Alt başlık başarıyla oluşturuldu.",
  "data": "5fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Not:**
- ZIP dosyası otomatik extract edilir
- HTML dosyası index.html olarak aranır
- İçerik şu path'e kaydedilir: `/content/modules/{moduleId}/trainings/{trainingId}/subtopics/{subTopicId}/`

---

### 3.2 Alt Başlık Güncelle
```http
PUT /api/admin/adminsubtopics/{id}
Content-Type: multipart/form-data
Authorization: Bearer {admin_token}
```

**Request (FormData):**
```
Name: string (required)
Description: string (optional)
MinimumDurationSeconds: int (required)
IsMandatory: bool (required)
IsActive: bool (required)
ZipFile: IFormFile (optional) - Yeni ZIP yüklenirse eski içerik silinir
ThumbnailFile: IFormFile (optional)
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Alt başlık başarıyla güncellendi."
}
```

**Not:** Yeni ZIP yüklenirse eski tüm içerik klasörü silinir ve yenisi extract edilir.

---

### 3.3 Alt Başlık Sil (Soft Delete)
```http
DELETE /api/admin/adminsubtopics/{id}
Authorization: Bearer {admin_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Alt başlık ve dosyaları başarıyla silindi."
}
```

**Not:**
- Soft Delete (IsDeleted = true)
- ZIP dosyası ve extract edilen tüm içerik silinir
- Thumbnail silinir

---

### 3.4 Alt Başlıkları Yeniden Sırala
```http
POST /api/admin/adminsubtopics/reorder
Content-Type: application/json
Authorization: Bearer {admin_token}
```

**Request Body:**
```json
{
  "subTopics": [
    { "id": "5fa85f64-5717-4562-b3fc-2c963f66afa6", "orderIndex": 0 },
    { "id": "6fa85f64-5717-4562-b3fc-2c963f66afa7", "orderIndex": 1 }
  ]
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Alt başlıklar başarıyla yeniden sıralandı."
}
```

---

## 🔑 Kimlik Doğrulama

### Admin Girişi

```http
POST /api/auth/login
Content-Type: application/json
```

**Request:**
```json
{
  "email": "admin@lms.com",
  "password": "Admin123!"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Giriş başarılı.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "admin@lms.com",
    "roles": ["Admin"],
    "permissions": [
      "Modules.Create",
      "Modules.Update",
      "Modules.Delete",
      "Trainings.Create",
      "Trainings.Update",
      "Trainings.Delete",
      "ModuleRequests.Approve",
      "ModuleRequests.Reject",
      "Announcements.Create",
      "Announcements.Update",
      "Announcements.Delete"
    ]
  }
}
```

### Token Kullanımı

Tüm Admin endpointlerine istek yaparken token'ı header'a ekleyin:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 📝 İş Akışı Örnekleri

### Örnek 1: Yeni Modül + Eğitim + Alt Başlık Oluşturma

```bash
# 1. Admin giriş yap
POST /api/auth/login
Body: { "email": "admin@lms.com", "password": "Admin123!" }
→ Token al

# 2. Modül oluştur
POST /api/admin/adminmodules
Authorization: Bearer {token}
FormData:
  Name: ".NET Core"
  Description: "Modern backend development"
  EstimatedDurationMinutes: 600
  ImageFile: dotnet.png
→ ModuleId al (örn: abc-123)

# 3. Eğitim oluştur
POST /api/admin/admintrainings
FormData:
  ModuleId: abc-123
  Name: "ASP.NET Core Basics"
  TotalDurationSeconds: 3600
  ThumbnailFile: aspnet-thumb.jpg
→ TrainingId al (örn: def-456)

# 4. Alt başlık oluştur
POST /api/admin/adminsubtopics
FormData:
  TrainingId: def-456
  Name: "Dependency Injection"
  MinimumDurationSeconds: 300
  IsMandatory: true
  ZipFile: di-content.zip
→ SubTopicId al (örn: ghi-789)
```

---

### Örnek 2: Modül Sırasını Değiştirme

```bash
# Varsayılan sıra:
# 1. .NET Core (OrderIndex: 0)
# 2. React (OrderIndex: 1)
# 3. SQL (OrderIndex: 2)

# Yeni sıra istiyoruz:
# 1. SQL
# 2. .NET Core
# 3. React

POST /api/admin/adminmodules/reorder
Body:
{
  "modules": [
    { "id": "sql-module-id", "orderIndex": 0 },
    { "id": "dotnet-module-id", "orderIndex": 1 },
    { "id": "react-module-id", "orderIndex": 2 }
  ]
}
→ Sıra güncellenir
```

---

## ⚠️ Hata Mesajları

### 400 Bad Request
```json
{
  "isSuccess": false,
  "message": "Validasyon hatası",
  "errors": [
    "Modül seçilmelidir.",
    "Name alanı boş olamaz."
  ]
}
```

### 401 Unauthorized
```json
{
  "isSuccess": false,
  "message": "Yetkisiz erişim. Lütfen giriş yapın."
}
```

### 403 Forbidden
```json
{
  "isSuccess": false,
  "message": "Bu işlem için yetkiniz yok."
}
```

### 404 Not Found
```json
{
  "isSuccess": false,
  "message": "Modül bulunamadı."
}
```

---

## 🎯 Özellikler

### Modül İşlemleri
- ✅ CRUD (Create, Read, Update, Delete)
- ✅ Soft Delete (fiziksel silme değil)
- ✅ Resim yükleme/güncelleme/silme
- ✅ OrderIndex ile sıralama
- ✅ Toplu yeniden sıralama

### Eğitim İşlemleri
- ✅ CRUD
- ✅ Soft Delete
- ✅ Thumbnail + Video Intro yükleme
- ✅ OrderIndex ile sıralama
- ✅ ModuleId ile ilişkilendirme

### Alt Başlık İşlemleri
- ✅ CRUD
- ✅ Soft Delete
- ✅ ZIP yükleme ve otomatik extract
- ✅ HTML içerik yönetimi
- ✅ Thumbnail yükleme
- ✅ MinimumDurationSeconds kontrolü
- ✅ IsMandatory flag (zorunlu mu?)
- ✅ OrderIndex ile sıralama

---

## 📂 Dosya Yönetimi

### Modül Resimleri
```
/wwwroot/content/images/modules/module_{guid}.{ext}
```

### Eğitim Dosyaları
```
/wwwroot/content/images/trainings/training_{guid}.{ext}
/wwwroot/content/videos/trainings/video_{guid}.{ext}
```

### Alt Başlık Dosyaları
```
/wwwroot/content/modules/{moduleId}/trainings/{trainingId}/subtopics/{subTopicId}/
  ├── content.zip
  ├── index.html (extract edilmiş)
  └── ... (diğer HTML/CSS/JS dosyaları)
```

### Thumbnail'lar
```
/wwwroot/content/images/subtopics/subtopic_{guid}.{ext}
```

---

## 🧪 Test Senaryosu (Scalar UI)

### 1. Scalar'a Git
```
https://localhost:5001/scalar/v1
```

### 2. Admin Token Al
```
POST /api/auth/login
Body:
{
  "email": "admin@lms.com",
  "password": "Admin123!"
}
```

### 3. Token'ı Authorization Header'a Ekle
```
Scalar UI'da sağ üstte "Authorize" butonuna tıkla
Bearer {token} formatında ekle
```

### 4. Modül Oluştur
```
POST /api/admin/adminmodules
FormData kullan
Name: "Test Modülü"
EstimatedDurationMinutes: 120
```

### 5. Oluşturulan Modülü Gör
```
GET /api/modules
→ Yeni modül listede görünecek
```

---

## ✅ Özet

Artık sistemde **tam CRUD işlemleri** var:

1. ✅ **Modül:** Create, Update, Delete, Reorder
2. ✅ **Eğitim:** Create, Update, Delete, Reorder
3. ✅ **Alt Başlık:** Create, Update, Delete, Reorder

**Tüm endpointler:**
- Admin rolü gerektirir
- JWT token ile korunur
- Soft Delete kullanır
- Dosya yükleme destekler
- Validation içerir
- Scalar UI'da test edilebilir

**Hepsi hazır, eksik yok!** 🚀
