using Microsoft.AspNetCore.Mvc;

namespace TechnicalTestNorlys;

[Route("/controller")]
[ApiController]
public class Controller : ControllerBase
{
    public readonly ILogger<Controller> _logger;
    public Controller(ILogger<Controller> logger)
    {
        _logger = logger;
    }
}