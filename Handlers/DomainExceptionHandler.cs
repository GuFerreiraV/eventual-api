using eventual.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace eventual.Handlers;

public class DomainExceptionHandler(
    ILogger<DomainExceptionHandler> logger,
    IProblemDetailsService problemDetailsService
) : IExceptionHandler
{
    private static readonly Dictionary<Type, int> StatusByException = new()
    {
        [typeof(LoteSoldOutException)] = StatusCodes.Status409Conflict,
        [typeof(EventNotFoundException)] = StatusCodes.Status404NotFound,
        [typeof(TicketAlreadyUsedException)] = StatusCodes.Status409Conflict
    };

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = new ProblemDetails();

        if (exception is DomainException
            && StatusByException.TryGetValue(exception.GetType(), out var status))
        {
            problem.Status = status;
            problem.Title = "Não foi possível concluir a operação.";
            problem.Detail = exception.Message;
        }
        else
        {
            logger.LogError(
                exception,
                "Exceção não mapeada capturada no Handler: {ExceptionType} - Mensagem: {Message}",
                exception.GetType().Name,
                exception.Message);

            problem.Status = StatusCodes.Status500InternalServerError;
            problem.Title = "Erro interno no servidor";
            problem.Detail = "Ocorreu um erro inesperado. Tente novamente mais tarde.";
        }

        httpContext.Response.StatusCode = problem.Status!.Value;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}