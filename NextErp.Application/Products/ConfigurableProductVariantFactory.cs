using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Products
{
    /// <summary>
    /// Composes sellable <see cref="Entities.ProductVariant"/> rows from global options/values
    /// and request payloads (option order + per-variant "optIdx:valIdx" keys).
    /// </summary>
    public static class ConfigurableProductVariantFactory
    {
        public static async Task<Dictionary<string, Entities.VariationOption>> LoadActiveGlobalOptionsAsync(
            IApplicationDbContext dbContext,
            IEnumerable<string> optionNames,
            CancellationToken cancellationToken)
        {
            var nameSet = optionNames.Distinct().ToList();
            var options = await dbContext.VariationOptions
                .Include(vo => vo.Values.OrderBy(v => v.DisplayOrder))
                .Where(vo => vo.IsActive && nameSet.Contains(vo.Name))
                .ToListAsync(cancellationToken);

            return options.ToDictionary(vo => vo.Name, vo => vo);
        }

        /// <summary>
        /// Ensures every value string in the product payload exists as an active global
        /// <see cref="Entities.VariationValue"/> (product edit/create can add new values before variants are saved).
        /// Call <see cref="LoadActiveGlobalOptionsAsync"/> again after <c>SaveChanges</c> so navigations include new rows.
        /// </summary>
        public static async Task SyncVariationValuesFromRequestAsync(
            IReadOnlyList<ProductVariation.Request.VariationOptionDto> optionsInRequestOrder,
            IReadOnlyDictionary<string, Entities.VariationOption> optionByName,
            IApplicationDbContext dbContext,
            CancellationToken cancellationToken)
        {
            foreach (var dto in optionsInRequestOrder)
            {
                if (!optionByName.TryGetValue(dto.Name, out var globalOption))
                    continue;

                for (var valIdx = 0; valIdx < dto.Values.Count; valIdx++)
                    await UpsertVariationValueAsync(
                        dto.Name,
                        globalOption,
                        valIdx,
                        dto.Values[valIdx],
                        dbContext,
                        cancellationToken);
            }
        }

        /// <summary>
        /// Maps "optionIndex:valueIndex" → <see cref="Entities.VariationValue"/>.
        /// Indices match the client: <c>optIdx</c> is the position in <paramref name="optionsInRequestOrder"/>,
        /// <c>valIdx</c> is the position in that option's <c>Values</c> list (same as the UI cartesian product).
        /// Each slot resolves by value string on the global option (not by sorted DisplayOrder index).
        /// </summary>
        public static Dictionary<string, Entities.VariationValue> BuildValueKeyMap(
            IReadOnlyList<ProductVariation.Request.VariationOptionDto> optionsInRequestOrder,
            IReadOnlyDictionary<string, Entities.VariationOption> optionByName) =>
            optionsInRequestOrder
                .Select((dto, optIdx) => (dto, optIdx, Global: RequireGlobalOption(optionByName, dto.Name)))
                .SelectMany(t => t.dto.Values.Select((valDto, valIdx) => (
                    Key: $"{t.optIdx}:{valIdx}",
                    Entity: MatchActiveVariationValue(t.Global, t.dto.Name, t.optIdx, valIdx, valDto.Value))))
                .ToDictionary(x => x.Key, x => x.Entity);

        /// <summary>
        /// Resolves keys to entities; throws if any key is unknown.
        /// </summary>
        public static List<Entities.VariationValue> ResolveVariationValues(
            IEnumerable<string> variationValueKeys,
            IReadOnlyDictionary<string, Entities.VariationValue> keyMap) =>
            variationValueKeys.Select(key => RequireKey(keyMap, key)).ToList();

        public static string BuildDisplayTitle(IEnumerable<Entities.VariationValue> values) =>
            string.Join(" / ", values.Select(v => v.Value));

        /// <summary>Stable key for matching the same combination across create/update.</summary>
        public static string BuildCombinationKey(IEnumerable<string> variationValueKeys)
        {
            var ordered = variationValueKeys.Order(StringComparer.Ordinal).ToList();
            return ordered.Count == 0 ? string.Empty : string.Join(",", ordered);
        }

        /// <summary>variation value id → "optIdx:valIdx".</summary>
        public static Dictionary<int, string> BuildVariationValueIdToKeyMap(
            IReadOnlyDictionary<string, Entities.VariationValue> keyMap) =>
            keyMap.ToDictionary(kvp => kvp.Value.Id, kvp => kvp.Key);

        /// <summary>Existing variants keyed by the same combination key as <see cref="BuildCombinationKey"/>.</summary>
        public static Dictionary<string, Entities.ProductVariant> IndexExistingVariantsByCombinationKey(
            IEnumerable<Entities.ProductVariant> existingVariants,
            IReadOnlyDictionary<int, string> variationValueIdToKey)
        {
            return existingVariants
                .Select(v =>
                {
                    var keys = v.VariationValues
                        .Select(vv => variationValueIdToKey.GetValueOrDefault(vv.Id))
                        .Where(k => !string.IsNullOrEmpty(k))
                        .Cast<string>();

                    var combinationKey = BuildCombinationKey(keys);
                    return new { combinationKey, v };
                })
                .Where(x => !string.IsNullOrEmpty(x.combinationKey))
                .ToDictionary(x => x.combinationKey, x => x.v);
        }

        private static Entities.VariationOption RequireGlobalOption(
            IReadOnlyDictionary<string, Entities.VariationOption> optionByName,
            string name)
        {
            if (!optionByName.TryGetValue(name, out var globalOption))
            {
                throw new InvalidOperationException(
                    $"Global variation option '{name}' not found. Create it first in Variation management.");
            }

            return globalOption;
        }

        private static Entities.VariationValue RequireKey(
            IReadOnlyDictionary<string, Entities.VariationValue> keyMap,
            string key) =>
            keyMap.TryGetValue(key, out var value)
                ? value
                : throw new InvalidOperationException(
                    $"Invalid variation key '{key}' for the configured options.");

        private static Entities.VariationValue MatchActiveVariationValue(
            Entities.VariationOption globalOption,
            string optionName,
            int optIdx,
            int valIdx,
            string? rawValue)
        {
            var str = (rawValue ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(str))
            {
                throw new InvalidOperationException(
                    $"Variation option '{optionName}' contains an empty value at index {valIdx}.");
            }

            var match = globalOption.Values
                .Where(v => v.IsActive && v.Value == str)
                .OrderBy(v => v.DisplayOrder)
                .ThenBy(v => v.Id)
                .FirstOrDefault();

            return match ?? throw new InvalidOperationException(
                $"Variation value '{str}' for option '{optionName}' (key {optIdx}:{valIdx}) is missing from the catalog. " +
                "Ensure SyncVariationValuesFromRequestAsync ran and options were reloaded.");
        }

        private static async Task UpsertVariationValueAsync(
            string optionName,
            Entities.VariationOption globalOption,
            int valIdx,
            ProductVariation.Request.VariationValueDto valDto,
            IApplicationDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var str = (valDto.Value ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(str))
            {
                throw new InvalidOperationException(
                    $"Variation option '{optionName}' contains an empty value at index {valIdx}.");
            }

            var displayOrder = valDto.DisplayOrder != 0 ? valDto.DisplayOrder : valIdx;
            var existing = await dbContext.VariationValues
                .FirstOrDefaultAsync(
                    vv => vv.VariationOptionId == globalOption.Id && vv.Value == str,
                    cancellationToken);

            if (existing is null)
            {
                await dbContext.VariationValues.AddAsync(
                    new Entities.VariationValue
                    {
                        Title = str,
                        Name = str,
                        Value = str,
                        VariationOptionId = globalOption.Id,
                        DisplayOrder = displayOrder,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        TenantId = globalOption.TenantId,
                        BranchId = globalOption.BranchId,
                    },
                    cancellationToken);
                return;
            }

            existing.IsActive = true;
            existing.DisplayOrder = displayOrder;
            existing.UpdatedAt = DateTime.UtcNow;
        }
    }
}
