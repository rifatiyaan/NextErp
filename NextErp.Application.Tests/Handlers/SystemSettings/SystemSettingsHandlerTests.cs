using AutoMapper;
using NextErp.Application.Commands.SystemSettings;
using NextErp.Application.Handlers.CommandHandlers.SystemSettings;
using NextErp.Application.Handlers.QueryHandlers.SystemSettings;
using NextErp.Application.Queries;
using SystemSettingsDto = NextErp.Application.DTOs.SystemSettings;

namespace NextErp.Application.Tests.Handlers.SystemSettings;

public class SystemSettingsHandlerTests : HandlerTestBase
{
    private static IMapper BuildMapper()
    {
        var cfg = new MapperConfiguration(c =>
            c.AddMaps(typeof(NextErp.Application.ApplicationAssemblyMarker).Assembly));
        return cfg.CreateMapper();
    }

    // ─── Get ───

    [Fact]
    public async Task Get_returns_defaults_when_no_row_exists()
    {
        var sut = new GetSystemSettingsHandler(Db, BuildMapper());

        var dto = await sut.Handle(new GetSystemSettingsQuery(), CancellationToken.None);

        dto.PresetAccentTheme.Should().Be("theme-slate");
        dto.NavigationPlacement.Should().Be("sidebar");
        dto.Radius.Should().Be("md");
        dto.CompanyName.Should().BeNull();
        dto.CompanyLogoUrl.Should().BeNull();

        // Lazy materialisation: defaults returned but NOT persisted.
        var rowCount = await Db.SystemSettings.AsNoTracking().CountAsync();
        rowCount.Should().Be(0);
    }

    [Fact]
    public async Task Get_returns_persisted_row_when_one_exists()
    {
        Db.SystemSettings.Add(new NextErp.Domain.Entities.SystemSettings
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.Empty,
            PresetAccentTheme = "theme-blue",
            NavigationPlacement = "topbar",
            Radius = "sm",
            CompanyName = "Acme Corp",
            CreatedAt = DateTime.UtcNow,
        });
        await Db.SaveChangesAsync();

        var sut = new GetSystemSettingsHandler(Db, BuildMapper());
        var dto = await sut.Handle(new GetSystemSettingsQuery(), CancellationToken.None);

