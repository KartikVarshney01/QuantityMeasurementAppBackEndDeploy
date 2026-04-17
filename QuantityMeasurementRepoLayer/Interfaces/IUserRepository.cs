using QuantityMeasurementAppModelLayer.Entities;

namespace QuantityMeasurementAppRepoLayer.Interfaces;

/// <summary>
/// UC18 repository contract for user persistence.
/// Implemented by <c>EFCoreUserRepository</c>.
/// </summary>
public interface IUserRepository
{
    /// <summary>Finds a user by email address. Returns null when not found.</summary>
    UserEntity? FindByEmail(string email);

    /// <summary>Finds a user by their database ID. Returns null when not found.</summary>
    UserEntity? FindById(long id);

    /// <summary>Persists a new user and returns the saved entity (with Id populated).</summary>
    UserEntity Save(UserEntity user);

    /// <summary>Updates an existing user record (e.g. LastLoginAt).</summary>
    void Update(UserEntity user);
}
