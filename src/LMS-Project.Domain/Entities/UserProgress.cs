using LMS_Project.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS_Project.Domain.Entities;
public class UserProgress : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SubTopicId { get; set; }
    public int DurationSeconds { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedDate { get; set; }
    public DateTime LastAccessedDate { get; set; }
    public int AccessCount { get; set; } = 0;
    public bool IsLocked { get; set; } = false;

    // ⭐ Tek cihaz kontrolü için session bilgileri
    public string? SessionId { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceInfo { get; set; } // UserAgent
    public bool IsSessionActive { get; set; } = true;

    public virtual User User { get; set; } = null!;
    public virtual SubTopic SubTopic { get; set; } = null!;
}

