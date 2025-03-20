namespace backend.Helpers;

public interface IPasswordHasher
{
    public string HashPassword(string password);
    public bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}