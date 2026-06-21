using ERP.Core.DTOs.ProjectTask;

namespace ERP.Core.Interfaces
{
    public interface IProjectTaskService
    {
        Task<IEnumerable<ProjectTaskDto>> GetAllAsync();
        Task<ProjectTaskDto?> GetByIdAsync(int id);
        Task<ProjectTaskDto> CreateAsync(CreateProjectTaskDto dto);
        Task<ProjectTaskDto?> UpdateAsync(int id, UpdateProjectTaskDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
