using System;
using System.Collections.Generic;

namespace UP01_Korzh_Polls.Models;

public partial class Answer
{
    public int Id { get; set; }

    public int ResponseId { get; set; }

    public int QuestionId { get; set; }

    public int? OptionId { get; set; }

    public string? TextAnswer { get; set; }

    public virtual QuestionOption? Option { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual Response Response { get; set; } = null!;
}
