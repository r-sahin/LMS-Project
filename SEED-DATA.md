# 🌱 Seed Data Dokümantasyonu

## 📋 Genel Bakış

Uygulama ilk çalıştırıldığında otomatik olarak **varsayılan kullanıcılar, roller ve izinler** oluşturulur.

## 👥 Varsayılan Kullanıcılar

### 1. 👨‍💼 Admin (Yönetici)
```
Email: admin@lms.com
Password: Admin123!
Role: Admin
```

**Yetkiler:**
- ✅ Tüm modül/eğitim/alt başlık CRUD işlemleri
- ✅ Tüm kullanıcı yönetimi
- ✅ Tüm talep onaylama/reddetme
- ✅ Duyuru yönetimi
- ✅ Sistem yönetimi

### 2. 👨‍⚖️ Moderator (Onaylayıcı)
```
Email: moderator@lms.com
Password: Moderator123!
Role: Moderator
```

**Yetkiler:**
- ✅ Modül kayıt taleplerini görüntüleme
- ✅ Talepleri onaylama
- ✅ Talepleri reddetme
- ✅ Student yetkilerinin tümü

### 3. 👨‍🎓 Student (Öğrenci)
```
Email: student@lms.com
Password: Student123!
Role: Student
```

**Yetkiler:**
- ✅ Kayıtlı olduğu modülleri görüntüleme
- ✅ Eğitimlere erişim
- ✅ İlerleme kaydı
- ✅ Modül kaydı için talep oluşturma
- ✅ Bildirimlerini görüntüleme

## 🎭 Oluşturulan Roller

### 1. Admin
- Tüm izinlere sahip
- Sistem yönetimi
- 13 permission

### 2. Moderator
- Modül talep yönetimi
- 3 permission (ModuleRequests.*)

### 3. Student
- Eğitim erişimi
- 0 permission (default kullanıcı)

## 🔐 Oluşturulan Permissions

```
Modules.View          → Modülleri görüntüleme
Modules.Create        → Modül oluşturma
Modules.Update        → Modül güncelleme
Modules.Delete        → Modül silme

Trainings.Create      → Eğitim oluşturma
Trainings.Update      → Eğitim güncelleme
Trainings.Delete      → Eğitim silme

ModuleRequests.Approve → Talepleri onaylama
ModuleRequests.Reject  → Talepleri reddetme
ModuleRequests.View    → Tüm talepleri görüntüleme

Announcements.Create   → Duyuru oluşturma
Announcements.Update   → Duyuru güncelleme
Announcements.Delete   → Duyuru silme
```

## 🚀 Kullanım

### İlk Giriş

1. **Uygulamayı çalıştır:**
   ```bash
   dotnet run --project src/LMS-Project.API
   ```

2. **Scalar UI'ya git:**
   ```
   https://localhost:5001/scalar/v1
   ```

3. **Login endpoint'ini kullan:**
   ```
   POST /api/auth/login
   ```

4. **Admin ile giriş yap:**
   ```json
   {
     "email": "admin@lms.com",
     "password": "Admin123!"
   }
   ```

5. **Token'ı kopyala** ve Authorization header'a ekle:
   ```
   Authorization: Bearer {token}
   ```

### Scalar UI Kullanımı

**Swagger yerine Scalar kullanıyoruz:**
- ✅ Modern ve kullanıcı dostu arayüz
- ✅ Otomatik request/response örnekleri
- ✅ JWT token yönetimi
- ✅ Koyu tema
- ✅ Try It Out özelliği

**URL:** `https://localhost:5001/scalar/v1`

### Test Senaryosu

```bash
# 1. Admin giriş yapar
POST /api/auth/login
Body: { "email": "admin@lms.com", "password": "Admin123!" }
→ Token alır

# 2. Modül oluşturur
POST /api/admin/adminmodules
Authorization: Bearer {admin_token}
Body: { "name": ".NET Core", ... }

# 3. Student giriş yapar
POST /api/auth/login
Body: { "email": "student@lms.com", "password": "Student123!" }

# 4. Student modülleri göremez (kayıtlı değil)
GET /api/modules
→ Boş liste

# 5. Student kayıt talebinde bulunur
POST /api/modulerequests
Body: { "moduleId": "...", "requestReason": "..." }

# 6. Moderator talebi onaylar
POST /api/moderator/moderatormodulerequests/{id}/approve
Authorization: Bearer {moderator_token}

# 7. Student artık modülü görebilir
GET /api/modules
→ Modül listede
```

## 🔄 Seed Data Yeniden Oluşturma

Eğer seed data'yı temizleyip yeniden oluşturmak istersen:

### 1. Database'i Sil
```bash
dotnet ef database drop --project src/LMS-Project.Infrastructure --startup-project src/LMS-Project.API --force
```

### 2. Yeniden Oluştur
```bash
dotnet ef database update --project src/LMS-Project.Infrastructure --startup-project src/LMS-Project.API
```

### 3. Uygulamayı Çalıştır
```bash
dotnet run --project src/LMS-Project.API
```
→ Seed data otomatik oluşturulur!

## ⚙️ Seed Data Nasıl Çalışır?

### DbInitializer.cs
```csharp
public class DbInitializer
{
    public async Task SeedAsync()
    {
        // 1. Database'de veri var mı kontrol et
        if (await _context.Users.AnyAsync())
            return; // Zaten seed yapılmış

        // 2. Roller oluştur
        // 3. Permissions oluştur
        // 4. Role-Permission ilişkileri
        // 5. Kullanıcılar oluştur
        // 6. User-Role ilişkileri
        // 7. SaveChanges
    }
}
```

### Program.cs
```csharp
// Seed Database
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = services.GetRequiredService<DbInitializer>();
    await dbInitializer.SeedAsync();
}
```

## 📝 Notlar

- ✅ Seed data **sadece bir kez** çalışır (database boşsa)
- ✅ Şifreler **hash'lenerek** saklanır (PBKDF2)
- ✅ Tüm kullanıcılar **aktif** durumda
- ✅ Console'a bilgi mesajı yazdırılır
- ✅ Hata durumunda log kaydı

## 🎨 Scalar vs Swagger

### Swagger (Eski)
```
URL: https://localhost:5001/swagger
```

### Scalar (Yeni)
```
URL: https://localhost:5001/scalar/v1
```

**Scalar Avantajları:**
- 🎨 Modern, temiz UI
- 🚀 Daha hızlı
- 🔐 Daha iyi JWT yönetimi
- 📱 Responsive design
- 🌙 Dark mode default

## ✅ Kontrol Listesi

Uygulama çalıştığında console'da göreceksin:

```
✅ Seed Data başarıyla oluşturuldu!

📝 Varsayılan Kullanıcılar:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
👨‍💼 Admin:
   Email: admin@lms.com
   Password: Admin123!

👨‍⚖️ Moderator:
   Email: moderator@lms.com
   Password: Moderator123!

👨‍🎓 Student:
   Email: student@lms.com
   Password: Student123!
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

## 🎉 Özet

Artık uygulamayı çalıştırdığında:
1. ✅ Database otomatik oluşturulur
2. ✅ Seed data otomatik yüklenir
3. ✅ 3 kullanıcı hazır
4. ✅ Scalar UI kullanıma hazır
5. ✅ Admin ile hemen giriş yapabilirsin!

**Swagger YOK, Scalar VAR!** 🚀
