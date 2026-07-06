using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class QueryHelper
    {
        // 🔍 Search
        public static IQueryable<T> ApplySearch<T>(
         IQueryable<T> query,
         string? search,
         params Expression<Func<T, string>>[] searchFields
     )
        {
            if (string.IsNullOrWhiteSpace(search) || searchFields.Length == 0)
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? body = null;

            foreach (var field in searchFields)
            {
                var replaced = new ReplaceParameterVisitor(field.Parameters[0], parameter)
                    .Visit(field.Body);

                var containsMethod = Expression.Call(
                    replaced!,
                    nameof(string.Contains),
                    Type.EmptyTypes,
                    Expression.Constant(search)
                );

                body = body == null
                    ? containsMethod
                    : Expression.OrElse(body, containsMethod);
            }

            var predicate = Expression.Lambda<Func<T, bool>>(body!, parameter);
            return query.Where(predicate);
        }

        class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParam;
            private readonly ParameterExpression _newParam;

            public ReplaceParameterVisitor(ParameterExpression oldParam, ParameterExpression newParam)
            {
                _oldParam = oldParam;
                _newParam = newParam;
            }

            protected override Expression VisitParameter(ParameterExpression node)
                => node == _oldParam ? _newParam : base.VisitParameter(node);
        }



        public static IQueryable<T> ApplySorting<T>(
    IQueryable<T> query,
    string? sortBy,
    string? sortDirection)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query;

            var propertyInfo = typeof(T)
                .GetProperties()
                .FirstOrDefault(p =>
                    p.Name.Equals(sortBy, StringComparison.OrdinalIgnoreCase));

            if (propertyInfo == null)
                return query; // or throw a controlled 400 error

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyInfo);
            var lambda = Expression.Lambda(property, parameter);

            return sortDirection?.ToLower() == "desc"
                ? Queryable.OrderByDescending(query, (dynamic)lambda)
                : Queryable.OrderBy(query, (dynamic)lambda);
        }

        // 📄 Pagination
        public static IQueryable<T> ApplyPagination<T>(
            IQueryable<T> query,
            int pageNumber,
            int pageSize
        )
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            return query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }


    }
}
