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
        /// Maps "optionIndex:valueIndex" → <see cref="Entities.VariationValue"/> from request option order
        /// and resolved global options (active values by display order).
        /// </summary>
        public static Dictionary<string, Entities.VariationValue> BuildValueKeyMap(
            IReadOnlyList<ProductVariation.Request.VariationOptionDto> optionsInRequestOrder,
            IReadOnlyDictionary<string, Entities.VariationOption> optionByName)
        {
            return optionsInRequestOrder
                .Select((dto, optIdx) =>
                {
                    if (!optionByName.TryGetValue(dto.Name, out var globalOption))
                    {
                        throw new InvalidOperationException(
                            $"Global variation option '{dto.Name}' not found. Create it first in Variation management.");
                    }

                    return (OptIdx: optIdx, Option: globalOption);
                })
                .SelectMany(ctx => ctx.Option.Values
                    .Where(v => v.IsActive)
                    .OrderBy(v => v.DisplayOrder)
                    .Select((val, valIdx) => ($"{ctx.OptIdx}:{valIdx}", val)))
                .ToDictionary(t => t.Item1, t => t.Item2);
        }

        /// <summary>
        /// Resolves keys to entities; throws if any key is unknown.
        /// </summary>
        public static List<Entities.VariationValue> ResolveVariationValues(
            IEnumerable<string> variationValueKeys,
            IReadOnlyDictionary<string, Entities.VariationValue> keyMap)
        {
            return variationValueKeys
                .Select(key =>
                {
                    if (!keyMap.TryGetValue(key, out var value))
                    {
                        throw new InvalidOperationException(
                            $"Invalid variation key '{key}' for the configured options.");
                    }

                    return value;
                })
                .ToList();
        }

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
    }
}
