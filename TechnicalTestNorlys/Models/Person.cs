namespace TechnicalTestNorlys.Models;

public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public int OfficeId { get; set; }
    public Office Office { get; set; } = null!;
}