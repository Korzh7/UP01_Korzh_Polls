namespace UP01_Korzh_Polls.Models;

public partial class PollsDbContext
{
    private static PollsDbContext? _context;

    /// <summary>
    /// Возвращает статический экземпляр контекста базы данных (паттерн Singleton).
    /// </summary>
    /// <returns>Экземпляр <see cref="PollsDbContext"/>.</returns>
    public static PollsDbContext GetContext()
    {
        if (_context == null)
        {
            _context = new PollsDbContext();
        }
        return _context;
    }
}
