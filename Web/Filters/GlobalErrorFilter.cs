using ApplicationCore.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Linq;

namespace Web.Filters
{
    public class GlobalErrorFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is ValidationException ve)
            {
                this.HandleValidationException(context, ve);
                return;
            }
            if (context.Exception is NotFoundException nfe)
            {
                this.HandleNotFoundException(context, nfe);
            }
            if (context.Exception is BadRequestException bre)
            {
                this.HandleBadRequestException(context, bre);
            }
            if (context.Exception is UnAuthorizedException uae)
            {
                this.HandleUnAuthorizedExeption(context, uae);
            }
            else
            {
                var problemDetails = this.GetProblemDeatils(context, StatusCodes.Status500InternalServerError);
                problemDetails.Errors.Add("unexpected", new string[] { context.Exception.Message });
            }
        }

        private void HandleValidationException(
            ExceptionContext context,
            ValidationException e)
        {
            var problemDetails = this.GetProblemDeatils(context, StatusCodes.Status400BadRequest);

            e.Errors
                .ToList()
                .ForEach(err =>
                {
                    var errorList = new List<string>();
                    if (!problemDetails.Errors.ContainsKey(err.PropertyName))
                    {
                        problemDetails.Errors.Add(
                            err.PropertyName,
                            new string[] { });
                    }
                    errorList = problemDetails.Errors[err.PropertyName].ToList();
                    errorList.Add(err.ErrorMessage);
                    problemDetails.Errors[err.PropertyName] = errorList.Distinct().ToArray();
                });
        }

        private void HandleNotFoundException(ExceptionContext context, NotFoundException e)
        {
            var problemDetails = this.GetProblemDeatils(context, StatusCodes.Status404NotFound);
            problemDetails.Errors.Add(e.Id, new string[] { e.Message });
        }

        private void HandleBadRequestException(ExceptionContext context, BadRequestException bre)
        {
            var problemDetails = this.GetProblemDeatils(context, StatusCodes.Status400BadRequest);
            bre.Errors.ToList().ForEach(e => problemDetails.Errors.Add(e.Item1, new string[] { e.Item2 }));
        }

        private void HandleUnAuthorizedExeption(ExceptionContext context, UnAuthorizedException uae)
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Instance = context.HttpContext.Request.Path,
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://asp.net/core",
                Detail = "Please refer to the errors property for additional details.",
            };
            context.Result = new UnauthorizedObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json", "application/problem+xml" }
            };
            context.ExceptionHandled = true;
        }

        private ValidationProblemDetails GetProblemDeatils(ExceptionContext context, int status)
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Instance = context.HttpContext.Request.Path,
                Status = status,
                Type = "https://asp.net/core",
                Detail = "Please refer to the errors property for additional details.",
            };
            context.Result = new BadRequestObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json", "application/problem+xml" }
            };
            context.ExceptionHandled = true;

            return problemDetails;
        }
    }
}
