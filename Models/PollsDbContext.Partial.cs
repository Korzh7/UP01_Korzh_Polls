namespace UP01_Korzh_Polls.Models;

public partial class PollsDbContext
{
    private static PollsDbContext? _context;

    public static PollsDbContext GetContext()
    {
        if (_context == null)
        {
            _context = new PollsDbContext();
        }
        return _context;
    }
}
