using Microsoft.EntityFrameworkCore;
using TechnicalTestNorlys.Entities;
using TechnicalTestNorlys.Models;

namespace TechnicalTestNorlys.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly AppDbContext _context;

    public PersonRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Person> AddPersonAsync(AddPersonRequest request)
    {
        // Verify the office exists
        var office = await _context.Offices
            .FirstOrDefaultAsync(o => o.Location == request.Office);

        if (office == null)
        {
            throw new InvalidOperationException($"[Repository] Office with location '{request.Office}' does not exist.");
        }

        var person = new Person
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            Office = office
        };

        _context.Persons.Add(person);
        await _context.SaveChangesAsync();

        return person;
    }

    public async Task<List<Person>> GetAllPersonsAsync()
    {
        return await _context.Persons
            .Include(p => p.Office)
            .ToListAsync();
    }

    public async Task<Person?> GetPersonByIdAsync(int id)
    {
        return await _context.Persons
            .Include(p => p.Office)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Person?> UpdatePersonAsync(int id, UpdatePersonRequest request)
    {
        var person = await _context.Persons
            .Include(p => p.Office)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (person == null)
        {
            return null;
        }

        // Update only provided fields (PATCH behavior)
        if (request.FirstName != null)
        {
            person.FirstName = request.FirstName;
        }

        if (request.LastName != null)
        {
            person.LastName = request.LastName;
        }

        if (request.DateOfBirth != null)
        {
            person.DateOfBirth = request.DateOfBirth.Value;
        }

        if (request.Office != null)
        {
            var office = await _context.Offices
                .FirstOrDefaultAsync(o => o.Location == request.Office);

            if (office == null)
            {
                throw new InvalidOperationException($"[Repository] Office with location '{request.Office}' does not exist.");
            }

            person.Office = office;
            person.OfficeId = office.Id;
        }

        await _context.SaveChangesAsync();

        return person;
    }

    public async Task<Person?> DeletePersonAsync(int id)
    {
        var person = await _context.Persons
            .Include(p => p.Office)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (person == null)
        {
            return null;
        }

        _context.Persons.Remove(person);
        await _context.SaveChangesAsync();

        return person;
    }
}

