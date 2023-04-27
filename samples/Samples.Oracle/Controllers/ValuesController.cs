using Microsoft.AspNetCore.Mvc;

namespace Samples.Oracle.Controllers;

[ApiController, Route("api/[controller]")]
public class ValuesController : ControllerBase
{
    [HttpGet(nameof(Get))]
    public string Get()
    {
        return "hello";
    }
}
