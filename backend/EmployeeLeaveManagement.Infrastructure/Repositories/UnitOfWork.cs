using System.Collections.Concurrent;
using EmployeeLeaveManagement.Core.Interfaces;
using EmployeeLeaveManagement.Infrastructure.Data;

namespace EmployeeLeaveManagement.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ConcurrentDictionary<string, object> _repositories = new();

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        var typeName = typeof(T).Name;

        return (IGenericRepository<T>)_repositories.GetOrAdd(typeName, _ =>
            new GenericRepository<T>(_context));
    }

    public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}
