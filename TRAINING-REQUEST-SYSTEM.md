# 🎓 Eğitim Talep Sistemi (Training Request System)

## 📋 YENİ SİSTEM AKIŞI

### Eski Sistem (Değişti):
```
1. Student modül için talepte bulunur
2. Moderator modül talebini onaylar
3. UserModule oluşturulur
4. Kullanıcı TÜM eğitimlere erişir ❌
```

### Yeni Sistem (Profesyonel):
```
1. Admin modülü publish eder (IsPublished = true)
2. Student YAYINLANMIŞ modül için talepte bulunur
3. Moderator modül talebini onaylar
4. UserModule oluşturulur (modül görünür olur)
5. Student modüldeki YAYINLANMIŞ eğitimleri görür AMA KİLİTLİ
6. Student istediği eğitim için talepte bulunur
7. Moderator eğitim talebini onaylar
8. UserTraining oluşturulur
9. Student o eğitime erişir ✅
```

---

## 🆕 Yeni Özellikler

### 1. Module IsPublished
```csharp
public class Module
{
    public bool IsPublished { get; set; } = false;
    public DateTime? PublishedDate { get; set; }
    public string? PublishedBy { get; set; }
}
```

**Admin işlemleri:**
- POST `/api/admin/adminmodules/{id}/publish` - Modülü yayınla
- POST `/api/admin/adminmodules/{id}/unpublish` - Yayından kaldır

**Kurallar:**
- Sadece yayınlanmış modüller öğrenciler tarafından görülebilir
- Modül publish edilmeden talep edilemez

---

### 2. TrainingRequest Entity
```csharp
public class TrainingRequest
{
    public Guid UserId { get; set; }
    public Guid TrainingId { get; set; }
    public string RequestReason { get; set; }
    public string Status { get; set; } // Pending, Approved, Rejected
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? ReviewNote { get; set; }
}
```

---

## 📊 İş Akışı Detayı

### Adım 1: Admin Modülü Yayınlar

```http
POST /api/admin/adminmodules/{module-id}/publish
Authorization: Bearer {admin_token}
```

**Kontroller:**
- ✅ Modülde en az 1 yayınlanmış eğitim olmalı
- ✅ Eğitimlerin toplam süresi modül süresine eşit olmalı

**Response:**
```json
{
  "isSuccess": true,
  "message": "Modül başarıyla yayınlandı! 3 eğitim ile toplamda 180 dakika."
}
```

---

### Adım 2: Student Yayınlanmış Modülleri Görür

```http
GET /api/modules/available
Authorization: Bearer {student_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": [
    {
      "moduleId": "...",
      "moduleName": "ASP.NET Core",
      "description": "...",
      "totalTrainings": 3,
      "estimatedDurationMinutes": 180,
      "hasPendingRequest": false
    }
  ]
}
```

---

### Adım 3: Student Modül İçin Talepte Bulunur

```http
POST /api/modulerequests
Content-Type: application/json

{
  "moduleId": "module-guid",
  "requestReason": "Bu modülü almak istiyorum"
}
```

---

### Adım 4: Moderator Modül Talebini Onaylar

```http
POST /api/moderator/moderatormodulerequests/{request-id}/approve
```

**Yeni Davranış:**
- ✅ UserModule oluşturulur
- ✅ Bildirim gönderilir
- ❌ UserTraining OLUŞTURULMAZ (önemli değişiklik!)

---

### Adım 5: Student Modülü Görür, Eğitimler Kilitli

```http
GET /api/modules/{module-id}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "id": "...",
    "name": "ASP.NET Core",
    "trainings": [
      {
        "id": "training-1",
        "name": "Introduction",
        "isLocked": true,
        "hasPendingRequest": false,
        "lockReason": "Bu eğitime erişim için talep oluşturmalısınız"
      },
      {
        "id": "training-2",
        "name": "Advanced",
        "isLocked": true,
        "hasPendingRequest": false
      }
    ]
  }
}
```

---

### Adım 6: Student Eğitim İçin Talepte Bulunur

```http
POST /api/trainingrequests
Content-Type: application/json

{
  "trainingId": "training-1",
  "requestReason": "Bu eğitimi almak istiyorum"
}
```

**Kontroller:**
- ✅ Eğitim yayınlanmış olmalı
- ✅ Modül kullanıcıya atanmış olmalı
- ✅ Daha önce pending talep olmamalı
- ✅ Zaten onaylanmış eğitim olmamalı

---

### Adım 7: Moderator Eğitim Talebini Onaylar

```http
POST /api/moderator/trainingrequests/{request-id}/approve
```

**Yapılanlar:**
- ✅ TrainingRequest status = "Approved"
- ✅ UserTraining oluşturulur
- ✅ Bildirim gönderilir

---

### Adım 8: Student Eğitime Erişir

```http
GET /api/trainings/{training-id}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "id": "training-1",
    "name": "Introduction",
    "subTopics": [...],
    "isLocked": false
  }
}
```

