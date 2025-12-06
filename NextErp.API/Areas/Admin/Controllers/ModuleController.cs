using AutoMapper;
using NextErp.Application.Commands.Module;
using NextErp.Application.DTOs;
using NextErp.Application.Queries.Module;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace NextErp.API.Web.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ModuleController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public ModuleController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Get menu items for the current authenticated user based on their roles
        /// </summary>
        [HttpGet("user-menu")]
        public async Task<IActionResult> GetUserMenu()
        {
            // Extract user roles from claims
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToArray();

            // Extract tenant ID from claims (adjust claim type as needed)
            var tenantIdClaim = User.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                // Default tenant or return error based on your requirements
                tenantId = Guid.Empty;
            }

            var query = new GetMenuByUserQuery(roles, tenantId);
            var menuItems = await _mediator.Send(query);

            return Ok(menuItems);
        }

        /// <summary>
        /// Get all modules/menu items (admin only)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? tenantId = null)
        {
            var effectiveTenantId = tenantId ?? Guid.Empty;
            
            var query = new GetAllModulesQuery(effectiveTenantId);
            var modules = await _mediator.Send(query);

            return Ok(modules);
        }

        /// <summary>
        /// Get modules/menu items by type (1=Module, 2=Link)
        /// </summary>
        [HttpGet("by-type/{type}")]
        public async Task<IActionResult> GetByType(int type, [FromQuery] Guid? tenantId = null)
        {
            var effectiveTenantId = tenantId ?? Guid.Empty;
            
            var query = new GetModulesByTypeQuery(type, effectiveTenantId);
            var modules = await _mediator.Send(query);

            return Ok(modules);
        }

        /// <summary>
        /// Get a specific module/menu item by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var query = new GetModuleByIdQuery(id);
            var module = await _mediator.Send(query);

            if (module == null)
                return NotFound();

            return Ok(module);
        }

        /// <summary>
        /// Create a new module/menu item
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ModuleRequestDto dto)
        {
            var command = new CreateModuleCommand(dto);
            var id = await _mediator.Send(command);

            var module = await _mediator.Send(new GetModuleByIdQuery(id));
            return CreatedAtAction(nameof(Get), new { id }, module);
        }

        /// <summary>
        /// Update an existing module/menu item
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ModuleRequestDto dto)
        {
            var command = new UpdateModuleCommand(id, dto);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost("bulk")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<int>>> BulkCreate([FromBody] List<BulkModuleDto> modules)
        {
            var result = await _mediator.Send(new BulkCreateModuleCommand { Modules = modules });
            return Ok(result);
        }

        /// <summary>
        /// Delete a module/menu item
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteModuleCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
