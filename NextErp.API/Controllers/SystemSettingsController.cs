using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Commands.SystemSettings;
using NextErp.Application.DTOs.Ecommerce;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using NextErp.Application.Queries.Ecommerce;
using NextErp.Application.DTOs.SystemSettings;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SystemSettingsController(IMediator mediator, IImageService imageService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SystemSettingsResponse>> Get(CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetSystemSettingsQuery(), ct);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<SystemSettingsResponse>> Update(
        [FromBody] UpdateSystemSettingsRequest dto,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new UpdateSystemSettingsCommand(dto), ct);
        return Ok(result);
    }

    [HttpPost("reset")]
    public async Task<ActionResult<SystemSettingsResponse>> Reset(CancellationToken ct = default)
    {
        var result = await mediator.Send(new ResetSystemSettingsCommand(), ct);
        return Ok(result);
    }

    [HttpPost("logo")]
    public async Task<ActionResult<object>> UploadLogo(IFormFile file, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });

        // Sanity guards — Cloudinary handles real validation, but cheap rejects help error UX.
        if (file.Length > 2 * 1024 * 1024)
            return BadRequest(new { error = "Logo must be under 2 MB." });

        var contentType = file.ContentType?.ToLowerInvariant() ?? string.Empty;
        if (!contentType.StartsWith("image/"))
            return BadRequest(new { error = "Logo must be an image (PNG, JPG, SVG, WebP)." });

        var url = await imageService.UploadImageAsync(file);
        return Ok(new { url });
    }

    // Generic image upload (banners, hero slides, etc.). Returns the CDN URL.
    [HttpPost("image")]
    public async Task<ActionResult<object>> UploadImage(IFormFile file, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided." });
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { error = "Image must be under 5 MB." });

        var contentType = file.ContentType?.ToLowerInvariant() ?? string.Empty;
        if (!contentType.StartsWith("image/"))
            return BadRequest(new { error = "File must be an image (PNG, JPG, WebP)." });

        var url = await imageService.UploadImageAsync(file);
        return Ok(new { url });
    }

    [HttpGet("ecommerce/hero-slides")]
    public async Task<ActionResult<List<StoreHeroSlide>>> GetHeroSlides(CancellationToken ct = default)
        => Ok(await mediator.Send(new GetEcommerceHeroSlidesQuery(), ct));

    [HttpPut("ecommerce/hero-slides")]
    public async Task<ActionResult<object>> UpdateHeroSlides([FromBody] List<StoreHeroSlide> slides, CancellationToken ct = default)
    {
        var count = await mediator.Send(new UpdateEcommerceHeroSlidesCommand(slides ?? new List<StoreHeroSlide>()), ct);
        return Ok(new { count });
    }
}

