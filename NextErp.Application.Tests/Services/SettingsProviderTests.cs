using NextErp.Application.Settings;
using NextErp.Application.Tests.Infrastructure;
using NextErp.Infrastructure.Services;

namespace NextErp.Application.Tests.Services;

public class SettingsProviderTests : HandlerTestBase
{
    private SettingsProvider BuildSut() => new(Db);

    [Fact]
    public void Schema_discovers_registered_modules()
    {
        var sut = BuildSut();
        var schema = sut.GetSchema();

        // Schema emits camelCase to line up with ASP.NET's DictionaryKeyPolicy=CamelCase output.
        schema.Modules.Should().Contain(m => m.Name == "sales");
        schema.Modules.Should().Contain(m => m.Name == "inventory");
        schema.Modules.Should().Contain(m => m.Name == "uI");
        schema.Modules.Should().Contain(m => m.Name == "locale");
    }

    [Fact]
    public void Schema_emits_setting_definitions_with_type_and_defaults()
    {
        var sut = BuildSut();
        var sales = sut.GetSchema().Modules.Single(m => m.Name == "sales");

        var vatEnabled = sales.Settings.Single(s => s.Key == "storeVatEnabled");
        vatEnabled.Type.Should().Be("bool");
        vatEnabled.Default.Should().Be(false);

        var vatRate = sales.Settings.Single(s => s.Key == "storeVatPercent");
        vatRate.Type.Should().Be("decimal");
        vatRate.Min.Should().Be(0);
        vatRate.Max.Should().Be(100);
    }

    [Fact]
    public void Schema_emits_enum_options_for_enum_typed_settings()
    {
        var sut = BuildSut();
        var inventory = sut.GetSchema().Modules.Single(m => m.Name == "inventory");

        var consumptionOrder = inventory.Settings.Single(s => s.Key == "consumptionOrder");
        consumptionOrder.Type.Should().Be("enum");
        consumptionOrder.Options.Should().Contain(new[] { "Single", "Fifo", "Lifo" });
    }

    [Fact]
    public async Task GetAsync_returns_defaults_when_no_row_persisted()
    {
        var sut = BuildSut();

        var sales = await sut.GetAsync<SalesSettings>();

        sales.StoreVatEnabled.Should().BeFalse();
        sales.StoreVatPercent.Should().Be(0m);
        sales.EnablePricingPreview.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_persists_values_and_GetAsync_returns_them()
    {
        var sut = BuildSut();

        await sut.UpdateAsync(new SalesSettings
        {
            StoreVatEnabled = true,
            StoreVatPercent = 25m,
            EnablePricingPreview = false,
        });

        var roundTrip = await sut.GetAsync<SalesSettings>();
        roundTrip.StoreVatEnabled.Should().BeTrue();
        roundTrip.StoreVatPercent.Should().Be(25m);
        roundTrip.EnablePricingPreview.Should().BeFalse();
    }

    [Fact]
    public async Task PatchAsync_applies_partial_update_and_keeps_other_defaults()
    {
        var sut = BuildSut();

        await sut.PatchAsync("Sales", new Dictionary<string, object?>
        {
            ["StoreVatEnabled"] = true,
        });

        var result = await sut.GetAsync<SalesSettings>();
        result.StoreVatEnabled.Should().BeTrue();
        // Other fields still at default.
        result.StoreVatPercent.Should().Be(0m);
        result.EnablePricingPreview.Should().BeTrue();
    }

    [Fact]
    public async Task PatchAsync_enum_accepts_string_value()
    {
        var sut = BuildSut();

        await sut.PatchAsync("Inventory", new Dictionary<string, object?>
        {
            ["ConsumptionOrder"] = "Fifo",
        });

        var result = await sut.GetAsync<InventorySettings>();
        result.ConsumptionOrder.Should().Be(InventoryConsumptionOrder.Fifo);
    }

    [Fact]
    public async Task PatchAsync_unknown_key_throws()
    {
        var sut = BuildSut();

        var act = async () => await sut.PatchAsync("Sales", new Dictionary<string, object?>
        {
            ["NonExistent"] = 1,
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*NonExistent*");
    }

    [Fact]
    public async Task PatchAsync_out_of_range_value_throws()
    {
        var sut = BuildSut();

        var act = async () => await sut.PatchAsync("Sales", new Dictionary<string, object?>
        {
            ["StoreVatPercent"] = 150m,  // SettingRange(0, 100)
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*between*0*100*");
    }

    [Fact]
    public async Task PatchAsync_unknown_module_throws()
    {
        var sut = BuildSut();

        var act = async () => await sut.PatchAsync("NonExistent", new Dictionary<string, object?>
        {
            ["X"] = 1,
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*NonExistent*");
    }

    [Fact]
    public async Task GetAllValuesAsync_returns_one_dict_per_module()
    {
        var sut = BuildSut();
        await sut.PatchAsync("Sales", new Dictionary<string, object?>
        {
            ["StoreVatEnabled"] = true,
        });

        var all = await sut.GetAllValuesAsync();

        all.Should().ContainKey("sales");
        all["sales"]["storeVatEnabled"].Should().Be(true);
        all.Should().ContainKey("inventory");
        all.Should().ContainKey("uI");
        all.Should().ContainKey("locale");
    }
}
