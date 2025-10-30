using LMS_Project.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS_Project.Domain.Entities;
public class Training : BaseEntity
{
    public Guid ModuleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalDurationSeconds { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; } = false;
    public bool IsPublished { get; set; } = false;
    public DateTime? PublishedDate { get; set; }
    public string? PublishedBy { get; set; }
    public bool IsCompleted { get; set; } = false;
    public string? ThumbnailPath { get; set; }
    public string? VideoIntroPath { get; set; }

    // Navigation Properties
    public virtual Module Module { get; set; } = null!;
    public virtual ICollection<SubTopic> SubTopics { get; set; } = new List<SubTopic>();
    public virtual ICollection<UserTraining> UserTrainings { get; set; } = new List<UserTraining>();
    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}

