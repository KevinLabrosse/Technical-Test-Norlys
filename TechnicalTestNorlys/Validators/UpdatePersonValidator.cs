using FluentValidation;
using TechnicalTestNorlys.Entities;

namespace TechnicalTestNorlys.Validators;

public class UpdatePersonValidator : AbstractValidator<UpdatePersonRequest>
{
    public UpdatePersonValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(50).WithMessage("[Validator] FirstName cannot exceed 50 characters")
            .When(x => x.FirstName != null);

        RuleFor(x => x.LastName)
            .Must(name => !name!.Any(char.IsWhiteSpace)).WithMessage("[Validator] LastName cannot contain whitespace (put middle names with the first name)")
            .MaximumLength(50).WithMessage("[Validator] LastName cannot exceed 50 characters")
            .When(x => x.LastName != null);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateOnly.FromDateTime(DateTime.Now.AddYears(-13))).WithMessage("[Validator] Only people older than 13 years can work here")
            .GreaterThan(DateOnly.FromDateTime(DateTime.Now.AddYears(-120))).WithMessage("[Validator] You are not that old and we don't hire vampires")
            .When(x => x.DateOfBirth != null);

        RuleFor(x => x.Office)
            .MaximumLength(100).WithMessage("[Validator] Office cannot exceed 100 characters")
            .When(x => x.Office != null);
    }
}

