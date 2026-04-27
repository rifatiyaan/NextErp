using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;
using BranchDto = NextErp.Application.DTOs.Branch;
using DomainBranch = NextErp.Domain.Entities.Branch;

namespace NextErp.API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class BranchController(
    IApplicationDbContext dbContext,
    IMapper mapper,
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
        var response = mapper.Map<List<BranchDto.Response.Get.Single>>(entities);
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

        var response = mapper.Map<BranchDto.Response.Get.Single>(entity);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BranchDto.Request.Create.Single dto, CancellationToken cancellationToken = default)
    {
        if (!branchProvider.IsGlobal())
            return Forbid();

        var entity = mapper.Map<DomainBranch>(dto);
        entity.Id = Guid.NewGuid();
        entity.TenantId = Guid.Empty;
        entity.CreatedAt = DateTime.UtcNow;

        await dbContext.Branches.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = mapper.Map<BranchDto.Response.Create.Single>(entity);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] BranchDto.Request.Update.Single dto, CancellationToken cancellationToken = default)
    {
        if (!branchProvider.IsGlobal())
            return Forbid();

        var entity = await dbContext.Branches.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null)
            return NotFound();

        mapper.Map(dto, entity);
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
