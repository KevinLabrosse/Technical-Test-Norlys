using Microsoft.AspNetCore.Mvc;
using TechnicalTestNorlys.Entities;
using TechnicalTestNorlys.Handlers;
using TechnicalTestNorlys.Validators;

namespace TechnicalTestNorlys.API;

[Route("/add-person")]
[ApiController]
public class AddPersonEndpoint : ControllerBase
{
    private readonly ILogger<AddPersonEndpoint> _logger;
    private readonly PersonHandler _handler;
    private static readonly AddPersonValidator Validator = new();
    public AddPersonEndpoint(ILogger<AddPersonEndpoint> logger, PersonHandler handler)
    {
        _handler = handler;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AddPerson([FromBody] AddPersonRequest req)
    {
        try
        {
            _logger.LogInformation("[Controller] Received add person request for {FirstName}", req.FirstName);
            
            var validation = Validator.Validate(req);
            
            if (!validation.IsValid)
            {
                _logger.LogWarning("[Controller] Validation failed with {ErrorCount} errors: {Errors}",
                    validation.Errors.Count,
                string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));                
                return BadRequest(new
                {
                    errors = validation.Errors.Select(e => e.ErrorMessage)
                });
            }
            
            var person = await _handler.AddPersonAsync(req);
            _logger.LogInformation("[Controller] Successfully added person with ID {PersonId}", person.Id);
            
            return Ok(person);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Controller] Error occurred while adding person");
            return StatusCode(500, "An error occurred while processing the 'AddPerson' request.");
        }
    }
}