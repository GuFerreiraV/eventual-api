using eventual.Models.Dtos;
using eventual.Services;
using Microsoft.AspNetCore.Mvc;

namespace eventual.Controllers;

[ApiController]
[Route("lotes")]
public class LotesController(ILoteService loteService) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType<LoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        => Ok(await loteService.GetAsync(id, cancellationToken));
}
