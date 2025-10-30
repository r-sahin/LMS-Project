using LMS_Project.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS_Project.Domain.Entities;
public class SubTopic : BaseEntity
{
    public Guid TrainingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MinimumDurationSeconds { get; set; }
    public string ZipFilePath { get; set; } = string.Empty;
    public string HtmlFilePath { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsMandatory { get; set; } = true;
    public string? ThumbnailPath { get; set; }

    // Navigation Properties
    public virtual Training Training { get; set; } = null!;
    public virtual ICollection<UserProgress> UserProgresses { get; set; } = new List<UserProgress>();
}

