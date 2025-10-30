# Admin API Kullanım Kılavuzu

## 🔐 Yetkilendirme

Tüm admin endpoint'leri **Admin** rolü gerektirir:
```
Authorization: Bearer {admin_token}
Role: Admin
```

## 📚 Modül Yönetimi

### 1. Modül Oluştur

```http
POST /api/admin/adminmodules
Authorization: Bearer {admin_token}
Content-Type: multipart/form-data

Form Data:
- Name: ".NET Core Eğitimi" (required)
- Description: "Modern .NET ile backend development"
- EstimatedDurationMinutes: 1200 (required)
- ImageFile: [dosya] (optional, max 100MB)
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Modül başarıyla oluşturuldu.",
  "data": "module-guid",
  "errors": []
}
```

### 2. Modül Güncelle

```http
PUT /api/admin/adminmodules/{id}
Authorization: Bearer {admin_token}
Content-Type: multipart/form-data

Form Data:
- Id: "module-guid"
- Name: "Updated Name"
- Description: "Updated desc"
- EstimatedDurationMinutes: 1500
- IsActive: true
- ImageFile: [yeni dosya] (optional)
```

### 3. Modül Sil

```http
DELETE /api/admin/adminmodules/{id}
Authorization: Bearer {admin_token}
```

**Not:** Soft delete yapılır + Tüm fiziksel dosyalar silinir (modül klasörü)

### 4. Modülleri Yeniden Sırala

```http
POST /api/admin/adminmodules/reorder
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "modules": [
    { "id": "module-1-guid", "orderIndex": 0 },
    { "id": "module-2-guid", "orderIndex": 1 },
    { "id": "module-3-guid", "orderIndex": 2 }
  ]
}
```

---

## 🎓 Eğitim Yönetimi

### 1. Eğitim Oluştur

```http
POST /api/admin/admintrainings
Authorization: Bearer {admin_token}
Content-Type: multipart/form-data

Form Data:
- ModuleId: "module-guid" (required)
- Name: "C# Temelleri" (required)
- Description: "C# programlama dili temelleri"
- TotalDurationSeconds: 3600 (required)
- ThumbnailFile: [resim dosyası] (optional)
- VideoIntroFile: [video dosyası] (optional)
```

**Özellikler:**
- Otomatik OrderIndex ataması (en son sıra)
- Thumbnail ve video upload
- ModuleId ile ilişkilendirme

### 2. Eğitim Güncelle

```http
PUT /api/admin/admintrainings/{id}
Authorization: Bearer {admin_token}
Content-Type: multipart/form-data

Form Data:
- Id: "training-guid"
- Name: "Updated Name"
- Description: "Updated description"
- TotalDurationSeconds: 4200
- IsActive: true
- ThumbnailFile: [yeni resim] (optional)
- VideoIntroFile: [yeni video] (optional)
```

**Not:** Yeni dosya yüklenirse eski otomatik silinir

### 3. Eğitim Sil

```http
DELETE /api/admin/admintrainings/{id}
Authorization: Bearer {admin_token}
```

**Silinen dosyalar:**
- Thumbnail resmi
- Intro video
- Tüm alt başlık klasörü (`/content/modules/{moduleId}/trainings/{trainingId}`)

### 4. Eğitimleri Yeniden Sırala

```http
POST /api/admin/admintrainings/reorder
Content-Type: application/json

{
  "trainings": [
    { "id": "training-1-guid", "orderIndex": 0 },
    { "id": "training-2-guid", "orderIndex": 1 }
  ]
}
```

---

## 📝 Alt Başlık Yönetimi (ZIP Upload)

### 1. Alt Başlık Oluştur ⭐ (ZIP Otomatik Ayıklanır)

```http
POST /api/admin/adminsubtopics
Authorization: Bearer {admin_token}
Content-Type: multipart/form-data

Form Data:
- TrainingId: "training-guid" (required)
- Name: "Değişkenler ve Veri Tipleri" (required)
- Description: "C# veri tipleri"
- MinimumDurationSeconds: 300 (required, ⭐ kritik!)
- IsMandatory: true
- ZipFile: [content.zip dosyası] (required, ⭐ kritik!)
- ThumbnailFile: [resim] (optional)
```

