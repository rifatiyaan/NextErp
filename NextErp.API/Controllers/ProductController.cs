using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands;
using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ProductController(IMediator mediator, IMapper mapper, IImageService imageService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var query = new GetProductByIdQuery(id);
        var product = await mediator.Send(query);

        if (product == null)
            return NotFound();

        var dto = mapper.Map<Product.Response.Get.Single>(product);
        return Ok(dto);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int? page = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchText = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? status = null,
        [FromQuery] bool includeStock = false)
    {
        var effectivePage = page ?? pageIndex ?? 1;
        if (effectivePage < 1) effectivePage = 1;
        if (pageSize < 1) pageSize = 10;

        var query = new GetPagedProductsQuery(
            effectivePage,
            pageSize,
            searchText,
            sortBy,
            categoryId,
            status,
            includeStock);
        var pagedResult = await mediator.Send(query);

        return Ok(new
        {
            total = pagedResult.Total,
            totalDisplay = pagedResult.TotalDisplay,
            page = effectivePage,
            pageSize,
            data = pagedResult.Records
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] Product.Request.Create.Single dto)
    {
        await ResolveProductGalleryAsync(dto, HttpContext.RequestAborted);

        int id;

        bool hasValidVariations = dto.HasVariations &&
            dto.VariationOptions != null && dto.VariationOptions.Any() &&
            dto.ProductVariants != null && dto.ProductVariants.Any();

        var createGallery = NormalizeCreateGallery(dto);

        if (hasValidVariations)
        {
            var command = new CreateProductWithVariationsCommand(
                dto.Title,
                dto.Code,
                dto.ParentId,
                dto.CategoryId ?? 0,
                dto.Price,
                dto.Stock,
                dto.IsActive,
                dto.ImageUrl,
                createGallery,
                dto.Metadata?.Description,
                dto.Metadata?.Color,
                dto.Metadata?.Warranty,
                dto.VariationOptions!,
                dto.ProductVariants!);
            id = await mediator.Send(command);
        }
        else
        {
            var command = mapper.Map<CreateProductCommand>(dto);
            id = await mediator.Send(command);
        }

        var product = await mediator.Send(new GetProductByIdQuery(id));
        var response = mapper.Map<Product.Response.Get.Single>(product);

        return CreatedAtAction(nameof(Get), new { id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] Product.Request.Update.Single dto)
    {
        dto.Id = id;
        await ResolveProductGalleryAsync(dto, HttpContext.RequestAborted);

        bool hasValidVariations = dto.HasVariations &&
            dto.VariationOptions != null && dto.VariationOptions.Any() &&
            dto.ProductVariants != null && dto.ProductVariants.Any();

        var thumbUpdates = dto.ProductImageThumbnailUpdates is { Count: > 0 }
            ? dto.ProductImageThumbnailUpdates
            : null;

        if (hasValidVariations)
        {
            var command = new UpdateProductWithVariationsCommand(
                id,
                dto.Title,
                dto.Code,
                dto.ParentId,
                dto.CategoryId ?? 0,
                dto.Price,
                dto.Stock,
                dto.IsActive,
                dto.ImageUrl,
                dto.ResolvedGallery,
                thumbUpdates,
                dto.Metadata?.Description,
                dto.Metadata?.Color,
                dto.Metadata?.Warranty,
                dto.VariationOptions!,
                dto.ProductVariants!);
            await mediator.Send(command);
        }
        else
        {
            var command = mapper.Map<UpdateProductCommand>(dto, opts => opts.Items["Id"] = id);
            await mediator.Send(command);
        }

        return NoContent();
    }

    private static IReadOnlyList<Product.Request.GalleryResolvedSlot> NormalizeCreateGallery(Product.Request.Create.Single dto)
    {
        if (dto.ResolvedGallery is { Count: > 0 })
            return dto.ResolvedGallery;
        if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            return new List<Product.Request.GalleryResolvedSlot> { new(dto.ImageUrl.Trim(), true) };
        return Array.Empty<Product.Request.GalleryResolvedSlot>();
    }

    /// <summary>Resolves uploads into <see cref="Product.Request.Base.ResolvedGallery"/>; sets primary <see cref="Product.Request.Base.ImageUrl"/> when the gallery is rebuilt.</summary>
    private async Task ResolveProductGalleryAsync(Product.Request.Base dto, CancellationToken cancellationToken)
    {
        var isUpdate = dto is Product.Request.Update.Single;

        if (dto.ClearGallery)
        {
            dto.ResolvedGallery = new List<Product.Request.GalleryResolvedSlot>();
            dto.ImageUrl = null;
            return;
        }

        if (dto.ImageSlots is { Count: > 0 })
        {
            var list = new List<Product.Request.GalleryResolvedSlot>();
            foreach (var slot in dto.ImageSlots)
            {
                string? url = null;
                if (!string.IsNullOrWhiteSpace(slot.Url))
                    url = slot.Url.Trim();
                else if (slot.File != null)
                    url = await imageService.UploadImageAsync(slot.File);
                if (url != null)
                    list.Add(new Product.Request.GalleryResolvedSlot(url, slot.IsThumbnail));
            }

            if (list.Count > 0 && !list.Any(x => x.IsThumbnail))
                list[0] = new Product.Request.GalleryResolvedSlot(list[0].Url, true);

            dto.ResolvedGallery = list;
            dto.ImageUrl = list.FirstOrDefault(x => x.IsThumbnail)?.Url ?? list[0].Url;
            return;
        }

        if (dto.Image != null)
        {
            var url = await imageService.UploadImageAsync(dto.Image);
            dto.ResolvedGallery = new List<Product.Request.GalleryResolvedSlot> { new(url, true) };
            dto.ImageUrl = url;
            return;
        }

        // Create only: legacy single URL without slots (update must use slots or thumbnail flags so multi-image galleries are not collapsed).
        if (!isUpdate && !string.IsNullOrWhiteSpace(dto.ImageUrl))
        {
            var url = dto.ImageUrl.Trim();
            dto.ResolvedGallery = new List<Product.Request.GalleryResolvedSlot> { new(url, true) };
            return;
        }

        dto.ResolvedGallery = null;
    }
}
