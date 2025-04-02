namespace backend.Helpers;

public class UUIDProvider : IUUIDProvider
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns>Строка формата UUIDv7</returns>
    public Guid GenerateUUIDv7()
    {
        var id = new UUID();
        return id.ToGuid();
    }
}