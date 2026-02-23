using System;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;
using EC.TaskBoard.Web.Features.Board.Domain.Enums;
using EC.TaskBoard.Web.Features.Board.Presentation.API.REST.Schemas;
using EC.TaskBoard.Web.Features.Board.Presentation.API.REST.Validators;
using EC.TaskBoard.Web.Shared.Configs;
using Xunit;

namespace EC.TaskBoard.Web.Tests.Features.Board.Presentation.Validators;

public sealed class CardUpdateRequestValidatorTests
{
    private const int MaxNameLength = 50;
    private const int MaxDescriptionLength = 200;

    private readonly CardUpdateRequestValidator _sut;

    public CardUpdateRequestValidatorTests()
    {
        var config = new CardConfig
        {
            MaxNameLength = MaxNameLength,
            MaxDescriptionLength = MaxDescriptionLength,
        };

        _sut = new CardUpdateRequestValidator(Options.Create(config));
    }

    // -------------------------------------------------------------------------
    // All-null is valid (partial update, nothing changed)
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_AllFieldsNull_PassesValidation()
    {
        var request = new CardUpdateRequest(null, null, null, null, null);

        var result = _sut.TestValidate(request);

        Assert.True(result.IsValid);
    }

    // -------------------------------------------------------------------------
    // Name (optional, validated when present)
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_NameWithinMaxLength_NoErrorsForName()
    {
        var request = new CardUpdateRequest(null, "Good Name", null, null, null);

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameExceedsMaxLength_HasValidationError()
    {
        var longName = new string('a', MaxNameLength + 1);
        var request = new CardUpdateRequest(null, longName, null, null, null);

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage($"Name must not exceed {MaxNameLength} characters");
    }

    [Fact]
    public void Validate_NullName_NoErrorsForName()
    {
        // When name is null the rule is skipped entirely
        var request = new CardUpdateRequest(null, null, null, null, null);

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // -------------------------------------------------------------------------
    // ListId (optional, must be non-empty when present)
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_ValidListId_NoErrorsForListId()
    {
        var request = new CardUpdateRequest(Guid.NewGuid(), null, null, null, null);

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.ListId);
    }

    [Fact]
    public void Validate_EmptyListId_HasValidationError()
    {
        var request = new CardUpdateRequest(Guid.Empty, null, null, null, null);

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ListId)
              .WithErrorMessage("List ID is required");
    }

    [Fact]
    public void Validate_NullListId_NoErrorsForListId()
    {
        var request = new CardUpdateRequest(null, null, null, null, null);

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.ListId);
    }

    // -------------------------------------------------------------------------
    // Priority (optional, validated when present)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(0)] // CardPriority.Lowest
    [InlineData(1)] // CardPriority.Medium
    [InlineData(2)] // CardPriority.Highest
    public void Validate_ValidPriority_NoErrorsForPriority(int priorityValue)
    {
        var priority = (CardPriority)priorityValue;
        var request = new CardUpdateRequest(null, null, null, priority, null);

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Priority);
    }

    [Fact]
    public void Validate_NullPriority_NoErrorsForPriority()
    {
        var request = new CardUpdateRequest(null, null, null, null, null);

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Priority);
    }

    // -------------------------------------------------------------------------
    // DueDate (optional, must be UTC when present)
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_DueDateInUtc_NoErrorsForDueDate()
    {
        var request = new CardUpdateRequest(null, null, null, null, DateTimeOffset.UtcNow.AddDays(1));

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public void Validate_DueDateNotInUtc_HasValidationError()
    {
        var nonUtc = new DateTimeOffset(2030, 1, 1, 12, 0, 0, TimeSpan.FromHours(3));
        var request = new CardUpdateRequest(null, null, null, null, nonUtc);

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.DueDate!.Value)
              .WithErrorMessage("Due date must be in UTC");
    }

    [Fact]
    public void Validate_NullDueDate_NoErrorsForDueDate()
    {
        var request = new CardUpdateRequest(null, null, null, null, null);

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }

    // -------------------------------------------------------------------------
    // Description (optional, validated when present)
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_DescriptionWithinMaxLength_NoErrorsForDescription()
    {
        var request = new CardUpdateRequest(null, null, "Good description", null, null);

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_DescriptionExceedsMaxLength_HasValidationError()
    {
        var longDesc = new string('d', MaxDescriptionLength + 1);
        var request = new CardUpdateRequest(null, null, longDesc, null, null);

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Description)
              .WithErrorMessage($"Description must not exceed {MaxDescriptionLength} characters");
    }

    [Fact]
    public void Validate_NullDescription_NoErrorsForDescription()
    {
        var request = new CardUpdateRequest(null, null, null, null, null);

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
