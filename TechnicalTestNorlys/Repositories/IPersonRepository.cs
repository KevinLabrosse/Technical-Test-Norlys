using TechnicalTestNorlys.Entities;
using TechnicalTestNorlys.Models;

namespace TechnicalTestNorlys.Repositories;

public interface IPersonRepository
{
    Task<Person> AddPersonAsync(AddPersonRequest request);
    Task<List<Person>> GetAllPersonsAsync();
    Task<Person?> GetPersonByIdAsync(int id);
    Task<Person?> UpdatePersonAsync(int id, UpdatePersonRequest request);
    Task<Person?> DeletePersonAsync(int id);
}

