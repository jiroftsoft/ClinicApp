using System;
using System.Linq;
using System.Linq.Expressions;
using ClinicApp.Models.Core;

namespace ClinicApp.Infrastructure;

public static class QueryExtensions
{
    public static IQueryable<T> ActiveOnly<T>(this IQueryable<T> query) where T : class
    {
        var isActiveProperty = typeof(T).GetProperty("IsActive");
        if (isActiveProperty != null)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, "IsActive");
            var constant = Expression.Constant(true);
            var equality = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);
            return query.Where(lambda);
        }
        return query;
    }

    public static IQueryable<T> NotDeleted<T>(this IQueryable<T> query) where T : ISoftDelete
    {
        return query.Where(x => !x.IsDeleted);
    }
}