using ERP.Core.Entities;
using ERP.Core.Interfaces;
using ERP.Data.Repositories;

namespace ERP.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            Products =
                new Repository<Product>(_context);

            Customers =
                new Repository<Customer>(_context);

            Sales =
                new Repository<Sale>(_context);

            ServiceOrders =
                new Repository<ServiceOrder>(_context);

            ProjectTasks =
                new Repository<ProjectTask>(_context);
        }

        public IRepository<Product> Products { get; }

        public IRepository<Customer> Customers { get; }

        public IRepository<Sale> Sales { get; }

        public IRepository<ServiceOrder> ServiceOrders { get; }

        public IRepository<ProjectTask> ProjectTasks { get; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}