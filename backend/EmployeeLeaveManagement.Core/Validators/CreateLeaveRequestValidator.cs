using EmployeeLeaveManagement.Core.DTOs;
using FluentValidation;

namespace EmployeeLeaveManagement.Core.Validators;

public class CreateLeaveRequestValidator : AbstractValidator<CreateLeaveRequestDto>
{
    public CreateLeaveRequestValidator()
    {
        RuleFor(x => x.LeaveType)
            .IsInEnum()
            .WithMessage("Invalid leave type specified.");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required.")
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after the start date.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required.")
            .MaximumLength(500)
            .WithMessage("Reason must not exceed 500 characters.");
    }
}
