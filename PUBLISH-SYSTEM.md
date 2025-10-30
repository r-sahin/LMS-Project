# 🚀 Eğitim Yayınlama Sistemi (Publish System)

## 📋 Genel Bakış

Eğitimler artık **otomatik yayınlanmaz**. Admin manuel olarak onaylamalıdır.

### Kurallar:
1. ✅ **Eğitim oluşturulduğunda** → `IsPublished = false` (yayınlanmamış)
2. ✅ **Alt başlıkların toplamı eğitim süresine EŞİT** olmalı
3. ✅ **Admin onayladığında** → `IsPublished = true` (yayınlanmış)
4. ✅ **Öğrenciler sadece yayınlanmış eğitimleri** görebilir

---

## 🔄 İş Akışı

### 1. Admin Eğitim Oluşturur
```http
POST /api/admin/admintrainings
Authorization: Bearer {admin_token}

FormData:
  ModuleId: "abc-123"
  Name: "ASP.NET Core Basics"
  TotalDurationSeconds: 3600 (60 dakika)
```

**Sonuç:**
```json
{
  "isSuccess": true,
  "message": "Eğitim başarıyla oluşturuldu.",
  "data": "training-id-xyz"
}
```

**Not:** Eğitim oluşturuldu ama `IsPublished = false` → **Öğrenciler göremez!**

---

### 2. Admin Alt Başlıklar Ekler

#### İlk Alt Başlık (20 dakika)
```http
POST /api/admin/adminsubtopics

FormData:
  TrainingId: "training-id-xyz"
  Name: "Introduction"
  MinimumDurationSeconds: 1200 (20 dakika)
  ZipFile: intro.zip
```

**Sonuç:**
```json
{
  "isSuccess": true,
  "message": "Alt başlık başarıyla oluşturuldu.",
  "remainingSeconds": 2400,
  "remainingMinutes": 40
}
```

#### İkinci Alt Başlık (40 dakika)
```http
POST /api/admin/adminsubtopics

FormData:
  TrainingId: "training-id-xyz"
  Name: "Deep Dive"
  MinimumDurationSeconds: 2400 (40 dakika)
```

**Sonuç:**
```json
{
  "isSuccess": true,
  "message": "Alt başlık başarıyla oluşturuldu.",
  "remainingSeconds": 0,
  "remainingMinutes": 0
}
```

✅ Toplam: 1200 + 2400 = 3600 saniye = Eğitim süresi ✅

---

### 3. Admin Süre Kontrolü Yapar

```http
GET /api/admin/admintrainings/{training-id-xyz}/duration-info
Authorization: Bearer {admin_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "trainingId": "training-id-xyz",
    "trainingName": "ASP.NET Core Basics",
    "trainingTotalSeconds": 3600,
    "trainingTotalMinutes": 60,
    "usedSeconds": 3600,
    "remainingSeconds": 0,
    "remainingMinutes": 0,
    "remainingSecondsRemainder": 0,
    "usagePercentage": 100.00,
    "isPublished": false,
    "canBePublished": true,
    "publishBlockReason": null,
    "subTopics": [
      {
        "subTopicId": "...",
        "subTopicName": "Introduction",
        "durationSeconds": 1200,
        "durationMinutes": 20
      },
      {
        "subTopicId": "...",
        "subTopicName": "Deep Dive",
        "durationSeconds": 2400,
        "durationMinutes": 40
      }
    ]
  }
}
```

**Önemli Alanlar:**
- `canBePublished`: **true** → Yayınlanabilir
- `publishBlockReason`: **null** → Engel yok
- `usagePercentage`: **100%** → Tamamlandı

---

### 4. Admin Eğitimi Yayınlar

```http
POST /api/admin/admintrainings/{training-id-xyz}/publish
Authorization: Bearer {admin_token}
```

**Başarılı Response:**
```json
{
  "isSuccess": true,
  "message": "Eğitim başarıyla yayınlandı! Toplam 2 alt başlık ile 3600 saniye süre."
}
```

**Artık:**
- ✅ `IsPublished = true`
- ✅ `PublishedDate = 2025-10-19T12:00:00Z`
- ✅ `PublishedBy = "admin-user-id"`
- ✅ **Öğrenciler bu eğitimi görebilir**

---

## ❌ Hata Senaryoları

### Senaryo 1: Alt Başlık Yok
```http
POST /api/admin/admintrainings/{id}/publish
```

**Hata:**
```json
{
  "isSuccess": false,
  "message": "Eğitim yayınlanamaz! En az bir alt başlık eklemelisiniz."
}
```

---

### Senaryo 2: Süre Eksik
**Eğitim süresi:** 60 dakika (3600 saniye)
**Alt başlıklar toplamı:** 40 dakika (2400 saniye)

```http
POST /api/admin/admintrainings/{id}/publish
```

**Hata:**
```json
{
  "isSuccess": false,
  "message": "Eğitim yayınlanamaz! Alt başlıkların toplam süresi eğitim süresinden KISA. Eğitim süresi: 3600 saniye. Alt başlıklar toplamı: 2400 saniye. Eksik süre: 20 dakika 0 saniye."
}
```

---

### Senaryo 3: Süre Fazla
**Eğitim süresi:** 60 dakika (3600 saniye)
**Alt başlıklar toplamı:** 70 dakika (4200 saniye)

