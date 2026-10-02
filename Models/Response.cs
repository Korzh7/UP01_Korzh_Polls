using System;
using System.Collections.Generic;

namespace UP01_Korzh_Polls.Models;

public partial class Response
{
    public int Id { get; set; }

    public int PollId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Poll Poll { get; set; } = null!;

    public virtual UserAccount User { get; set; } = null!;
}
