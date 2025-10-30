# Bildirimler ve Duyurular API Dokümantasyonu

## 📢 Duyuru Yönetimi (Announcements)

### Admin Endpoints

#### 1. Duyuru Oluştur

```http
POST /api/admin/adminannouncements
Authorization: Bearer {admin_token}
Content-Type: multipart/form-data

Form Data:
- Title: "Yeni Modül Eklendi!" (required, max 200 karakter)
- Content: "C# Advanced modülü platformumuza eklendi..." (required)
- PublishDate: "2024-01-15T10:00:00Z" (required)
- ExpiryDate: "2024-02-15T23:59:59Z" (optional)
- Priority: "High" (required: Low, Normal, High, Urgent)
- TargetRole: "Student" (optional: null = herkese, "Admin", "Student")
- ImageFile: [resim dosyası] (optional, max 10MB, jpg/png/gif/webp)
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Duyuru başarıyla oluşturuldu.",
  "data": "announcement-guid",
  "errors": []
}
```

#### 2. Duyuru Güncelle

```http
PUT /api/admin/adminannouncements/{id}
Authorization: Bearer {admin_token}
Content-Type: multipart/form-data

Form Data:
- Id: "announcement-guid"
- Title: "Güncellenmiş Başlık"
- Content: "Güncellenmiş içerik"
- PublishDate: "2024-01-15T10:00:00Z"
- ExpiryDate: "2024-02-15T23:59:59Z"
- Priority: "Urgent"
- TargetRole: null
- IsActive: true
- ImageFile: [yeni resim] (optional)
```

**Not:** Yeni resim yüklenirse eski otomatik silinir.

#### 3. Duyuru Sil

```http
DELETE /api/admin/adminannouncements/{id}
Authorization: Bearer {admin_token}
```

**Silinen Dosyalar:**
- Duyuru resmi (varsa)
- Veritabanında soft delete

#### 4. Tüm Duyuruları Getir

```http
GET /api/admin/adminannouncements?onlyActive=false
Authorization: Bearer {admin_token}
```

#### 5. ID'ye Göre Duyuru Getir

```http
GET /api/admin/adminannouncements/{id}
Authorization: Bearer {admin_token}
```

---

### Kullanıcı Endpoints

#### 1. Aktif Duyuruları Getir

```http
GET /api/announcements
Authorization: Bearer {user_token}
```

**Filtreler:**
- Sadece aktif duyurular (IsActive = true)
- Yayın tarihi geçmiş (PublishDate <= now)
- Son geçerlilik tarihi geçmemiş (ExpiryDate > now veya null)
- Kullanıcının rolüne uygun (TargetRole = null veya kullanıcının rolü)

**Response:**
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": "guid",
      "title": "Yeni Modül Eklendi!",
      "content": "C# Advanced modülü...",
      "publishDate": "2024-01-15T10:00:00Z",
      "expiryDate": "2024-02-15T23:59:59Z",
      "isActive": true,
      "priority": "High",
      "targetRole": null,
      "imagePath": "/content/announcements/announcement_guid.jpg",
      "createdDate": "2024-01-15T09:00:00Z"
    }
  ]
}
```

#### 2. Duyuru Detayı

```http
GET /api/announcements/{id}
Authorization: Bearer {user_token}
```

---

## 🔔 Bildirim Sistemi (Notifications)

### Kullanıcı Endpoints

#### 1. Bildirimlerimi Getir

```http
GET /api/notifications?onlyUnread=false
Authorization: Bearer {user_token}
```

**Query Parameters:**
- `onlyUnread`: true = sadece okunmamışlar, false = hepsi (default: false)

**Response:**
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": "guid",
      "userId": "user-guid",
      "title": "Modül Tamamlandı!",
      "message": ".NET Core modülünü tamamladınız.",
      "type": "Achievement",
      "isRead": false,
      "readDate": null,
      "relatedEntityType": "Module",
      "relatedEntityId": "module-guid",
      "actionUrl": "/modules/module-guid",
      "createdDate": "2024-01-15T10:00:00Z"
    }
  ]
}
```

