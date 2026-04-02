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
public class CategoryController(IMediator mediator, IMapper mapper, IImageService imageService) : ControllerBase
{
    // GET api/category/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var query = new GetCategoryByIdQuery(id);
        var category = await mediator.Send(query);

        if (category == null) return NotFound();

        var dto = mapper.Map<Category.Response.Get.Single>(category);
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

        var dtoList = mapper.Map<List<Category.Response.Get.Single>>(pagedResult.Records);

        return Ok(new
        {
            total = pagedResult.Total,
            totalDisplay = pagedResult.TotalDisplay,
            data = dtoList
        });
    }

    // POST api/category
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] Category.Request.Create.Single dto)
    {
        // Upload images and create assets
        if (dto.Images != null && dto.Images.Length > 0)
        {
            dto.Assets = new List<Category.Request.Asset>();
            foreach (var image in dto.Images)
            {
                var imageUrl = await imageService.UploadImageAsync(image);
                dto.Assets.Add(new Category.Request.Asset
                {
                    Filename = image.FileName,
                    Url = imageUrl,
                    Type = "image",
                    Size = image.Length,
                    UploadedAt = DateTime.UtcNow
                });
            }
        }

        var command = mapper.Map<CreateCategoryCommand>(dto);
        var id = await mediator.Send(command);

        var category = await mediator.Send(new GetCategoryByIdQuery(id));
        var response = mapper.Map<Category.Response.Get.Single>(category);

        return CreatedAtAction(nameof(Get), new { id }, response);
    }

    // PUT api/category/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] Category.Request.Update.Single dto)
    {
        // Upload new images and add to assets
        if (dto.Images != null && dto.Images.Length > 0)
        {
            if (dto.Assets == null)
            {
                dto.Assets = new List<Category.Request.Asset>();
            }
            foreach (var image in dto.Images)
            {
                var imageUrl = await imageService.UploadImageAsync(image);
                dto.Assets.Add(new Category.Request.Asset
                {
                    Filename = image.FileName,
                    Url = imageUrl,
                    Type = "image",
                    Size = image.Length,
                    UploadedAt = DateTime.UtcNow
                });
            }
        }

        var command = mapper.Map<UpdateCategoryCommand>(dto, opts => opts.Items["Id"] = id);
        await mediator.Send(command);
        return NoContent();
    }
}
