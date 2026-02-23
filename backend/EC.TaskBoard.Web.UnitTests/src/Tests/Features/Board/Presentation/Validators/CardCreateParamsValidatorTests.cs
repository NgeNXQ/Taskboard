using System;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;
using EC.TaskBoard.Web.Features.Board.Domain.Enums;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;
using EC.TaskBoard.Web.Features.Board.Presentation.API.REST.Validators;
using EC.TaskBoard.Web.Shared.Configs;
using Xunit;

namespace EC.TaskBoard.Web.Tests.Features.Board.Presentation.Validators;

public sealed class CardCreateParamsValidatorTests
{
    private const int MaxNameLength = 50;
    private const int MaxDescriptionLength = 200;

    private readonly CardCreateParamsValidator _sut;

    public CardCreateParamsValidatorTests()
    {
        var config = new CardConfig
        {
            MaxNameLength = MaxNameLength,
            MaxDescriptionLength = MaxDescriptionLength,
        };

        _sut = new CardCreateParamsValidator(Options.Create(config));
    }

    private const string DefaultName = "Valid Name";
    private const string DefaultDescription = "Valid description";

    private static CardCreateParams ValidParams(
        string? name = DefaultName,
        Guid? listId = null,
        string? description = DefaultDescription,
        CardPriority priority = CardPriority.Medium,
        DateTimeOffset? dueDate = null
    ) => new(
        ListId: listId ?? Guid.NewGuid(),
        Name: name!,
        Description: description!,
        Priority: priority,
        DueDate: dueDate ?? DateTimeOffset.UtcNow.AddDays(7)
    );

    // -------------------------------------------------------------------------
    // Name
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_ValidName_NoErrorsForName()
    {
        var result = _sut.TestValidate(ValidParams(name: "My Card"));

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_HasValidationError(string? name)
    {
        var result = _sut.TestValidate(ValidParams(name: name!));

        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Validate_NameExceedsMaxLength_HasValidationError()
    {
        var longName = new string('a', MaxNameLength + 1);

        var result = _sut.TestValidate(ValidParams(name: longName));

        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage($"Name must not exceed {MaxNameLength} characters");
    }

    [Fact]
    public void Validate_NameAtMaxLength_NoErrorsForName()
    {
        var maxName = new string('a', MaxNameLength);

        var result = _sut.TestValidate(ValidParams(name: maxName));

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // -------------------------------------------------------------------------
    // ListId
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_EmptyListId_HasValidationError()
    {
        var result = _sut.TestValidate(ValidParams(listId: Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.ListId)
              .WithErrorMessage("List ID is required");
    }

    [Fact]
    public void Validate_ValidListId_NoErrorsForListId()
    {
        var result = _sut.TestValidate(ValidParams(listId: Guid.NewGuid()));

        result.ShouldNotHaveValidationErrorFor(x => x.ListId);
    }

    // -------------------------------------------------------------------------
    // DueDate
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_DueDateInUtc_NoErrorsForDueDate()
    {
        var result = _sut.TestValidate(ValidParams(dueDate: DateTimeOffset.UtcNow.AddDays(1)));

        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public void Validate_DueDateNotInUtc_HasValidationError()
    {
        var nonUtcDate = new DateTimeOffset(2030, 6, 1, 12, 0, 0, TimeSpan.FromHours(2));

        var result = _sut.TestValidate(ValidParams(dueDate: nonUtcDate));

        result.ShouldHaveValidationErrorFor(x => x.DueDate)
              .WithErrorMessage("Due date must be in UTC");
    }

    // -------------------------------------------------------------------------
    // Priority
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(0)] // CardPriority.Lowest
    [InlineData(1)] // CardPriority.Medium
    [InlineData(2)] // CardPriority.Highest
    public void Validate_ValidPriority_NoErrorsForPriority(int priorityValue)
    {
        var priority = (CardPriority)priorityValue;
        var result = _sut.TestValidate(ValidParams(priority: priority));

        result.ShouldNotHaveValidationErrorFor(x => x.Priority);
    }

    // -------------------------------------------------------------------------
    // Description
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_ValidDescription_NoErrorsForDescription()
    {
        var result = _sut.TestValidate(ValidParams(description: "Valid description"));

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyDescription_HasValidationError(string? description)
    {
        var result = _sut.TestValidate(ValidParams(description: description!));

        result.ShouldHaveValidationErrorFor(x => x.Description)
              .WithErrorMessage("Description is required");
    }

    [Fact]
    public void Validate_DescriptionExceedsMaxLength_HasValidationError()
    {
        var longDescription = new string('d', MaxDescriptionLength + 1);

        var result = _sut.TestValidate(ValidParams(description: longDescription));

        result.ShouldHaveValidationErrorFor(x => x.Description)
              .WithErrorMessage($"Description must not exceed {MaxDescriptionLength} characters");
    }

    [Fact]
    public void Validate_DescriptionAtMaxLength_NoErrorsForDescription()
    {
        var maxDescription = new string('d', MaxDescriptionLength);

        var result = _sut.TestValidate(ValidParams(description: maxDescription));

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    // -------------------------------------------------------------------------
    // Fully valid model
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_AllFieldsValid_PassesValidation()
    {
        var result = _sut.TestValidate(ValidParams());

        Assert.True(result.IsValid);
    }
}