**ZIP Dosyası Gereksinimleri:**
- ✅ Uzantı: `.zip`
- ✅ Max boyut: 100 MB
- ✅ İçinde **mutlaka HTML dosyası** olmalı
- ✅ Tercih edilen: `index.html` (yoksa ilk HTML dosyası kullanılır)

**Otomatik İşlemler:**
1. ZIP dosyası yüklenir: `/content/modules/{moduleId}/trainings/{trainingId}/subtopics/{subTopicId}/content.zip`
2. ZIP otomatik ayıklanır: `/content/modules/{moduleId}/trainings/{trainingId}/subtopics/{subTopicId}/files/`
3. HTML dosyası bulunur ve path kaydedilir
4. OrderIndex otomatik atanır

**Klasör Yapısı:**
```
wwwroot/
└── content/
    └── modules/
        └── {moduleId}/
            └── trainings/
                └── {trainingId}/
                    └── subtopics/
                        └── {subTopicId}/
                            ├── content.zip
                            └── files/
                                ├── index.html  ✅
                                ├── style.css
                                ├── script.js
                                └── images/
```

### 2. Alt Başlık Güncelle

```http
PUT /api/admin/adminsubtopics/{id}
Authorization: Bearer {admin_token}
Content-Type: multipart/form-data

Form Data:
- Id: "subtopic-guid"
- Name: "Updated Name"
- Description: "Updated desc"
- MinimumDurationSeconds: 450
- IsMandatory: true
- IsActive: true
- ZipFile: [yeni content.zip] (optional)
- ThumbnailFile: [yeni resim] (optional)
```

**Not:**
- Yeni ZIP yüklenirse:
  - Eski klasör tamamen silinir
  - Yeni ZIP ayıklanır
  - HTML path güncellenir

### 3. Alt Başlık Sil ⚠️

```http
DELETE /api/admin/adminsubtopics/{id}
Authorization: Bearer {admin_token}
```

**Silinen Dosyalar:**
- ZIP dosyası (`content.zip`)
- Ayıklanan tüm dosyalar (`files/` klasörü)
- Thumbnail resmi
- Tüm subtopic klasörü

### 4. Alt Başlıkları Yeniden Sırala

```http
POST /api/admin/adminsubtopics/reorder
Content-Type: application/json

{
  "subTopics": [
    { "id": "subtopic-1-guid", "orderIndex": 0 },
    { "id": "subtopic-2-guid", "orderIndex": 1 },
    { "id": "subtopic-3-guid", "orderIndex": 2 }
  ]
}
```

**Önemli:**
- OrderIndex değiştirince sıralı ilerleme mantığı etkilenir
- OrderIndex 0 = İlk alt başlık (her zaman erişilebilir)
- OrderIndex 1+ = Önceki tamamlanmalı

---

## 📦 Örnek ZIP İçeriği

### Basit Örnek:
```
content.zip
├── index.html         ✅ (Zorunlu)
├── style.css
└── script.js
```

### Gelişmiş Örnek:
```
content.zip
├── index.html         ✅
├── css/
│   └── main.css
├── js/
│   └── app.js
└── images/
    ├── diagram1.png
    └── diagram2.png
```

**index.html Örneği:**
```html
<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <title>Değişkenler ve Veri Tipleri</title>
    <link rel="stylesheet" href="css/main.css">
</head>
<body>
    <h1>C# Değişkenler</h1>
    <p>C# dilinde değişkenler...</p>
    <script src="js/app.js"></script>
</body>
</html>
```

---

## 🔄 Tam Workflow Örneği

### Senaryo: Sıfırdan Bir Modül Oluşturma

