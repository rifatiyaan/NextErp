using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands;
using NextErp.Application.DTOs.Category;
using NextErp.Application.DTOs.Common;
using NextErp.Application.Interfaces;
using NextErp.Application.Mapping;
using NextErp.Application.Queries;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CategoryController(IMediator mediator, IImageService imageService) : ControllerBase
{
    // GET api/category/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var query = new GetCategoryByIdQuery(id);
        var category = await mediator.Send(query);

        if (category == null) return NotFound();

        var dto = category.ToResponse();
        return Ok(dto);
    }

    // GET api/category
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchText = null,
        [FromQuery] string? sortBy = null)
    {
        var query = new GetPagedCategoriesQuery(pageIndex, pageSize, searchText, sortBy);
        var pagedResult = await mediator.Send(query);

        var dtoList = pagedResult.Records.Select(c => c.ToResponse()).ToList();

        return Ok(new
        {
            total = pagedResult.Total,
            totalDisplay = pagedResult.TotalDisplay,
            data = dtoList
        });
    }

    // POST api/category
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateCategoryRequest dto)
    {
        // Upload images and create assets
        if (dto.Images != null && dto.Images.Length > 0)
        {
            dto.Assets = new List<CategoryAssetRequest>();
            foreach (var image in dto.Images)
            {
                var imageUrl = await imageService.UploadImageAsync(image);
                dto.Assets.Add(new CategoryAssetRequest
                {
                    Filename = image.FileName,
                    Url = imageUrl,
                    Type = "image",
                    Size = image.Length,
                    UploadedAt = DateTime.UtcNow
                });
            }
        }

        var command = dto.ToCommand();
        var id = await mediator.Send(command);

        var category = await mediator.Send(new GetCategoryByIdQuery(id));
        var response = category?.ToResponse();

        return CreatedAtAction(nameof(Get), new { id }, response);
    }

    // POST api/category/batch/deactivate
    // Multi-row soft delete; transactional via the ITransactionalRequest pipeline behaviour.
    [HttpPost("batch/deactivate")]
    public async Task<ActionResult<object>> BatchDeactivate(
        [FromBody] BatchIdsDto<int> dto,
        CancellationToken cancellationToken = default)
    {
        var count = await mediator.Send(
            new BatchDeactivateCategoriesCommand(dto?.Ids ?? new List<int>()),
            cancellationToken);
        return Ok(new { deactivated = count });
    }

    // PUT api/category/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateCategoryRequest dto)
    {
        // Upload new images and add to assets
        if (dto.Images != null && dto.Images.Length > 0)
        {
            if (dto.Assets == null)
            {
                dto.Assets = new List<CategoryAssetRequest>();
            }
            foreach (var image in dto.Images)
            {
                var imageUrl = await imageService.UploadImageAsync(image);
                dto.Assets.Add(new CategoryAssetRequest
                {
                    Filename = image.FileName,
                    Url = imageUrl,
                    Type = "image",
                    Size = image.Length,
                    UploadedAt = DateTime.UtcNow
                });
            }
        }

        dto.Id = id;
        var command = dto.ToCommand();
        await mediator.Send(command);
        return NoContent();
    }
}
