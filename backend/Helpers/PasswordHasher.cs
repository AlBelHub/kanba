namespace backend.Helpers;
using static BCrypt.Net.BCrypt;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentNullException("Password cannot be null or empty.");
        }
        
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        return Verify(hashedPassword, providedPassword);
    }
}