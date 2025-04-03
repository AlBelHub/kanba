using backend.Models;

namespace backend.Helpers;

public interface ITokenGenerator
{
    public string GenerateToken(UserModel user);
}