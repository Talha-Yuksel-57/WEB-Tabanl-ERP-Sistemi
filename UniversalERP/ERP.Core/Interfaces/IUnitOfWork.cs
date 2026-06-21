using ERP.Core.Entities;

namespace ERP.Core.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<Product> Products { get; }

        IRepository<Customer> Customers { get; }

        IRepository<Sale> Sales { get; }

        IRepository<ServiceOrder> ServiceOrders { get; }

        IRepository<ProjectTask> ProjectTasks { get; }

        Task<int> CompleteAsync();
    }
}