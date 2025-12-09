using FluentValidation;
using TechnicalTestNorlys.Entities;

namespace TechnicalTestNorlys.Validators;

public class AddPersonValidator : AbstractValidator<AddPersonRequest>
{
    public AddPersonValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("[Validator] FirstName is required")
            .MaximumLength(50).WithMessage("[Validator] FirstName cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("[Validator] LastName is required")
            .Must(name => !name.Any(char.IsWhiteSpace)).WithMessage("[Validator] LastName cannot contain whitespace (put middle names with the first name)")
            .MaximumLength(50).WithMessage("[Validator] LastName cannot exceed 50 characters");
        
        RuleFor(x => x.DateOfBirth)
            .LessThan(DateOnly.FromDateTime(DateTime.Now.AddYears(-13))).WithMessage("[Validator] Only people older than 13 years can work here")
            .GreaterThan(DateOnly.FromDateTime(DateTime.Now.AddYears(-120))).WithMessage("[Validator] You are not that old and we don't hire vampires");

        RuleFor(x => x.Office)
            .NotEmpty().WithMessage("[Validator] Location is required")
            .MaximumLength(100).WithMessage("[Validator] Location cannot exceed 100 characters");
    }
}