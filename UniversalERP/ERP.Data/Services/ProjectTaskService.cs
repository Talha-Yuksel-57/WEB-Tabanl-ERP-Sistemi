using AutoMapper;
using ERP.Core.DTOs.ProjectTask;
using ERP.Core.Interfaces;

namespace ERP.Data.Services
{
    public class ProjectTaskService : IProjectTaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProjectTaskService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProjectTaskDto>> GetAllAsync()
        {
            var tasks = await _unitOfWork.ProjectTasks.GetAllAsync();
            return _mapper.Map<IEnumerable<ProjectTaskDto>>(tasks);
        }

        public async Task<ProjectTaskDto?> GetByIdAsync(int id)
        {
            var task = await _unitOfWork.ProjectTasks.GetByIdAsync(id);
            return task == null ? null : _mapper.Map<ProjectTaskDto>(task);
        }

        public async Task<ProjectTaskDto> CreateAsync(CreateProjectTaskDto dto)
        {
            var task = _mapper.Map<Core.Entities.ProjectTask>(dto);
            task.Status = "Yapılacak";
            await _unitOfWork.ProjectTasks.AddAsync(task);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<ProjectTaskDto>(task);
        }

        public async Task<ProjectTaskDto?> UpdateAsync(int id, UpdateProjectTaskDto dto)
        {
            var task = await _unitOfWork.ProjectTasks.GetByIdAsync(id);
            if (task == null) return null;

            _mapper.Map(dto, task);
            _unitOfWork.ProjectTasks.Update(task);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<ProjectTaskDto>(task);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _unitOfWork.ProjectTasks.GetByIdAsync(id);
            if (task == null) return false;

            task.IsDeleted = true;
            _unitOfWork.ProjectTasks.Update(task);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
