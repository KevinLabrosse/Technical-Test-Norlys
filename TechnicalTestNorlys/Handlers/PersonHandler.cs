using TechnicalTestNorlys.Entities;
using TechnicalTestNorlys.Models;
using TechnicalTestNorlys.Repositories;

namespace TechnicalTestNorlys.Handlers;

public class PersonHandler
{
    private readonly ILogger<PersonHandler> _logger;
    private readonly IPersonRepository _repository;

    public PersonHandler(ILogger<PersonHandler> logger, IPersonRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }
    
    public async Task<Person> AddPersonAsync(AddPersonRequest request)
    {
        _logger.LogInformation("[Handler] Handling AddPerson for {FirstName} {LastName}", request.FirstName, request.LastName);
        return await _repository.AddPersonAsync(request);
    }

    public async Task<List<Person>> GetAllPersonsAsync()
    {
        _logger.LogInformation("[Handler] Getting all persons");
        return await _repository.GetAllPersonsAsync();
    }

    public async Task<Person?> GetPersonByIdAsync(int id)
    {
        _logger.LogInformation("[Handler] Getting person with ID {PersonId}", id);
        return await _repository.GetPersonByIdAsync(id);
    }

    public async Task<Person?> UpdatePersonAsync(int id, UpdatePersonRequest request)
    {
        try
        {
            _logger.LogInformation("[Handler] Updating person with ID {PersonId}", id);
            return await _repository.UpdatePersonAsync(id, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Handler] Error occurred while updating person with ID {PersonId}", id);
            throw;
        }
    }

    public async Task<Person?> DeletePersonAsync(int id)
    {
        try
        {
            _logger.LogInformation("[Handler] Deleting person with ID {PersonId}", id);
            return await _repository.DeletePersonAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Handler] Error occurred while deleting person with ID {PersonId}", id);
            throw;
        }
    }
}

