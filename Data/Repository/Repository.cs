using Models.ViewModels;
using Data.Repository.IRepository;
using Models.DtoModels;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Data.Repository
{
	public class Repository<T> : IRepository<T> where T : class
	{
		private readonly ApplicationDbContext _db;
		internal DbSet<T> dbSet;
		public Repository(ApplicationDbContext db)
		{
			_db = db;
			this.dbSet = _db.Set<T>();
		}

		public void Add(T entity)
		{
			dbSet.Add(entity);
		}

		public void Attach(T entity)
		{
			dbSet.Attach(entity);
		}

		public void AddRange(IEnumerable<T> entity)
		{
			dbSet.AddRange(entity);
		}

		//Include prop - "Category,CoverType"
		public IEnumerable<T> GetAll(Expression<Func<T, bool>>? condition = null, string? includeProperties = null, Dictionary<string, FilterCondition>? dynamicFilters = null)
		{
			IQueryable<T> query = dbSet;
			if (condition != null)
			{
				query = query.Where(condition);
			}

			query = ApplyAdvancedFilters(query, dynamicFilters);

			if (includeProperties != null)
			{
				foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(includeProp);
				}
			}
			return query.ToList();
		}

        public IQueryable<T> GetAllQueriable(Expression<Func<T, bool>>? condition = null, string? includeProperties = null, Dictionary<string, FilterCondition>? dynamicFilters = null)
        {
            IQueryable<T> query = dbSet; // dbSet is DbSet<T> → IQueryable<T>

            // Apply filter if provided
            if (condition != null)
            {
                query = query.Where(condition);
            }

            query = ApplyAdvancedFilters(query, dynamicFilters);

            // Include navigation properties if provided
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp.Trim());
                }
            }

            return query; // ⚡ return IQueryable<T> (deferred execution)
        }


        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? condition = null, string? includeProperties = null, Dictionary<string, FilterCondition>? dynamicFilters = null)
        {
            IQueryable<T> query = dbSet;

            if (condition != null)
            {
                query = query.Where(condition);
            }

            query = ApplyAdvancedFilters(query, dynamicFilters);

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp.Trim());
                }
            }

            return await query.ToListAsync();
        }


        //Include prop - "Category,CoverType"
        public T GetById(Expression<Func<T, bool>>? condition, string? includeProperties = null)
		{
			IQueryable<T> query = dbSet;
			if (condition != null)
			{
				query = query.Where(condition);
			}

			if (includeProperties != null)
			{
				foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(includeProp);
				}
			}
			return query.FirstOrDefault();
		}

		//Paginated data
		public PaginatedViewModel<T> GetAllPaginated(int page = 1, int pageSize = 10, Expression<Func<T, bool>>? condition = null, string? includeProperties = null, string? sortBy = null, string? sortOrder = null, Dictionary<string, FilterCondition>? dynamicFilters = null)
		{
			IQueryable<T> query = dbSet;
			if (condition != null)
			{
				query = query.Where(condition);
			}

			query = ApplyAdvancedFilters(query, dynamicFilters);

			if (includeProperties != null)
			{
				foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(includeProp);
				}
			}

			// Apply sorting based on the provided parameters
			if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(sortOrder))
			{
				query = ApplySorting(query, sortBy, sortOrder);
			}			

			// Apply pagination
			var result = new PaginatedViewModel<T>
			{
				Data = query.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
				Page = page,
				PageSize = pageSize,
				TotalCount = query.Count()
			};

			return result;
		}

		public T? GetFirstOrDefault(Expression<Func<T, bool>>? filter, string? includeProperties = null)
		{
			IQueryable<T> query = dbSet;
			if (filter != null)
			{
				query = query.Where(filter);
			}

			if (includeProperties != null)
			{
				foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(includeProp);
				}
			}

			return query.FirstOrDefault();
		}

        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>>? filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp.Trim());
                }
            }

            return await query.FirstOrDefaultAsync();
        }


        public void Remove(T entity)
		{
			dbSet.Remove(entity);
		}

		public void RemoveRange(IEnumerable<T> entities)
		{
			dbSet.RemoveRange(entities);
		}

		private IQueryable<TEntity> ApplySorting<TEntity>(
		IQueryable<TEntity> query, string sortBy, string sortOrder) where TEntity : class
		{
			var entityType = typeof(TEntity);

			// Ensure sortBy is a valid property of the entity
			var property = entityType.GetProperty(sortBy);
			if (property == null)
			{
				return query;
			}

			var parameter = Expression.Parameter(entityType, "x");
			var propertyAccess = Expression.MakeMemberAccess(parameter, property);
			var orderByExp = Expression.Lambda(propertyAccess, parameter);

			//var parameter = Expression.Parameter(typeof(TEntity), "x");
			//var property = Expression.Property(parameter, sortBy);
			//var lambda = Expression.Lambda(property, parameter);

			switch (sortOrder.ToLower())
			{
				case "asc":
					query = Queryable.OrderBy(query, (dynamic)orderByExp);
					break;
				case "desc":
					query = Queryable.OrderByDescending(query, (dynamic)orderByExp);
					break;
				default:
					throw new ArgumentException("Invalid sortOrder");
			}

			return query;
		}

		private IQueryable<TEntity> ApplySearchFilter<TEntity>(IQueryable<TEntity> query, string search) where TEntity : class
		{
			var stringProperties = typeof(TEntity).GetProperties()
				.Where(prop => prop.PropertyType == typeof(string));

			var parameter = Expression.Parameter(typeof(TEntity), "x");
			Expression? combinedExpression = null;

			foreach (var property in stringProperties)
			{
				var propertyExpression = Expression.Property(parameter, property);
				var likeExpression = Expression.Call(
					typeof(DbFunctionsExtensions),
					"Like",
					new[] { typeof(string) },
					propertyExpression,
					Expression.Constant($"%{search}%")
				);

				if (combinedExpression == null)
				{
					combinedExpression = likeExpression;
				}
				else
				{
					combinedExpression = Expression.OrElse(combinedExpression, likeExpression);
				}
			}

			if (combinedExpression != null)
			{
				var lambda = Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameter);
				query = query.Where(lambda);
			}

			return query;
		}

		public void RmoveCondition(Expression<Func<T, bool>> condition)
		{
			IQueryable<T> query = dbSet;
			if (condition != null)
			{
				query = query.Where(condition);
			}
			query.ExecuteDelete();
		}

		public bool Any(Expression<Func<T, bool>> condition)
		{
			IQueryable<T> query = dbSet;
			return query.Any(condition); ;
		}

        private IQueryable<T> ApplyAdvancedFilters(IQueryable<T> query, Dictionary<string, FilterCondition>? filters)
        {
            if (filters == null || !filters.Any())
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? combinedBody = null;

            foreach (var filter in filters)
            {
                var propertyInfo = typeof(T).GetProperty(
                    filter.Key, 
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
                );

                if (propertyInfo == null)
                    continue;

                var condition = filter.Value;
                if (string.IsNullOrWhiteSpace(condition.Operator) || condition.Value == null)
                    continue;

                var memberAccess = Expression.Property(parameter, propertyInfo);
                var targetType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

                object? convertedValue = null;
                try
                {
                    if (targetType == typeof(DateTime))
                    {
                        if (DateTime.TryParse(condition.Value, out DateTime parsedDate))
                        {
                            convertedValue = parsedDate;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (targetType.IsEnum)
                    {
                        convertedValue = Enum.Parse(targetType, condition.Value, true);
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(condition.Value, targetType);
                    }
                }
                catch
                {
                    continue;
                }

                Expression valueExpression = Expression.Constant(convertedValue, propertyInfo.PropertyType);
                Expression comparison;

                switch (condition.Operator.ToLower())
                {
                    case "eq":
                        comparison = Expression.Equal(memberAccess, valueExpression);
                        break;
                    case "neq":
                        comparison = Expression.NotEqual(memberAccess, valueExpression);
                        break;
                    case "gt":
                        comparison = Expression.GreaterThan(memberAccess, valueExpression);
                        break;
                    case "gte":
                        comparison = Expression.GreaterThanOrEqual(memberAccess, valueExpression);
                        break;
                    case "lt":
                        comparison = Expression.LessThan(memberAccess, valueExpression);
                        break;
                    case "lte":
                        comparison = Expression.LessThanOrEqual(memberAccess, valueExpression);
                        break;
                    case "contains":
                        if (targetType != typeof(string)) continue;
                        comparison = Expression.Call(
                            memberAccess,
                            typeof(string).GetMethod("Contains", new[] { typeof(string) })!,
                            valueExpression
                        );
                        break;
                    case "startswith":
                        if (targetType != typeof(string)) continue;
                        comparison = Expression.Call(
                            memberAccess,
                            typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!,
                            valueExpression
                        );
                        break;
                    default:
                        continue;
                }

                combinedBody = combinedBody == null 
                    ? comparison 
                    : Expression.AndAlso(combinedBody, comparison);
            }

            if (combinedBody == null)
                return query;

            var predicate = Expression.Lambda<Func<T, bool>>(combinedBody, parameter);
            return query.Where(predicate);
        }
	}
}
