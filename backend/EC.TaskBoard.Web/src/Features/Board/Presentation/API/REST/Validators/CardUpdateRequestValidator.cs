using System;
using FluentValidation;
using Microsoft.Extensions.Options;
using EC.TaskBoard.Web.Shared.Configs;
using EC.TaskBoard.Web.Features.Board.Presentation.API.REST.Schemas;

namespace EC.TaskBoard.Web.Features.Board.Presentation.API.REST.Validators;

internal sealed class CardUpdateRequestValidator : AbstractValidator<CardUpdateRequest>
{
    internal CardUpdateRequestValidator(IOptions<CardConfig> cardConfig)
    {
        var config = cardConfig.Value;

        RuleFor(dto => dto.Name)
            .MaximumLength(config.MaxNameLength)
                .WithMessage($"Name must not exceed {config.MaxNameLength} characters")
            .When(dto => dto.Name is not null);

        RuleFor(dto => dto.ListId)
            .Must(id => id != Guid.Empty)
                .WithMessage("List ID is required")
            .When(dto => dto.ListId.HasValue);

        RuleFor(dto => dto.Priority)
            .IsInEnum()
                .WithMessage("Invalid priority value")
            .When(dto => dto.Priority.HasValue);

        RuleFor(dto => dto.DueDate!.Value)
            .Must(date => date.Offset == TimeSpan.Zero)
                .WithMessage("Due date must be in UTC")
            .When(dto => dto.DueDate.HasValue);

        RuleFor(dto => dto.Description)
            .MaximumLength(config.MaxDescriptionLength)
                .WithMessage(
                    $"Description must not exceed {config.MaxDescriptionLength} characters"
                )
            .When(dto => dto.Description is not null);
    }
}
