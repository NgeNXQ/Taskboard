using System;
using FluentValidation;
using Microsoft.Extensions.Options;
using EC.TaskBoard.Web.Shared.Configs;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;

namespace EC.TaskBoard.Web.Features.Board.Presentation.API.REST.Validators;

internal sealed class CardCreateParamsValidator : AbstractValidator<CardCreateParams>
{
    internal CardCreateParamsValidator(IOptions<CardConfig> cardConfig)
    {
        var config = cardConfig.Value;

        RuleFor(dto => dto.Name)
            .NotEmpty()
                .WithMessage("Name is required")
            .MaximumLength(config.MaxNameLength)
                .WithMessage($"Name must not exceed {config.MaxNameLength} characters");

        RuleFor(dto => dto.ListId)
            .NotEmpty()
                .WithMessage("List ID is required");

        RuleFor(dto => dto.DueDate)
            .NotEmpty()
                .WithMessage("Due date is required")
            .Must(date => date.Offset == TimeSpan.Zero)
                .WithMessage("Due date must be in UTC");

        RuleFor(dto => dto.Priority)
            .IsInEnum()
                .WithMessage("Invalid priority value");

        RuleFor(dto => dto.Description)
            .NotEmpty()
                .WithMessage("Description is required")
            .MaximumLength(config.MaxDescriptionLength)
                .WithMessage(
                    $"Description must not exceed {config.MaxDescriptionLength} characters"
                );
    }
}
