namespace NextErp.Application.DTOs.Common
{
    /// <summary>
    /// Generic batch payload for endpoints that operate on multiple ids in one request
    /// (e.g. batch deactivate). Validation of count + non-empty lives in the per-command
    /// validator so this DTO stays a thin transport contract.
    /// </summary>
    public class BatchIdsDto<T>
    {
        public List<T> Ids { get; set; } = new();
    }
}
