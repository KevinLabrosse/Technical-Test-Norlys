using Microsoft.AspNetCore.Mvc;
using TechnicalTestNorlys.Handlers;

namespace TechnicalTestNorlys.API;

[Route("/delete-person")]
[ApiController]
public class DeletePersonEndpoint : ControllerBase
{
    private readonly ILogger<DeletePersonEndpoint> _logger;
    private readonly PersonHandler _handler;

    public DeletePersonEndpoint(ILogger<DeletePersonEndpoint> logger, PersonHandler handler)
    {
        _handler = handler;
        _logger = logger;
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePerson(int id)
    {
        try
        {
            _logger.LogInformation("[Controller] Received request to delete person with ID {PersonId}", id);

            var person = await _handler.DeletePersonAsync(id);

            if (person == null)
            {
                _logger.LogWarning("[Controller] Person with ID {PersonId} not found", id);
                return NotFound($"Person with ID {id} not found.");
            }

            _logger.LogInformation("[Controller] Successfully deleted person with ID {PersonId}", id);

            return Ok(new
            {
                message = "Person deleted successfully.",
                deletedPerson = new
                {
                    person.Id,
                    person.FirstName,
                    person.LastName,
                    person.DateOfBirth,
                    Office = person.Office.Location
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Controller] Error occurred while deleting person with ID {PersonId}", id);
            return StatusCode(500, "An error occurred while processing the 'DeletePerson' request.");
        }
    }
}

