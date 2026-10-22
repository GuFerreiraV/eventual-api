using Eventual.Api.Models.Dtos;
using Eventual.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eventual.Api.Controllers;

[ApiController]
[Tags("Lotes")]
[Route("lotes")]
public class LotesController(ILoteService loteService) : ControllerBase
{
    /// <summary>Consulta um lote isolado, com as vagas disponíveis.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<LoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        => Ok(await loteService.GetAsync(id, cancellationToken));
}
