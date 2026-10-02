using System;
using System.Collections.Generic;

namespace UP01_Korzh_Polls.Models;

public partial class Question
{
    public int Id { get; set; }

    public int PollId { get; set; }

    public string Text { get; set; } = null!;

    public string Type { get; set; } = null!;

    public int OrderNumber { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Poll Poll { get; set; } = null!;

    public virtual ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();
}
