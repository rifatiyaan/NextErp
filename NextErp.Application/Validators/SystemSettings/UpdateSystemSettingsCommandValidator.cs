using System.Text.RegularExpressions;
using FluentValidation;
using NextErp.Application.Commands.SystemSettings;

namespace NextErp.Application.Validators.SystemSettings;

public sealed class UpdateSystemSettingsCommandValidator : AbstractValidator<UpdateSystemSettingsCommand>
{
    // Format: integer hue (0-360), then space, then int saturation% then space then int lightness%.
    // Examples: "221 83% 53%", "0 0% 100%". Allows fractional values too: "240 5.9% 10%".
    private static readonly Regex HslRegex = new(
        @"^\s*\d{1,3}(?:\.\d+)?\s+\d{1,3}(?:\.\d+)?%\s+\d{1,3}(?:\.\d+)?%\s*$",
        RegexOptions.Compiled);

    private static readonly string[] AllowedPlacements = { "sidebar", "topbar" };
    private static readonly string[] AllowedRadii = { "none", "sm", "md" };

    public UpdateSystemSettingsCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull();

        When(x => x.Dto != null, () =>
        {
            RuleFor(x => x.Dto.NavigationPlacement)
                .Must(p => p == null || AllowedPlacements.Contains(p))
                .WithMessage($"NavigationPlacement must be one of: {string.Join(", ", AllowedPlacements)}.");

            RuleFor(x => x.Dto.Radius)
                .Must(r => r == null || AllowedRadii.Contains(r))
                .WithMessage($"Radius must be one of: {string.Join(", ", AllowedRadii)}.");

            RuleFor(x => x.Dto.CustomPrimary)
                .Must(BeNullOrValidHsl)
                .WithMessage("CustomPrimary must be in HSL format \"H S% L%\" (e.g. \"221 83% 53%\").");

            RuleFor(x => x.Dto.CustomSecondary).Must(BeNullOrValidHsl).WithMessage("CustomSecondary must be HSL.");
            RuleFor(x => x.Dto.CustomSidebarBackground).Must(BeNullOrValidHsl).WithMessage("CustomSidebarBackground must be HSL.");
            RuleFor(x => x.Dto.CustomSidebarForeground).Must(BeNullOrValidHsl).WithMessage("CustomSidebarForeground must be HSL.");

            RuleFor(x => x.Dto.PresetAccentTheme)
                .MaximumLength(64);

            RuleFor(x => x.Dto.CompanyName).MaximumLength(200);
            RuleFor(x => x.Dto.CompanyLogoUrl).MaximumLength(500);

            // Mutual exclusion: cannot mix preset + custom in the same request.
            RuleFor(x => x.Dto)
                .Must(dto =>
                {
                    var hasPreset = !string.IsNullOrWhiteSpace(dto.PresetAccentTheme);
                    var hasAnyCustom =
                        !string.IsNullOrWhiteSpace(dto.CustomPrimary) ||
                        !string.IsNullOrWhiteSpace(dto.CustomSecondary) ||
                        !string.IsNullOrWhiteSpace(dto.CustomSidebarBackground) ||
                        !string.IsNullOrWhiteSpace(dto.CustomSidebarForeground);
                    return !(hasPreset && hasAnyCustom);
                })
                .WithMessage("Cannot set PresetAccentTheme together with custom HSL color values. Choose one approach.");
        });
    }

    private static bool BeNullOrValidHsl(string? value) =>
        string.IsNullOrWhiteSpace(value) || HslRegex.IsMatch(value);
}

