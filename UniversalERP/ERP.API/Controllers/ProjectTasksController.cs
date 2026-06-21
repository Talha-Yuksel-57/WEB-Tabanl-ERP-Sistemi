using ERP.Core.DTOs.ProjectTask;
using ERP.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectTasksController : ControllerBase
    {
        private readonly IProjectTaskService _taskService;

        public ProjectTasksController(IProjectTaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _taskService.GetAllAsync();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await _taskService.GetByIdAsync(id);
            if (task == null) return NotFound("Görev bulunamadı.");
            return Ok(task);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,Manager")]
        public async Task<IActionResult> Create([FromBody] CreateProjectTaskDto dto)
        {
            var task = await _taskService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,Manager,Employee")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectTaskDto dto)
        {
            var task = await _taskService.UpdateAsync(id, dto);
            if (task == null) return NotFound("Görev bulunamadı.");
            return Ok(task);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _taskService.DeleteAsync(id);
            if (!success) return NotFound("Görev bulunamadı.");
            return Ok(new { message = "Görev silindi." });
        }
    }
}
