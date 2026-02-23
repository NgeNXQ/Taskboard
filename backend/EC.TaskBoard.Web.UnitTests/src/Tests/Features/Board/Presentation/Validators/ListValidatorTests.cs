using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;
using EC.TaskBoard.Web.Features.Board.Presentation.API.REST.Schemas;
using EC.TaskBoard.Web.Features.Board.Presentation.API.REST.Validators;
using EC.TaskBoard.Web.Shared.Configs;
using Xunit;

namespace EC.TaskBoard.Web.Tests.Features.Board.Presentation.Validators;

public sealed class ListCreateParamsValidatorTests
{
    private const int MaxNameLength = 50;

    private readonly ListCreateParamsValidator _sut;

    public ListCreateParamsValidatorTests()
    {
        var config = new ListConfig { MaxNameLength = MaxNameLength };

        _sut = new ListCreateParamsValidator(Options.Create(config));
    }

    [Fact]
    public void Validate_ValidName_PassesValidation()
    {
        var result = _sut.TestValidate(new ListCreateParams("Backlog"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_HasValidationError(string? name)
    {
        var result = _sut.TestValidate(new ListCreateParams(name!));

        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Validate_NameExceedsMaxLength_HasValidationError()
    {
        var longName = new string('L', MaxNameLength + 1);

        var result = _sut.TestValidate(new ListCreateParams(longName));

        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage($"Name must not exceed {MaxNameLength} characters");
    }

    [Fact]
    public void Validate_NameAtMaxLength_PassesValidation()
    {
        var maxName = new string('L', MaxNameLength);

        var result = _sut.TestValidate(new ListCreateParams(maxName));

        Assert.True(result.IsValid);
    }
}

public sealed class ListUpdateRequestValidatorTests
{
    private const int MaxNameLength = 50;

    private readonly ListUpdateRequestValidator _sut;

    public ListUpdateRequestValidatorTests()
    {
        var config = new ListConfig { MaxNameLength = MaxNameLength };

        _sut = new ListUpdateRequestValidator(Options.Create(config));
    }

    [Fact]
    public void Validate_ValidName_PassesValidation()
    {
        var result = _sut.TestValidate(new ListUpdateRequest("Done"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_HasValidationError(string? name)
    {
        var result = _sut.TestValidate(new ListUpdateRequest(name!));

        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Validate_NameExceedsMaxLength_HasValidationError()
    {
        var longName = new string('L', MaxNameLength + 1);

        var result = _sut.TestValidate(new ListUpdateRequest(longName));

        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage($"Name must not exceed {MaxNameLength} characters");
    }

    [Fact]
    public void Validate_NameAtMaxLength_PassesValidation()
    {
        var maxName = new string('L', MaxNameLength);

        var result = _sut.TestValidate(new ListUpdateRequest(maxName));

        Assert.True(result.IsValid);
    }
}
