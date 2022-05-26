namespace System.Linq.Expressions;

public static class ExpressionHelper
{
    /// <summary>
    /// 将多个条件 使用 OR 连接成一个表达式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="predicates">多个条件表达式</param>
    /// <returns></returns>
    public static Expression<Func<T, bool>> WhereOr<T>(
        IReadOnlyList<Expression<Func<T, bool>>> predicates)
    {
        if (predicates.Count == 0) return x => false; // no matches!
        if (predicates.Count == 1) return predicates[0]; // simple
        var param = Expression.Parameter(typeof(T), "x");
        Expression body = Expression.Invoke(predicates[0], param);
        for (int i = 1; i < predicates.Count; i++)
        {
            body = Expression.OrElse(body, Expression.Invoke(predicates[i], param));
        }
        var lambda = Expression.Lambda<Func<T, bool>>(body, param);
        return lambda;
    }
}