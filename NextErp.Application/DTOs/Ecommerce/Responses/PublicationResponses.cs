namespace NextErp.Application.DTOs.Ecommerce;

public sealed record PublicationProductRow(int Id, string Title, string Code, decimal Price, bool IsPublishedOnline);

public sealed record PublicationCategoryResponse(
    int Id, string Title, int? ParentId, bool IsPublishedOnline, List<PublicationProductRow> Products);
