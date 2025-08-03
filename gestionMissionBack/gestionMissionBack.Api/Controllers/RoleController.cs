using gestionMissionBack.Application.DTOs.Role;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gestionMissionBack.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        }

        // GET: api/role
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        // GET: api/role/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> GetRole(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            return Ok(role);
        }

        // GET: api/role/name/Admin
        [HttpGet("name/{roleName}")]
        public async Task<ActionResult<RoleDto>> GetRoleByName(string roleName)
        {
            var role = await _roleService.GetRoleByNameAsync(roleName);
            return Ok(role);
        }

        //[Authorize(Policy = "AdminOnly")]
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var pagedUsers = await _roleService.GetPagedAsync(pageNumber, pageSize);
            return Ok(pagedUsers);
        }

        // POST: api/role
        [HttpPost]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] RoleDto roleDto)
        {
            var createdRole = await _roleService.CreateRoleAsync(roleDto);
            return CreatedAtAction(nameof(GetRole), new { id = createdRole.RoleId }, createdRole);
        }

        // PUT: api/role/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleDto roleDto)
        {
            if (id != roleDto.RoleId)
            {
                throw new ArgumentException("Role ID in URL must match Role ID in body");
            }

            await _roleService.UpdateRoleAsync(roleDto);
            return NoContent();
        }

        // DELETE: api/role/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            await _roleService.DeleteRoleAsync(id);
            return NoContent();
        }
    }
}