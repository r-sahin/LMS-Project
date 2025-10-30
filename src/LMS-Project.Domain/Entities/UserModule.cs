using LMS_Project.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS_Project.Domain.Entities;
public class UserModule : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ModuleId { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public decimal CompletionPercentage { get; set; } = 0;
    public DateTime? LastAccessedDate { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Module Module { get; set; } = null!;
}

