using TasksTracker.Contracts.Interfaces;
using TasksTracker.Contracts.Models;

namespace TasksTracker.Sqlite.Repositories;

public class TaskEntryRepository : Repository<TaskEntry>, ITaskEntryRepository
{
    public TaskEntryRepository(IDatabaseContext context) : base(context)
    { }

    public IEnumerable<TaskEntry> GetByCategoryList(Guid categoryId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<TaskEntry>> GetByCategoryListAsync(Guid categoryId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<TaskEntry> GetTodayList(Guid categoryId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<TaskEntry>> GetTodayListAsync(Guid categoryId)
    {
        throw new NotImplementedException();
    }
    
    protected override void Insert(TaskEntry entity)
    {
        throw new NotImplementedException();
    }

    protected override async Task InsertAsync(TaskEntry entity)
    {
        throw new NotImplementedException();
    }

    protected override void Update(TaskEntry entity)
    {
        throw new NotImplementedException();
    }

    protected override async Task UpdateAsync(TaskEntry entity)
    {
        throw new NotImplementedException();
    }
}