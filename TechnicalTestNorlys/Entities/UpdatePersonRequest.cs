namespace TechnicalTestNorlys.Entities;

public class UpdatePersonRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Office { get; set; }
}