```bash
# 1. Modül oluştur
POST /api/admin/adminmodules
{
  Name: ".NET Core Eğitimi",
  EstimatedDurationMinutes: 1200,
  ImageFile: dotnet-logo.png
}
# Response: moduleId

# 2. Eğitim ekle
POST /api/admin/admintrainings
{
  ModuleId: {moduleId},
  Name: "C# Temelleri",
  TotalDurationSeconds: 3600,
  ThumbnailFile: csharp-thumb.png
}
# Response: trainingId

# 3. Alt başlık ekle (ZIP ile)
POST /api/admin/adminsubtopics
{
  TrainingId: {trainingId},
  Name: "Değişkenler",
  MinimumDurationSeconds: 300,
  ZipFile: degiskenler-content.zip  ⭐
}
# Response: subTopicId
# ZIP otomatik ayıklandı! ✅

# 4. Daha fazla alt başlık ekle
POST /api/admin/adminsubtopics
{
  TrainingId: {trainingId},
  Name: "Operatörler",
  MinimumDurationSeconds: 300,
  ZipFile: operatorler-content.zip
}

# 5. Sıralama değiştir
POST /api/admin/adminsubtopics/reorder
{
  "subTopics": [
    { "id": "{subTopic2}", "orderIndex": 0 },  // Operatörler önce
    { "id": "{subTopic1}", "orderIndex": 1 }   // Değişkenler sonra
  ]
}
```

---

## ⚠️ Önemli Notlar

### Dosya Yükleme Limitleri:
- Max dosya boyutu: **100 MB**
- ZIP: `.zip` uzantılı olmalı
- Resimler: `.jpg`, `.jpeg`, `.png`, `.gif`
- Videolar: `.mp4`, `.webm`, `.mov`

### Silme İşlemleri:
- **Soft Delete** yapılır (IsDeleted = true)
- **Fiziksel dosyalar** tamamen silinir
- **Alt başlık silinince:** ZIP + Extract edilen dosyalar + Thumbnail
- **Eğitim silinince:** Tüm alt başlıkların dosyaları + Thumbnail + Video
- **Modül silinince:** Tüm modül klasörü + Resim

### Sıralama:
- OrderIndex **0'dan başlar**
- Yeni öğe eklenince otomatik **en sona** eklenir
- Manuel sıralama için `/reorder` endpoint kullanılır
- Sıralama değişince **kullanıcı ilerlemesi etkilenmez**

### Güvenlik:
- Tüm endpoint'ler **Admin rolü** gerektirir
- Dosya yüklerken **virüs taraması** yapılması önerilir
- ZIP içeriği **sandbox** ortamında kontrol edilmeli

---

## 🧪 Test İçin Örnek Senaryolar

### 1. ZIP Upload Testi
```bash
# Geçerli ZIP
✅ content.zip (içinde index.html var)
✅ lesson1.zip (içinde lesson.html + main.html → main.html seçilir)

# Geçersiz ZIP
❌ no-html.zip (HTML dosyası yok → Hata)
❌ too-large.zip (>100MB → Hata)
❌ malware.exe.zip (Zararlı dosya → Güvenlik riski)
```

### 2. Sıralama Testi
```bash
# Başlangıç
Alt Başlık 1 → OrderIndex: 0
Alt Başlık 2 → OrderIndex: 1
Alt Başlık 3 → OrderIndex: 2

# Sıralamayı Değiştir
POST /reorder: [
  { id: "3", orderIndex: 0 },  # 3 başa geldi
  { id: "1", orderIndex: 1 },  # 1 ortada
  { id: "2", orderIndex: 2 }   # 2 sonda
]

# Sonuç
Alt Başlık 3 → OrderIndex: 0 (İlk erişilebilir)
Alt Başlık 1 → OrderIndex: 1 (3 tamamlanınca açılır)
Alt Başlık 2 → OrderIndex: 2 (1 tamamlanınca açılır)
```

### 3. Güncelleme Testi
```bash
# İlk oluşturma
POST /adminsubtopics
ZipFile: version1.zip
→ ZIP ayıklandı: /files/index.html

# Güncelleme (yeni ZIP)
PUT /adminsubtopics/{id}
ZipFile: version2.zip
→ Eski /files/ silindi
→ Yeni ZIP ayıklandı
→ HtmlFilePath güncellendi
```

---

**Artık admin olarak tüm içerik yönetimini yapabilirsiniz!** 🎉
