# LMS Projesi - Mimari Dokümantasyon

## 📐 Clean Architecture Katmanları

Bu proje **Clean Architecture** (Onion Architecture) prensiplerine göre tasarlanmıştır.

```
┌─────────────────────────────────────────────────────────┐
│                         API                             │
│                   (Presentation)                        │
│  Controllers, Middleware, Program.cs                    │
└────────────────┬────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────┐
│                   Infrastructure                        │
│              (External Concerns)                        │
│  DbContext, Repositories, Services, JWT                 │
└────────────────┬────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────┐
│                    Application                          │
│                   (Use Cases)                           │
│  CQRS, Handlers, DTOs, Interfaces, Behaviors            │
└────────────────┬────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────┐
│                      Domain                             │
│                  (Business Logic)                       │
│  Entities, Interfaces, Common                           │
└─────────────────────────────────────────────────────────┘
```

## 🎯 Bağımlılık Kuralı (Dependency Rule)

> **Altın Kural:** Bağımlılıklar her zaman içe doğru işaret eder!

- ✅ **Domain** → Hiçbir şeye bağımlı değil
- ✅ **Application** → Sadece Domain'e bağımlı
- ✅ **Infrastructure** → Domain ve Application'a bağımlı
- ✅ **API** → Tüm katmanlara bağımlı (sadece composition root olarak)

## 📦 Katman Detayları

### 1. Domain Katmanı (Core)

**Sorumluluğu:** İş kuralları ve domain modelleri

**İçeriği:**
```
Domain/
├── Entities/           # Domain entity'leri
│   ├── Module.cs
│   ├── Training.cs
│   ├── SubTopic.cs
│   ├── UserProgress.cs  ⭐ En kritik entity
│   ├── User.cs
│   └── Certificate.cs
├── Interfaces/         # Repository interface'leri
│   ├── IRepository.cs
│   ├── IUnitOfWork.cs
│   ├── IModuleRepository.cs
│   └── IUserProgressRepository.cs  ⭐
└── Common/
    ├── BaseEntity.cs   # Tüm entity'lerin base'i
    └── Result.cs       # Generic result pattern
```

**Önemli Özellikler:**
- Hiçbir framework bağımlılığı yok
- Entity'ler saf C# POCO'ları
- Business rule'lar entity'lerde yaşar
- Navigation properties ile ilişkiler

**BaseEntity:**
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }  // Soft delete
    public DateTime? DeletedDate { get; set; }
    public string? DeletedBy { get; set; }
}
```

### 2. Application Katmanı (Use Cases)

**Sorumluluğu:** Uygulama iş mantığı ve orkestrasyon

**İçeriği:**
```
Application/
├── DTOs/                      # Data Transfer Objects
│   ├── ModuleDto.cs
│   ├── UserProgressDto.cs
│   └── CertificateDto.cs
├── Features/                  # CQRS (Vertical Slice)
│   ├── Progress/
│   │   ├── Commands/
│   │   │   ├── UpdateProgressCommand.cs  ⭐
│   │   │   └── UpdateProgressCommandValidator.cs
│   │   └── Queries/
│   │       ├── GetProgressSummaryQuery.cs
│   │       └── GetNextAvailableSubTopicQuery.cs
│   ├── Modules/
│   ├── Auth/
│   └── Trainings/
├── Interfaces/                # Service interface'leri
│   ├── IProgressService.cs    ⭐ Kritik
│   ├── ICertificateService.cs
│   ├── IAuthService.cs
│   └── IJwtService.cs
├── Behaviors/                 # MediatR Pipeline Behaviors
│   ├── ValidationBehavior.cs  # FluentValidation
│   ├── PermissionBehavior.cs  # Yetki kontrolü
│   └── LoggingBehavior.cs     # Loglama
└── DependencyInjection.cs     # Service registration
```

**CQRS Pattern:**
```
Command/Query → Handler → Service → Repository → Database
```

**Örnek Flow:**
```csharp
// 1. Command
public record UpdateProgressCommand(Guid SubTopicId, int DurationSeconds)
    : IRequest<Result<UserProgressDto>>;

