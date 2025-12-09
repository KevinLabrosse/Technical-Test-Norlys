using Microsoft.AspNetCore.Mvc;
using TechnicalTestNorlys.Handlers;

namespace TechnicalTestNorlys.API;

[Route("/get-persons")]
[ApiController]
public class GetPersonsEndpoint : ControllerBase
{
    private readonly ILogger<GetPersonsEndpoint> _logger;
    private readonly PersonHandler _handler;

    public GetPersonsEndpoint(ILogger<GetPersonsEndpoint> logger, PersonHandler handler)
    {
        _handler = handler;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPersons()
    {
        try
        {
            _logger.LogInformation("[Controller] Received request to get all persons");

            var persons = await _handler.GetAllPersonsAsync();

            _logger.LogInformation("[Controller] Successfully retrieved {Count} persons", persons.Count);

            return Ok(persons.Select(p => new
            {
                p.Id,
                p.FirstName,
                p.LastName,
                p.DateOfBirth,
                Office = p.Office.Location
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Controller] Error occurred while getting all persons");
            return StatusCode(500, "An error occurred while processing the 'GetAllPersons' request.");
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPersonById(int id)
    {
        try
        {
            _logger.LogInformation("[Controller] Received request to get person with ID {PersonId}", id);

            var person = await _handler.GetPersonByIdAsync(id);

            if (person == null)
            {
                _logger.LogWarning("[Controller] Person with ID {PersonId} not found", id);
                return NotFound($"Person with ID {id} not found.");
            }

            _logger.LogInformation("[Controller] Successfully retrieved person with ID {PersonId}", id);

            return Ok(new
            {
                person.Id,
                person.FirstName,
                person.LastName,
                person.DateOfBirth,
                Office = person.Office.Location
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Controller] Error occurred while getting person with ID {PersonId}", id);
            return StatusCode(500, "An error occurred while processing the 'GetPersonById' request.");
        }
    }
}

