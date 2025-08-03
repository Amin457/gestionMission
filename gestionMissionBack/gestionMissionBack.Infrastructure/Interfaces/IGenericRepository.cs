    using gestionMissionBack.Domain.Helpers;

    namespace gestionMissionBack.Infrastructure.Interfaces
    {
        public interface IGenericRepository<T> where T : class
        {
            Task<IEnumerable<T>> GetAllAsync();
            Task<T?> GetByIdAsync(int id);
            Task<T> AddAsync(T entity);
            Task<bool> UpdateAsync(T entity);
            Task<bool> DeleteAsync(int id);
            Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize);
            IQueryable<T> GetQueryable();

    }
}