**Notification Types:**
- `Info` - Bilgilendirme
- `Success` - Başarı mesajı
- `Warning` - Uyarı
- `Error` - Hata
- `Achievement` - Başarı/Ödül

#### 2. Okunmamış Bildirim Sayısı

```http
GET /api/notifications/unread-count
Authorization: Bearer {user_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": 5
}
```

#### 3. Bildirimi Okundu Olarak İşaretle

```http
POST /api/notifications/{id}/mark-as-read
Authorization: Bearer {user_token}
```

#### 4. Tüm Bildirimleri Okundu Olarak İşaretle

```http
POST /api/notifications/mark-all-as-read
Authorization: Bearer {user_token}
```

#### 5. Bildirimi Sil

```http
DELETE /api/notifications/{id}
Authorization: Bearer {user_token}
```

**Not:** Kullanıcı sadece kendi bildirimlerini işaretleyebilir/silebilir.

---

## 🔐 Şifre Sıfırlama (Password Reset)

### 1. Şifre Sıfırlama Talebi Oluştur

```http
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Eğer email adresiniz sistemde kayıtlıysa, şifre sıfırlama bağlantısı gönderilecektir."
}
```

**Güvenlik Notu:** Email bulunmasa bile aynı mesaj döner (email enumeration saldırılarını önlemek için).

**İşlem Akışı:**
1. Sistem kullanıcıyı email ile bulur
2. Benzersiz reset token üretir (32 byte, URL-safe)
3. Token'ı veritabanına kaydeder (1 saat geçerli)
4. Kullanıcıya email gönderilir

**Email İçeriği:**
```html
Şifre Sıfırlama Bağlantısı:
http://localhost:3000/reset-password?token={resetToken}&email={email}

Bu bağlantı 1 saat süreyle geçerlidir.
```

### 2. Şifreyi Sıfırla

```http
POST /api/auth/reset-password
Content-Type: application/json

{
  "email": "user@example.com",
  "resetToken": "abc123...",
  "newPassword": "NewSecureP@ss123"
}
```

**Şifre Gereksinimleri:**
- En az 8 karakter
- En az 1 büyük harf
- En az 1 küçük harf
- En az 1 rakam
- En az 1 özel karakter

**Response:**
```json
{
  "isSuccess": true,
  "message": "Şifreniz başarıyla sıfırlandı. Artık yeni şifrenizle giriş yapabilirsiniz."
}
```

**Hata Durumları:**
```json
{
  "isSuccess": false,
  "message": "Geçersiz veya süresi dolmuş sıfırlama token'ı.",
  "errors": ["Token geçersiz"]
}
```

---

## 📧 Email Servisi

### Konfigürasyon (appsettings.json)

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@lms.com",
    "FromName": "LMS Platform"
  },
  "Frontend": {
    "Url": "http://localhost:3000"
  }
}
```

### Gmail App Password Oluşturma

1. Google Hesap Ayarları → Güvenlik
2. 2-Adımlı Doğrulama'yı etkinleştir
3. Uygulama Şifreleri → LMS için şifre oluştur
4. Oluşturulan şifreyi `SmtpPassword` olarak kullan

### Email Şablonları

#### 1. Şifre Sıfırlama Emaili
- Konu: "Şifre Sıfırlama Talebi"
- İçerik: Reset linki + uyarılar
- Geçerlilik: 1 saat

#### 2. Hoş Geldiniz Emaili
- Konu: "LMS Platformuna Hoş Geldiniz!"
- İçerik: Karşılama mesajı + platform linki

#### 3. Sertifika Emaili
- Konu: "Tebrikler! Sertifikanızı Kazandınız"
- İçerik: Tebrik mesajı + sertifika linki

---

## 🔄 Bildirim Oluşturma (Programmatic)

Sistem içinden bildirim oluşturmak için:

```csharp
var command = new CreateNotificationCommand(
    UserId: userGuid,
    Title: "Modül Tamamlandı!",
    Message: ".NET Core modülünü başarıyla tamamladınız.",
    Type: "Achievement",
    RelatedEntityType: "Module",
    RelatedEntityId: moduleGuid,
    ActionUrl: $"/modules/{moduleGuid}"
);

