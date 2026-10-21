using eventual.Models.Dtos;
using eventual.Services;
using Microsoft.AspNetCore.Mvc;

namespace eventual.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost("~/lotes/{loteId:int}/orders")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(int loteId, CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var created = await orderService.CreateAsync(loteId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPost("{id:int}/payment")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Pay(int id, PayOrderRequest request, CancellationToken cancellationToken)
        => Ok(await orderService.PayAsync(id, request, cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        => Ok(await orderService.GetAsync(id, cancellationToken));
}
