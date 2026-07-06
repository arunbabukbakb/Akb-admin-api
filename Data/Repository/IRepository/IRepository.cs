using Models.ViewModels;
using Models.DtoModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.IRepository
{
	public interface IRepository<T> where T : class
	{
		T? GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null);
		Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>>? filter, string? includeProperties = null);
		IEnumerable<T> GetAll(Expression<Func<T, bool>>? condition = null, string? includeProperties = null, Dictionary<string, FilterCondition>? dynamicFilters = null);
		Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? condition = null, string? includeProperties = null, Dictionary<string, FilterCondition>? dynamicFilters = null);
		T GetById(Expression<Func<T, bool>>? condition = null, string? includeProperties = null);
		PaginatedViewModel<T> GetAllPaginated(int page = 1, int pageSize = 10, Expression<Func<T, bool>>? condition = null, string? includeProperties = null, string? sortBy = null, string? sortOrder = null, Dictionary<string, FilterCondition>? dynamicFilters = null);
		void Add(T entity);
		void Attach(T entity);
		void AddRange(IEnumerable<T> entity);
		void Remove(T entity);
		void RemoveRange(IEnumerable<T> entities);
		void RmoveCondition(Expression<Func<T, bool>> condition);
		bool Any(Expression<Func<T, bool>> condition);

		IQueryable<T> GetAllQueriable(Expression<Func<T, bool>>? condition = null, string? includeProperties = null, Dictionary<string, FilterCondition>? dynamicFilters = null);
	}
}
