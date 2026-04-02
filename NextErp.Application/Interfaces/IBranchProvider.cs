namespace NextErp.Application.Interfaces
{
    public interface IBranchProvider
    {
        Guid? GetBranchId();

        Guid GetRequiredBranchId();

        bool IsGlobal();
    }
}
