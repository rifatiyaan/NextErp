using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs.Branch;
using NextErp.Application.Interfaces;
using NextErp.Application.Mapping;
using DomainBranch = NextErp.Domain.Entities.Branch;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class BranchController(
    IApplicationDbContext dbContext,
    IBranchProvider branchProvider) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        IQueryable<DomainBranch> query = dbContext.Branches.AsNoTracking().OrderByDescending(x => x.CreatedAt);
        if (!branchProvider.IsGlobal())
        {
            var branchId = branchProvider.GetRequiredBranchId();
            query = query.Where(x => x.Id == branchId);
        }

        var entities = await query.ToListAsync(cancellationToken);
        var response = entities.Select(e => e.ToResponse()).ToList();
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        if (!branchProvider.IsGlobal() && branchProvider.GetRequiredBranchId() != id)
            return NotFound();

        var entity = await dbContext.Branches.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null)
            return NotFound();

        var response = entity.ToResponse();
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBranchRequest dto, CancellationToken cancellationToken = default)
    {
        if (!branchProvider.IsGlobal())
            return Forbid();

        var entity = dto.ToEntity();
        entity.Id = Guid.NewGuid();
        entity.TenantId = Guid.Empty;
        entity.CreatedAt = DateTime.UtcNow;

        await dbContext.Branches.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = entity.ToCreateResponse();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBranchRequest dto, CancellationToken cancellationToken = default)
    {
        if (!branchProvider.IsGlobal())
            return Forbid();

        var entity = await dbContext.Branches.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null)
            return NotFound();

        dto.ApplyTo(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        if (!branchProvider.IsGlobal())
            return Forbid();

        var entity = await dbContext.Branches.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null)
            return NotFound();

        if (!entity.IsActive)
            return NoContent();

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
