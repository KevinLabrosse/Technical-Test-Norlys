namespace TechnicalTestNorlys.Entities;

public class AddPersonRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Office { get; set; } = string.Empty;
}