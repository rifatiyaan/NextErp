using FluentAssertions;
using NextErp.Application.Commands;
using NextErp.Application.Validators.Stock;
using NextErp.Domain.Entities;

namespace NextErp.Application.Tests.Validators;

public class CreateStockAdjustmentCommandValidatorTests
{
    private readonly CreateStockAdjustmentCommandValidator _sut = new();

    private static CreateStockAdjustmentCommand Valid(
        int productVariantId = 1,
        StockAdjustmentMode mode = StockAdjustmentMode.Increase,
        decimal quantity = 5m,
        string reasonCode = StockAdjustmentReason.PhysicalCountCorrection,
        string? notes = null)
        => new(productVariantId, mode, quantity, reasonCode, notes);

    [Fact]
    public void Valid_command_passes()
    {
        var result = _sut.Validate(Valid());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ProductVariantId_must_be_positive(int variantId)
    {
        var result = _sut.Validate(Valid(productVariantId: variantId));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateStockAdjustmentCommand.ProductVariantId));
    }

    [Fact]
    public void Mode_outside_enum_fails()
    {
        var cmd = Valid(mode: (StockAdjustmentMode)99);
        var result = _sut.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateStockAdjustmentCommand.Mode));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Quantity_must_be_positive_for_Increase(decimal qty)
    {
        var result = _sut.Validate(Valid(mode: StockAdjustmentMode.Increase, quantity: qty));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateStockAdjustmentCommand.Quantity));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Quantity_must_be_positive_for_Decrease(decimal qty)
    {
        var result = _sut.Validate(Valid(mode: StockAdjustmentMode.Decrease, quantity: qty));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateStockAdjustmentCommand.Quantity));
    }

    [Fact]
    public void Quantity_zero_allowed_for_SetAbsolute()
    {
        // SetAbsolute with 0 means "set stock to zero" — legitimate
        var result = _sut.Validate(Valid(mode: StockAdjustmentMode.SetAbsolute, quantity: 0m));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Quantity_negative_rejected_for_SetAbsolute()
    {
        var result = _sut.Validate(Valid(mode: StockAdjustmentMode.SetAbsolute, quantity: -1m));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateStockAdjustmentCommand.Quantity));
    }

    [Fact]
    public void ReasonCode_empty_fails()
    {
        var result = _sut.Validate(Valid(reasonCode: ""));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateStockAdjustmentCommand.ReasonCode));
    }

    [Fact]
    public void ReasonCode_unknown_fails()
    {
        var result = _sut.Validate(Valid(reasonCode: "NotARealReason"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateStockAdjustmentCommand.ReasonCode));
    }

    [Theory]
    [MemberData(nameof(AllReasonCodes))]
    public void Every_known_reason_code_passes(string reasonCode)
    {
        var result = _sut.Validate(Valid(reasonCode: reasonCode));
        result.IsValid.Should().BeTrue();
    }

    public static IEnumerable<object[]> AllReasonCodes() =>
        StockAdjustmentReason.All.Select(r => new object[] { r });

    [Fact]
    public void Notes_at_max_length_passes()
    {
        var notes = new string('x', 500);
        var result = _sut.Validate(Valid(notes: notes));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Notes_exceeding_max_length_fails()
    {
        var notes = new string('x', 501);
        var result = _sut.Validate(Valid(notes: notes));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateStockAdjustmentCommand.Notes));
    }

    [Fact]
    public void Notes_null_passes()
    {
        var result = _sut.Validate(Valid(notes: null));
        result.IsValid.Should().BeTrue();
    }
}
