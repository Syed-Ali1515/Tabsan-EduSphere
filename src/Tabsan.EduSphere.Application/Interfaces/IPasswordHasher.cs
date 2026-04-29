namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Application-layer contract for password hashing operations.
/// Separates the Application layer from PBKDF2 / BCrypt implementation details.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Hashes a plain-text password and returns the storable hash string.</summary>
    string Hash(string password);

    /// <summary>Returns true when the provided plain-text matches the stored hash.</summary>
    bool Verify(string storedHash, string providedPassword);
}
