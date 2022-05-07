using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OData.Query;
using ODataAutomapperApi.Attributes.Contexts;

namespace ODataAutomapperApi.Attributes;

public class TakeOneElementEachEnableQueryAttribute : EnableQueryAttribute
{
    public override void OnActionExecuting(ActionExecutingContext actionExecutingContext)
    {
        if (actionExecutingContext.HttpContext.Request.Query.TryGetValue("$takeOneElementEach", out var takeOneElementEach))
        {
            if (int.TryParse(takeOneElementEach, out var number))
            {
                actionExecutingContext.HttpContext.RequestServices.GetRequiredService<TakeOneElementEachContext>().Number = number;
            }
        }

        base.OnActionExecuting(actionExecutingContext);
    }

    public override Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        return base.OnResultExecutionAsync(context, next);
    }

    public override IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
    {
        var query = base.ApplyQuery(queryable, queryOptions);

        var context = queryOptions.Request.HttpContext.RequestServices.GetRequiredService<TakeOneElementEachContext>();

        if (context.Number.HasValue)
        {
            var array = query.GetEnumerator();

            var list = new List<object>();

            for (int i = 0; array.MoveNext(); i++)
            {
                if (i % context.Number.Value == 0)
                {
                    list.Add(array.Current);
                }
            }

            return list.AsQueryable();
        }

        return query;
    }

    public override void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
    {
        if (request.HttpContext.RequestServices.GetRequiredService<TakeOneElementEachContext>().Number.HasValue)
        {
            return;
        }

        base.ValidateQuery(request, queryOptions);
    }
}