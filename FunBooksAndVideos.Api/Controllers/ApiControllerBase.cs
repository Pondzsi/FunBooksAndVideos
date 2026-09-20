using Microsoft.AspNetCore.Mvc;

namespace FunBooksAndVideos.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    // A missing resource in the URL is a 404 with a Problem Details body, like every other error.
    protected ObjectResult NotFoundProblem(string title, string detail)
    {
        return Problem(detail: detail, statusCode: StatusCodes.Status404NotFound, title: title);
    }
}
