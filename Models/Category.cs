using System;
using System.Collections.Generic;

namespace UP01_Korzh_Polls.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Poll> Polls { get; set; } = new List<Poll>();
}