        dto.PresetAccentTheme.Should().Be("theme-blue");
        dto.NavigationPlacement.Should().Be("topbar");
        dto.Radius.Should().Be("sm");
        dto.CompanyName.Should().Be("Acme Corp");
    }

    // ─── Update ───

    [Fact]
    public async Task Update_creates_row_on_first_call()
    {
        var sut = new UpdateSystemSettingsHandler(Db, BuildMapper());

        var dto = new SystemSettingsDto.Request.Update
        {
            PresetAccentTheme = "theme-rose",
            NavigationPlacement = "topbar",
            CompanyName = "First Run",
        };

        var result = await sut.Handle(new UpdateSystemSettingsCommand(dto), CancellationToken.None);

        result.PresetAccentTheme.Should().Be("theme-rose");
        result.NavigationPlacement.Should().Be("topbar");
        result.CompanyName.Should().Be("First Run");

        var persisted = await Db.SystemSettings.AsNoTracking().SingleAsync();
        persisted.PresetAccentTheme.Should().Be("theme-rose");
    }

    [Fact]
    public async Task Update_preserves_unspecified_fields()
    {
        Db.SystemSettings.Add(new NextErp.Domain.Entities.SystemSettings
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.Empty,
            PresetAccentTheme = "theme-blue",
            NavigationPlacement = "sidebar",
            Radius = "md",
            CompanyName = "Existing",
            CreatedAt = DateTime.UtcNow,
        });
        await Db.SaveChangesAsync();

        var sut = new UpdateSystemSettingsHandler(Db, BuildMapper());
        var dto = new SystemSettingsDto.Request.Update { Radius = "none" };
        var result = await sut.Handle(new UpdateSystemSettingsCommand(dto), CancellationToken.None);

        result.Radius.Should().Be("none");
        result.PresetAccentTheme.Should().Be("theme-blue");
        result.CompanyName.Should().Be("Existing");
    }

    [Fact]
    public async Task Update_switching_to_preset_clears_custom_color_fields()
    {
        Db.SystemSettings.Add(new NextErp.Domain.Entities.SystemSettings
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.Empty,
            CustomPrimary = "221 83% 53%",
            CustomSecondary = "215 14% 90%",
            CustomSidebarBackground = "240 10% 15%",
            NavigationPlacement = "sidebar",
            Radius = "md",
            CreatedAt = DateTime.UtcNow,
        });
        await Db.SaveChangesAsync();

        var sut = new UpdateSystemSettingsHandler(Db, BuildMapper());
        var dto = new SystemSettingsDto.Request.Update { PresetAccentTheme = "theme-violet" };
        var result = await sut.Handle(new UpdateSystemSettingsCommand(dto), CancellationToken.None);

        result.PresetAccentTheme.Should().Be("theme-violet");
        result.CustomPrimary.Should().BeNull();
        result.CustomSecondary.Should().BeNull();
        result.CustomSidebarBackground.Should().BeNull();
    }

    [Fact]
    public async Task Update_switching_to_custom_clears_preset()
    {
        Db.SystemSettings.Add(new NextErp.Domain.Entities.SystemSettings
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.Empty,
            PresetAccentTheme = "theme-blue",
            NavigationPlacement = "sidebar",
            Radius = "md",
            CreatedAt = DateTime.UtcNow,
        });
        await Db.SaveChangesAsync();

        var sut = new UpdateSystemSettingsHandler(Db, BuildMapper());
        var dto = new SystemSettingsDto.Request.Update { CustomPrimary = "12 88% 50%" };
        var result = await sut.Handle(new UpdateSystemSettingsCommand(dto), CancellationToken.None);

        result.PresetAccentTheme.Should().BeNull();
        result.CustomPrimary.Should().Be("12 88% 50%");
    }

    [Fact]
    public async Task Update_sets_UpdatedAt()
    {
        var sut = new UpdateSystemSettingsHandler(Db, BuildMapper());
        var dto = new SystemSettingsDto.Request.Update { PresetAccentTheme = "theme-green" };

        await sut.Handle(new UpdateSystemSettingsCommand(dto), CancellationToken.None);

        var persisted = await Db.SystemSettings.AsNoTracking().SingleAsync();
        persisted.UpdatedAt.Should().NotBeNull();
        persisted.UpdatedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ─── Reset ───

    [Fact]
    public async Task Reset_creates_defaults_when_no_row_exists()
    {
        var sut = new ResetSystemSettingsHandler(Db, BuildMapper());

        var result = await sut.Handle(new ResetSystemSettingsCommand(), CancellationToken.None);

        result.PresetAccentTheme.Should().Be("theme-slate");
        result.NavigationPlacement.Should().Be("sidebar");
        result.Radius.Should().Be("md");

        (await Db.SystemSettings.AsNoTracking().CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Reset_clears_all_customisation_on_existing_row()
    {
        Db.SystemSettings.Add(new NextErp.Domain.Entities.SystemSettings
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.Empty,
            PresetAccentTheme = null,
            CustomPrimary = "221 83% 53%",
            CustomSidebarBackground = "240 10% 15%",
            NavigationPlacement = "topbar",
            Radius = "none",
            CompanyName = "Aker Solutions",
            CompanyLogoUrl = "https://example.com/logo.png",
            CreatedAt = DateTime.UtcNow,
        });
        await Db.SaveChangesAsync();

        var sut = new ResetSystemSettingsHandler(Db, BuildMapper());
        var result = await sut.Handle(new ResetSystemSettingsCommand(), CancellationToken.None);

        result.PresetAccentTheme.Should().Be("theme-slate");
        result.CustomPrimary.Should().BeNull();
        result.CustomSidebarBackground.Should().BeNull();
        result.NavigationPlacement.Should().Be("sidebar");
        result.Radius.Should().Be("md");
        result.CompanyName.Should().BeNull();
        result.CompanyLogoUrl.Should().BeNull();

        // Same row, not a new one.
        (await Db.SystemSettings.AsNoTracking().CountAsync()).Should().Be(1);
    }
}

