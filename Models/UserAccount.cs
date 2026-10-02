using System;
using System.Collections.Generic;

namespace UP01_Korzh_Polls.Models;

public partial class UserAccount
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Photo { get; set; }

    public virtual ICollection<Poll> Polls { get; set; } = new List<Poll>();

    public virtual ICollection<Response> Responses { get; set; } = new List<Response>();
}