// 2. Validator
public class UpdateProgressCommandValidator : AbstractValidator<UpdateProgressCommand>
{
    RuleFor(x => x.DurationSeconds).GreaterThan(0);
}

// 3. Handler
public class UpdateProgressCommandHandler
    : IRequestHandler<UpdateProgressCommand, Result<UserProgressDto>>
{
    private readonly IProgressService _progressService;

    public async Task<Result<UserProgressDto>> Handle(...)
    {
        return await _progressService.UpdateProgressAsync(...);
    }
}
```

**Pipeline Behaviors:**
```
Request → ValidationBehavior → PermissionBehavior → LoggingBehavior → Handler
```

### 3. Infrastructure Katmanı (External)

**Sorumluluğu:** Dış dünya ile etkileşim

**İçeriği:**
```
Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs        # EF Core DbContext
│   ├── Configurations/                # Entity configurations
│   │   ├── ModuleConfiguration.cs
│   │   ├── UserProgressConfiguration.cs  ⭐
│   │   └── ...
│   ├── Repositories/                  # Repository implementations
│   │   ├── Repository.cs              # Generic base
│   │   ├── ModuleRepository.cs
│   │   ├── UserProgressRepository.cs  ⭐
│   │   └── ...
│   └── UnitOfWork.cs                  # Transaction management
├── Services/                          # Service implementations
│   ├── ProgressService.cs             ⭐ Kritik servis
│   ├── CertificateService.cs
│   ├── AuthService.cs
│   ├── JwtService.cs
│   ├── PermissionService.cs
│   ├── PasswordHasher.cs
│   └── CurrentUserService.cs
└── DependencyInjection.cs
```

**Repository Pattern:**
```csharp
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    // CRUD operations with soft delete support
    // Automatic filtering: .Where(e => !e.IsDeleted)
}
```

**Unit of Work Pattern:**
```csharp
public class UnitOfWork : IUnitOfWork
{
    public IModuleRepository Modules { get; }
    public IUserProgressRepository UserProgresses { get; }
    // ... other repositories

