using Eventual.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Eventual.Api.Handlers;

public class DomainExceptionHandler(
    ILogger<DomainExceptionHandler> logger,
    IProblemDetailsService problemDetailsService
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = new ProblemDetails();

        if (exception is DomainException)
        {
            // A categoria do domínio define o status; a mais específica vem primeiro.
            problem.Status = exception switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ConflictException => StatusCodes.Status409Conflict,
                PaymentException => StatusCodes.Status402PaymentRequired,
                _ => StatusCodes.Status400BadRequest
            };
            problem.Title = "Não foi possível concluir a operação.";
            problem.Detail = exception.Message;
        }
        else
        {
            logger.LogError(exception, "Exceção não tratada: {ExceptionType}", exception.GetType().Name);

            problem.Status = StatusCodes.Status500InternalServerError;
            problem.Title = "Erro interno no servidor.";
            problem.Detail = "Ocorreu um erro inesperado. Tente novamente mais tarde.";
        }

        httpContext.Response.StatusCode = problem.Status.Value;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}
