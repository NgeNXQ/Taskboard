using FluentValidation;
using Microsoft.Extensions.Options;
using EC.TaskBoard.Web.Shared.Configs;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;

namespace EC.TaskBoard.Web.Features.Board.Presentation.API.REST.Validators;

internal sealed class ListCreateParamsValidator : AbstractValidator<ListCreateParams>
{
    internal ListCreateParamsValidator(IOptions<ListConfig> listConfig)
    {
        var config = listConfig.Value;

        RuleFor(dto => dto.Name)
            .NotEmpty()
                .WithMessage("Name is required")
            .MaximumLength(config.MaxNameLength)
                .WithMessage($"Name must not exceed {config.MaxNameLength} characters");
    }
}