    public Task<int> SaveChangesAsync();
    public Task BeginTransactionAsync();
    public Task CommitTransactionAsync();
}
```

### 4. API Katmanı (Presentation)

**Sorumluluğu:** HTTP endpoints ve routing

**İçeriği:**
```
API/
├── Controllers/
│   ├── AuthController.cs
│   ├── ModulesController.cs
│   ├── TrainingsController.cs
│   ├── ProgressController.cs      ⭐
│   └── CertificatesController.cs
├── Program.cs                     # Application startup
├── appsettings.json               # Configuration
└── appsettings.Development.json
```

**Controller Örneği:**
```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost("update")]
    public async Task<IActionResult> UpdateProgress(UpdateProgressCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
```

## 🔄 İstek Akışı (Request Flow)

### Örnek: İlerleme Güncelleme

```
1. HTTP Request
   POST /api/progress/update
   { "subTopicId": "...", "durationSeconds": 150 }
   ↓

2. Controller
   ProgressController.UpdateProgress(command)
   ↓

3. MediatR Pipeline
   → ValidationBehavior (FluentValidation çalışır)
   → PermissionBehavior (Yetki kontrolü)
   → LoggingBehavior (Log kaydı)
   ↓

4. Command Handler
   UpdateProgressCommandHandler.Handle(command)
   ↓

5. Application Service
   IProgressService.UpdateProgressAsync(...)
   ↓

6. Infrastructure Service (ProgressService)
   ├─→ IUnitOfWork.SubTopics.GetByIdAsync()
   ├─→ IUnitOfWork.UserProgresses.GetProgressAsync()
   ├─→ Minimum süre kontrolü ⭐
   ├─→ Sıralı ilerleme kontrolü ⭐
   ├─→ UserProgress entity güncelleme
   └─→ IUnitOfWork.SaveChangesAsync()
   ↓

7. Repository
   Repository<UserProgress>.Update(entity)
   ↓

8. DbContext
   ApplicationDbContext.SaveChangesAsync()
   ↓

9. Database
   SQL Server - UPDATE UserProgresses...
   ↓

10. HTTP Response
    { "isSuccess": true, "data": {...} }
```

## 🎯 Kritik İş Kuralları (Business Rules)

### 1. ProgressService - Süre Kontrolü ⭐

**Kod Konumu:** `Infrastructure/Services/ProgressService.cs`

```csharp
public async Task<Result<UserProgressDto>> UpdateProgressAsync(
    Guid userId, Guid subTopicId, int durationSeconds)
{
    // 1. Erişim kontrolü
    var canAccess = await CanAccessSubTopicAsync(userId, subTopicId);
    if (!canAccess.IsSuccess) return Failure;

    // 2. SubTopic ve UserProgress getir
    var subTopic = await _unitOfWork.SubTopics.GetByIdAsync(subTopicId);
    var progress = await _unitOfWork.UserProgresses.GetProgressAsync(userId, subTopicId);

    // 3. Süre ekle
    if (progress == null)
        progress = new UserProgress { DurationSeconds = durationSeconds };
    else
        progress.DurationSeconds += durationSeconds;

    // 4. ⭐ MİNİMUM SÜRE KONTROLÜ ⭐
    if (progress.DurationSeconds >= subTopic.MinimumDurationSeconds)
    {
        progress.IsCompleted = true;
        progress.CompletedDate = DateTime.UtcNow;

        // Training ve Module tamamlanma kontrolü tetikle
        await CheckAndCompleteTrainingAsync(userId, subTopic.TrainingId);
    }

    await _unitOfWork.SaveChangesAsync();
    return Success(progressDto);
}
```

### 2. Sıralı İlerleme Kontrolü ⭐

```csharp
public async Task<Result<bool>> CanAccessSubTopicAsync(
    Guid userId, Guid subTopicId)
{
    var subTopic = await _unitOfWork.SubTopics.GetByIdAsync(subTopicId);

    // İlk alt başlık her zaman erişilebilir
    if (subTopic.OrderIndex == 0) return Success(true);

    // Önceki alt başlık tamamlanmış mı?
    var hasPreviousCompleted = await _unitOfWork.UserProgresses
        .HasUserCompletedPreviousSubTopicAsync(userId, subTopic.TrainingId, subTopic.OrderIndex);

    if (!hasPreviousCompleted)
        return Failure("Önceki alt başlığı tamamlamadan bu alt başlığa erişemezsiniz.");

    return Success(true);
}
```

### 3. Otomatik Tamamlanma Cascading

```
SubTopic Tamamlandı
    ↓
CheckAndCompleteTraining()
    ├─→ TÜM alt başlıklar tamamlanmış mı?
    ├─→ EVET → UserTraining.IsCompleted = true
    └─→ CheckAndCompleteModule()
            ├─→ TÜM eğitimler tamamlanmış mı?
            └─→ EVET → UserModule.IsCompleted = true
```

### 4. Sertifika Oluşturma Kontrolü

```csharp
public async Task<Result<CertificateDto>> GenerateModuleCertificateAsync(
    Guid userId, Guid moduleId)
{
    // 1. ⭐ MODÜL TAMAMLANMIŞ MI? ⭐
    var isCompleted = await _progressService.IsModuleCompletedAsync(userId, moduleId);
    if (!isCompleted.Data)
        return Failure("Modül henüz tamamlanmadı. Sertifika oluşturulamaz.");

    // 2. Daha önce sertifika verilmiş mi?
    var hasExisting = await _unitOfWork.Certificates
        .HasUserCertificateForModuleAsync(userId, moduleId);
    if (hasExisting)
        return Failure("Bu modül için zaten bir sertifikanız bulunmaktadır.");

    // 3. Sertifika oluştur
    var certificate = new Certificate
    {
        UserId = userId,
        ModuleId = moduleId,
        CertificateNumber = GenerateCertificateNumber("MOD"),
        VerificationCode = GenerateVerificationCode(),
        IssuedDate = DateTime.UtcNow
    };

    await _unitOfWork.Certificates.AddAsync(certificate);
    await _unitOfWork.SaveChangesAsync();

    return Success(certificateDto);
}
```

## 🔐 Güvenlik Mimarisi

### JWT Authentication Flow

```
1. Login Request
   ↓
2. AuthService.LoginAsync()
   ├─→ Kullanıcı doğrulama
   ├─→ Şifre kontrolü (PasswordHasher - PBKDF2)
   └─→ JwtService.GenerateAccessToken()
       ├─→ Claims: UserId, Email, Roles, Permissions
       ├─→ Expiration: 1 hour
       └─→ Signing: HMAC-SHA256
   ↓
3. Response: { accessToken, refreshToken }
```

### Permission Behavior

```csharp
[Authorize("Modules.View")]  // Attribute
public record GetAllModulesQuery : IRequest<Result<List<ModuleDto>>>;

// Pipeline'da otomatik kontrol
public class PermissionBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(...)
    {
        var attributes = request.GetCustomAttributes<AuthorizeAttribute>();
        foreach (var attr in attributes)
        {
            var hasPermission = await _permissionService
                .HasPermissionAsync(_currentUserService.UserId, attr.Permission);
            if (!hasPermission)
                throw new UnauthorizedAccessException();
        }
        return await next();
    }
}
```

## 📊 Veritabanı Stratejisi

### Entity Framework Core

**Migrations:**
```bash
dotnet ef migrations add InitialCreate --project Infrastructure
dotnet ef database update --project Infrastructure
```

**Global Query Filter (Soft Delete):**
```csharp
modelBuilder.Entity<UserProgress>()
    .HasQueryFilter(e => !e.IsDeleted);