```http
POST /api/admin/admintrainings/{id}/publish
```

**Hata:**
```json
{
  "isSuccess": false,
  "message": "Eğitim yayınlanamaz! Alt başlıkların toplam süresi eğitim süresinden UZUN. Eğitim süresi: 3600 saniye. Alt başlıklar toplamı: 4200 saniye. Fazla süre: 10 dakika 0 saniye."
}
```

---

### Senaryo 4: Alt Başlık Eklerken Süre Aşımı
**Eğitim süresi:** 60 dakika (3600 saniye)
**Mevcut alt başlıklar:** 50 dakika (3000 saniye)
**Yeni alt başlık:** 20 dakika (1200 saniye)

```http
POST /api/admin/adminsubtopics

FormData:
  TrainingId: "xyz"
  MinimumDurationSeconds: 1200
```

**Hata:**
```json
{
  "isSuccess": false,
  "message": "Alt başlık süresi eğitim süresini aşıyor! Eğitim toplam süresi: 3600 saniye. Mevcut alt başlıklar toplamı: 3000 saniye. Kalan süre: 10 dakika 0 saniye. Girdiğiniz süre: 1200 saniye."
}
```

---

## 🔓 Yayından Kaldırma

Admin eğitimi yayından kaldırabilir:

```http
POST /api/admin/admintrainings/{id}/unpublish
Authorization: Bearer {admin_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Eğitim yayından kaldırıldı."
}
```

**Sonuç:**
- ❌ `IsPublished = false`
- ❌ `PublishedDate = null`
- ❌ `PublishedBy = null`
- ❌ **Öğrenciler artık bu eğitimi göremez**

---

## 👨‍🎓 Öğrenci Tarafı

### Öğrenci Eğitime Erişmeye Çalışır

**Eğitim yayınlanmamışsa:**
```http
GET /api/trainings/{training-id}
Authorization: Bearer {student_token}
```

**Hata:**
```json
{
  "isSuccess": false,
  "message": "Bu eğitim henüz yayınlanmamış."
}
```

**Eğitim yayınlanmışsa:**
```json
{
  "isSuccess": true,
  "data": {
    "id": "training-id",
    "name": "ASP.NET Core Basics",
    "subTopics": [ ... ]
  }
}
```

---

## 📊 API Endpointleri

### Admin Endpointleri

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| POST | `/api/admin/admintrainings/{id}/publish` | Eğitimi yayınla |
| POST | `/api/admin/admintrainings/{id}/unpublish` | Eğitimi yayından kaldır |
| GET | `/api/admin/admintrainings/{id}/duration-info` | Süre bilgilerini getir |

---

## 🎯 Özellikler

### Training Entity
```csharp
public class Training
{
    public bool IsPublished { get; set; } = false;
    public DateTime? PublishedDate { get; set; }
    public string? PublishedBy { get; set; }
    // ...
}
```

### Süre Kontrolü (CreateSubTopic)
```csharp
// Mevcut alt başlıkların toplamını hesapla
var currentTotalSeconds = subTopics.Sum(st => st.MinimumDurationSeconds);

// Yeni alt başlık eklenince toplam
var newTotalSeconds = currentTotalSeconds + request.MinimumDurationSeconds;

// Eğitim süresini aşıyor mu?
if (newTotalSeconds > training.TotalDurationSeconds)
{
    return Result.Failure("Alt başlık süresi eğitim süresini aşıyor!");
}
```

### Yayınlama Kontrolü (PublishTraining)
```csharp
// En az 1 alt başlık olmalı
if (!subTopics.Any())
{
    return Result.Failure("En az bir alt başlık eklemelisiniz.");
}

// Alt başlıkların toplamı eğitim süresine EŞİT olmalı
var totalSubTopicSeconds = subTopics.Sum(st => st.MinimumDurationSeconds);

if (totalSubTopicSeconds != training.TotalDurationSeconds)
{
    return Result.Failure("Süre eşit değil!");
}
```

### Öğrenci Query Filtresi
```csharp
// GetTrainingByIdQuery
if (!training.IsPublished)
{
    return Result.Failure("Bu eğitim henüz yayınlanmamış.");
}

// GetModuleByIdQuery - Sadece yayınlanmış eğitimleri göster
var publishedTrainings = module.Trainings.Where(t => t.IsPublished);
```

---

## ✅ Özet

### Admin İş Akışı:
1. ✅ Modül oluştur
2. ✅ Eğitim oluştur (`IsPublished = false`)
3. ✅ Alt başlıklar ekle (süre kontrolü ile)
4. ✅ Süre bilgilerini kontrol et (`/duration-info`)
5. ✅ Eğitimi yayınla (`/publish`)

### Öğrenci İş Akışı:
1. ✅ Sadece yayınlanmış eğitimleri görür
2. ✅ Yayınlanmamış eğitime erişemez
3. ✅ Modül detayında sadece yayınlanmış eğitimler listelenir

### Süre Kontrolleri:
1. ✅ **Modül → Training:** Eğitimlerin toplamı modül süresini aşamaz
2. ✅ **Training → SubTopic:** Alt başlıkların toplamı eğitim süresini aşamaz
3. ✅ **Publish Kontrolü:** Alt başlıkların toplamı eğitim süresine EŞİT olmalı

**Sistem artık kusursuz! 🎉**
