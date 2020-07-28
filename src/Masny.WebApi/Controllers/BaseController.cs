using Masny.WebApi.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Masny.WebApi.Controllers
{
    [Controller]
    public abstract class BaseController : ControllerBase
    {
        public Account Account => (Account)HttpContext.Items["Account"];
    }
}
