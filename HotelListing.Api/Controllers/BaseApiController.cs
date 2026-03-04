using HotelListing.Api.Common.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using HotelListing.Api.Common.Result;


namespace HotelListing.Api.Controllers;

public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<T> ToActionResult<T>(Result<T> result)
        => result.IsSuccess ? Ok(result.Value) : MapErrorsToResponse(result.Errors);

    protected ActionResult ToActionResult(Result result)
        => result.IsSuccess ? NoContent() : MapErrorsToResponse(result.Errors);

    protected ActionResult MapErrorsToResponse(Error[] errors)
    {
        if (errors is null || errors.Length == 0) 
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An Error Occured!",
                detail: "No detail provided"
                );
        }
        

        var e = errors[0];
        var errorDetails = string.Join("; ", errors.Select(x => x.Description));

        return e.Code switch
        {
            ErrorCodes.NotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource not found",
                detail: errorDetails
                ),
            ErrorCodes.Validation => ValidationProblem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation failed",
                detail: errorDetails
                ),
            ErrorCodes.BadRequest => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad request",
                detail: errorDetails
                ),
            ErrorCodes.Conflict => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict found",
                detail: errorDetails
                ),
            ErrorCodes.Unauthorized => Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title:"User unauthorized",
                detail: errorDetails
                ),
            _ => Problem(detail: string.Join("; ", errors.Select(x => x.Description)), title: e.Code),
        };
    }
}