---

## 🔐 Yetki Kontrolleri

### GetModuleByIdQuery (Güncellenecek):
```csharp
// 1. Modül yayınlanmış mı?
if (!module.IsPublished)
    return Failure("Modül henüz yayınlanmamış.");

// 2. Kullanıcıya atanmış mı?
if (userModule == null)
    return Failure("Bu modüle erişim yetkiniz yok.");

// 3. Eğitimler kilitli mi?
foreach (var training in module.Trainings.Where(t => t.IsPublished))
{
    var userTraining = await GetUserTrainingAsync(userId, training.Id);
    var hasPendingRequest = await HasPendingTrainingRequestAsync(userId, training.Id);

    trainingDtos.Add(new TrainingDto
    {
        ...
        IsLocked = userTraining == null,
        HasPendingRequest = hasPendingRequest,
        LockReason = GetLockReason(userTraining, hasPendingRequest)
    });
}
```

### GetTrainingByIdQuery (Güncellenecek):
```csharp
// 1. Eğitim yayınlanmış mı?
if (!training.IsPublished)
    return Failure("Eğitim henüz yayınlanmamış.");

// 2. Modül kullanıcıya atanmış mı?
if (userModule == null)
    return Failure("Bu modüle erişim yetkiniz yok.");

// 3. Eğitime erişim var mı?
var userTraining = await GetUserTrainingAsync(userId, trainingId);
if (userTraining == null)
    return Failure("Bu eğitime erişim yetkiniz yok. Lütfen eğitim için talepte bulunun.");
```

---

## 📝 Yeni Endpointler

### Student Endpointleri:
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/modules/available` | Yayınlanmış, talep edilebilir modüller |
| POST | `/api/trainingrequests` | Eğitim talebi oluştur |
| GET | `/api/trainingrequests/my-requests` | Kendi eğitim taleplerimi gör |

### Moderator Endpointleri:
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/moderator/trainingrequests/pending` | Bekleyen eğitim talepleri |
| POST | `/api/moderator/trainingrequests/{id}/approve` | Eğitim talebini onayla |
| POST | `/api/moderator/trainingrequests/{id}/reject` | Eğitim talebini reddet |

### Admin Endpointleri:
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| POST | `/api/admin/adminmodules/{id}/publish` | Modülü yayınla |
| POST | `/api/admin/adminmodules/{id}/unpublish` | Modülü yayından kaldır |

---

## 🎯 Avantajlar

### Eski Sistem:
- ❌ Modül onayı = Tüm eğitimler açık
- ❌ Kullanıcı gereksiz eğitimlere erişiyor
- ❌ Spam riski var
- ❌ Moderator kontrolü az

### Yeni Sistem:
- ✅ Modül onayı = Sadece modül görünür
- ✅ Eğitim onayı = Sadece o eğitim açık
- ✅ Granular kontrol (eğitim bazında)
- ✅ Spam önlenir
- ✅ İstatistik tutulur (hangi eğitimler talep ediliyor)
- ✅ Moderator her eğitim için karar verebilir
- ✅ Admin publish kontrolü ile kalite kontrolü

---

## 🔄 Migration Gerekli

**Yeni alanlar:**
```sql
-- Module tablosuna
ALTER TABLE Modules ADD IsPublished BIT NOT NULL DEFAULT 0;
ALTER TABLE Modules ADD PublishedDate DATETIME2 NULL;
ALTER TABLE Modules ADD PublishedBy NVARCHAR(MAX) NULL;

-- Yeni tablo
CREATE TABLE TrainingRequests (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    TrainingId UNIQUEIDENTIFIER NOT NULL,
    RequestReason NVARCHAR(MAX),
    Status NVARCHAR(50),
    ReviewedBy UNIQUEIDENTIFIER NULL,
    ReviewedDate DATETIME2 NULL,
    ReviewNote NVARCHAR(MAX) NULL,
    ...
);
```

---

## ✅ Yapılacaklar Özeti

1. ✅ Module Entity - IsPublished eklendi
2. ✅ TrainingRequest Entity oluşturuldu
3. ⏳ TrainingRequestRepository implement edilecek
4. ⏳ PublishModule/UnpublishModule commands
5. ⏳ RequestTrainingEnrollment command
6. ⏳ ApproveTrainingRequest command
7. ⏳ RejectTrainingRequest command
8. ⏳ GetAvailableModulesQuery (yayınlanmış modüller)
9. ⏳ GetAvailableTrainingsQuery (kilitli/açık durumları)
10. ⏳ ApproveModuleRequestCommand güncelle (UserTraining oluşturma)
11. ⏳ GetModuleByIdQuery güncelle (eğitim kilitleme)
12. ⏳ GetTrainingByIdQuery güncelle (erişim kontrolü)
13. ⏳ Admin/Moderator/Student controller'lar
14. ⏳ Migration oluştur

**Devam ediyorum!** 🚀
