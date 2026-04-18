using QuantityMeasurementAppModelLayer.Entities;
using QuantityMeasurementAppRepoLayer.Data;
using QuantityMeasurementAppRepoLayer.Interfaces;

namespace QuantityMeasurementAppRepoLayer.Implementations;

/// <summary>
/// UC18: Entity Framework Core implementation of <see cref="IUserRepository"/>.
/// Scoped lifetime — one instance per HTTP request via ASP.NET Core DI.
/// </summary>
public class EFCoreUserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public EFCoreUserRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // ── FindByEmail ───────────────────────────────────────────────────

    /// <summary>
    /// Looks up a user by email (case-sensitive, uses the unique DB index).
    /// Returns null when no match is found.
    /// </summary>
    // public UserEntity? FindByEmail(string email)
    //     => _context.Users.FirstOrDefault(u => u.Email == email);

    public UserEntity? FindByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return _context.Users
            .FirstOrDefault(u => u.Email != null && u.Email.ToLower() == email.ToLower());
    }

    // ── FindById ──────────────────────────────────────────────────────

    /// <summary>
    /// Looks up a user by primary key.
    /// Returns null when no match is found.
    /// </summary>
    public UserEntity? FindById(long id)
        => _context.Users.FirstOrDefault(u => u.Id == id);

    // ── Save ──────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts a new user and returns the saved entity with the
    /// database-generated <c>Id</c> populated.
    /// </summary>
    public UserEntity Save(UserEntity user)
    {
        ArgumentNullException.ThrowIfNull(user);
        _context.Users.Add(user);
        _context.SaveChanges();
        return user;
    }

    // ── Update ────────────────────────────────────────────────────────

    /// <summary>
    /// Persists changes to an existing user (e.g. <c>LastLoginAt</c>).
    /// </summary>
    public void Update(UserEntity user)
    {
        ArgumentNullException.ThrowIfNull(user);
        _context.Users.Update(user);
        _context.SaveChanges();
    }
}
