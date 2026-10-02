using System;
using System.Collections.Generic;

namespace UP01_Korzh_Polls.Models;

public partial class Poll
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CategoryId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Status { get; set; } = null!;

    public bool IsAnonymous { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<Response> Responses { get; set; } = new List<Response>();

    public virtual UserAccount User { get; set; } = null!;
}