```

**Audit Fields (Automatic):**
```csharp
public override Task<int> SaveChangesAsync(...)
{
    foreach (var entry in ChangeTracker.Entries<BaseEntity>())
    {
        if (entry.State == EntityState.Added)
            entry.Entity.CreatedDate = DateTime.UtcNow;
        else if (entry.State == EntityState.Modified)
            entry.Entity.UpdatedDate = DateTime.UtcNow;
    }
    return base.SaveChangesAsync();
}
```

## 🧪 Test Stratejisi

### Unit Test (Önerilen)
```
Application.Tests/
├── Features/
│   └── Progress/
│       └── UpdateProgressCommandHandlerTests.cs
Infrastructure.Tests/
├── Services/
│   └── ProgressServiceTests.cs
```

### Integration Test
```
API.IntegrationTests/
└── Controllers/
    └── ProgressControllerTests.cs
```

## 📈 Performans Optimizasyonları

1. **Include ile N+1 Problem Önleme:**
   ```csharp
   _context.Modules
       .Include(m => m.Trainings)
           .ThenInclude(t => t.SubTopics)
   ```

2. **AsNoTracking (Read-Only):**
   ```csharp
   _context.Modules.AsNoTracking().ToListAsync()
   ```

3. **Pagination (PaginatedResult):**
   ```csharp
   var items = await query
       .Skip((pageNumber - 1) * pageSize)
       .Take(pageSize)
       .ToListAsync();
   ```

4. **Index Stratejisi:**
   ```csharp
   builder.HasIndex(up => new { up.UserId, up.SubTopicId }).IsUnique();
   builder.HasIndex(up => up.IsCompleted);
   ```

## 🎯 Best Practices

1. ✅ **Single Responsibility:** Her sınıf tek bir sorumluluğa sahip
2. ✅ **Dependency Inversion:** Interface'lere bağımlılık
3. ✅ **Separation of Concerns:** Katmanlar arası net ayrım
4. ✅ **CQRS:** Command ve Query ayrımı
5. ✅ **Repository Pattern:** Data access soyutlaması
6. ✅ **Unit of Work:** Transaction yönetimi
7. ✅ **Result Pattern:** Hata yönetimi
8. ✅ **Validation:** FluentValidation ile
9. ✅ **Logging:** Structured logging
10. ✅ **Soft Delete:** Veri kaybı önleme

---

Bu mimari, genişletilebilir, test edilebilir ve bakımı kolay bir sistem sağlar. 🚀
