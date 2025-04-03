using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

public abstract class KanbaControllerBase : ControllerBase
{
    protected readonly IDbConnection _db;
    public KanbaControllerBase(IDbConnection db)
    {
        _db = db;
    }    
}