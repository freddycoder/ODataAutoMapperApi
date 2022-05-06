using AutoMapper;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser;

namespace ODataAutomapperApi.Attributes;

public class AutoMapperQueryEnabledAttribute : TakeOneElementEachEnableQueryAttribute
{
    private IMapper? _mapper;
    private Type _source;
    private Type _destination;

    public AutoMapperQueryEnabledAttribute(Type source, Type destination)
    {
        _source = source;
        _destination = destination;
    }

    public override void OnActionExecuting(ActionExecutingContext actionExecutingContext)
    {
        _mapper = actionExecutingContext.HttpContext.RequestServices.GetRequiredService<IMapper>();

        base.OnActionExecuting(actionExecutingContext);
    }

    public override IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
    {
        if (_mapper == null)
        {
            throw new InvalidOperationException("_mapper must be initialized by calling OnActionExecuting before calling ApplyQuery");
        }

        var elements = base.ApplyQuery(queryable, queryOptions).Cast<object>().ToArray();

        if (elements.Length > 0 && elements[0].GetType().Equals(_source))
        {
            return ((object[])_mapper.Map(elements, elements.GetType(), _destination)).AsQueryable();
        }

        return elements.AsQueryable();
    }

    public override void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
    {
        return;
    }
}
