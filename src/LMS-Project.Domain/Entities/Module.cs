using LMS_Project.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS_Project.Domain.Entities;
public class Module : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPublished { get; set; } = false;
    public DateTime? PublishedDate { get; set; }
    public string? PublishedBy { get; set; }
    public string? ImagePath { get; set; }
    public int EstimatedDurationMinutes { get; set; }

    public virtual ICollection<Training> Trainings { get; set; } = new List<Training>();
    public virtual ICollection<UserModule> UserModules { get; set; } = new List<UserModule>();
    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}

