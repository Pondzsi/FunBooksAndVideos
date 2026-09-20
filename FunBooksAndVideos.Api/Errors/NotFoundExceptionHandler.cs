using FunBooksAndVideos.Application.Common;
using FunBooksAndVideos.Application.Customers;
using FunBooksAndVideos.Application.Products;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FunBooksAndVideos.Api.Errors;

// A customer or product named in a request body that does not exist is a 422: the request is well formed
// but cannot be processed. A missing resource in the URL is a 404, which the controllers return themselves.
internal sealed class NotFoundExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public NotFoundExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException notFound)
        {
            return false;
        }

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.21",
            Title = "The request refers to something that does not exist.",
            Detail = notFound.Message,
        };

        switch (notFound)
        {
            case CustomerNotFoundException customer:
                problem.Extensions["customerId"] = customer.CustomerId;
                break;
            case ProductNotFoundException product:
                problem.Extensions["productId"] = product.ProductId;
                break;
        }

        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem,
        });
    }
}