var result = await _mediator.Send(command);
```

**Kullanım Senaryoları:**
- Alt başlık tamamlandığında
- Eğitim tamamlandığında
- Modül tamamlandığında
- Sertifika kazanıldığında
- Admin duyurusu yapıldığında

---

## 📊 Dosya Yönetimi

### Duyuru Resimleri

**Yükleme:**
- Format: jpg, jpeg, png, gif, webp
- Max Boyut: 10 MB
- Konum: `/content/announcements/announcement_{guid}.{ext}`

**Silme:**
- Duyuru silindiğinde resim otomatik silinir
- Duyuru güncellendiğinde eski resim silinir

---

## 🧪 Test Senaryoları

### Duyuru Testi

```bash
# 1. Admin duyuru oluşturur
POST /api/admin/adminannouncements
{
  "title": "Bakım Bildirisi",
  "content": "Sistem bakımı 20:00-22:00 arasında yapılacaktır.",
  "publishDate": "2024-01-15T18:00:00Z",
  "expiryDate": "2024-01-15T22:00:00Z",
  "priority": "Urgent",
  "targetRole": null
}

# 2. Kullanıcılar duyuruyu görür
GET /api/announcements
→ Duyuru listede görünür

# 3. Son geçerlilik tarihi geçince
GET /api/announcements
→ Duyuru artık listede görünmez
```

### Bildirim Testi

```bash
# 1. Kullanıcı alt başlığı tamamlar
POST /api/progress
→ Sistem otomatik bildirim oluşturur

# 2. Kullanıcı bildirimlerini kontrol eder
GET /api/notifications?onlyUnread=true
→ Yeni bildirim görünür

# 3. Kullanıcı bildirimi okur
POST /api/notifications/{id}/mark-as-read
→ IsRead = true olur

# 4. Okunmamış sayısını kontrol eder
GET /api/notifications/unread-count
→ Sayı 1 azalır
```

### Şifre Sıfırlama Testi

```bash
# 1. Kullanıcı şifresini unutur
POST /api/auth/forgot-password
{ "email": "user@example.com" }
→ Email gönderilir

# 2. Kullanıcı emaildeki linke tıklar
→ Frontend: /reset-password?token=xxx&email=yyy

# 3. Kullanıcı yeni şifre girer
POST /api/auth/reset-password
{
  "email": "user@example.com",
  "resetToken": "xxx",
  "newPassword": "NewP@ssw0rd"
}
→ Şifre güncellenir

# 4. Kullanıcı yeni şifreyle giriş yapar
POST /api/auth/login
{ "email": "user@example.com", "password": "NewP@ssw0rd" }
→ Başarılı giriş
```

---

## ⚠️ Önemli Notlar

### Güvenlik

1. **Email Enumeration Koruması:**
   - Forgot password endpoint her zaman aynı mesajı döner
   - Sistemde olmayan emailler için de "başarılı" mesajı gösterilir

2. **Token Güvenliği:**
   - Reset token'lar kriptografik olarak güvenli (32 byte random)
   - URL-safe encoding (+, /, = karakterleri temizlenir)
   - 1 saat sonra otomatik expire olur

3. **Rate Limiting:**
   - Forgot password endpoint'e rate limiting uygulanmalı
   - Önerilen: 5 istek / 15 dakika / IP

4. **Email Doğrulama:**
   - SMTP bilgileri production'da environment variable olarak saklanmalı
   - Test ortamında email simülasyonu kullanılabilir

### Performans

1. **Bildirim Sorguları:**
   - Index'ler: UserId, IsRead, CreatedDate
   - Sayfalama kullanılmalı (büyük bildirim listeleri için)

2. **Duyuru Sorguları:**
   - Index'ler: PublishDate, Priority, IsActive
   - Cache kullanılabilir (10 dakika TTL önerilir)

3. **Email Gönderimi:**
   - Asenkron kuyruk sistemi kullanılmalı (production için)
   - Şu an senkron, büyük ölçekte sorun yaratabilir

---

**Tüm özellikler eksiksiz ve çalışır durumda!** 🎉
