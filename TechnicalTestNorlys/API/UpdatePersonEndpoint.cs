using Microsoft.AspNetCore.Mvc;
using TechnicalTestNorlys.Entities;
using TechnicalTestNorlys.Handlers;
using TechnicalTestNorlys.Validators;

namespace TechnicalTestNorlys.API;

[Route("/update-persons")]
[ApiController]
public class UpdatePersonEndpoint : ControllerBase
{
    private readonly ILogger<UpdatePersonEndpoint> _logger;
    private readonly PersonHandler _handler;
    private static readonly UpdatePersonValidator Validator = new();

    public UpdatePersonEndpoint(ILogger<UpdatePersonEndpoint> logger, PersonHandler handler)
    {
        _handler = handler;
        _logger = logger;
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdatePerson(int id, [FromBody] UpdatePersonRequest req)
    {
        try
        {
            _logger.LogInformation("[Controller] Received request to update person with ID {PersonId}", id);

            var validation = Validator.Validate(req);

            if (!validation.IsValid)
            {
                _logger.LogWarning("[Controller] Validation failed with {ErrorCount} errors: {Errors}",
                    validation.Errors.Count,
                    string.Join(", ", validation.Errors.Select(e => e.ErrorMessage).ToArray()));
                return BadRequest(new
                {
                    errors = validation.Errors.Select(e => e.ErrorMessage)
                });
            }

            var person = await _handler.UpdatePersonAsync(id, req);

            if (person == null)
            {
                _logger.LogWarning("[Controller] Person with ID {PersonId} not found", id);
                return NotFound($"Person with ID {id} not found.");
            }

            _logger.LogInformation("[Controller] Successfully updated person with ID {PersonId}", id);

            return Ok(new
            {
                person.Id,
                person.FirstName,
                person.LastName,
                person.DateOfBirth,
                Office = person.Office.Location
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "[Controller] Invalid operation while updating person with ID {PersonId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Controller] Error occurred while updating person with ID {PersonId}", id);
            return StatusCode(500, "An error occurred while processing the 'UpdatePerson' request.");
        }
    }
}

