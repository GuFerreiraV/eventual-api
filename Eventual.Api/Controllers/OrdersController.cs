using Eventual.Api.Models.Dtos;
using Eventual.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eventual.Api.Controllers;

[ApiController]
[Tags("Pedidos")]
[Route("orders")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    /// <summary>Reserva ingressos de um lote (máx. 5). O pedido nasce Pending e expira em 15 minutos.</summary>
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

    /// <summary>Paga o pedido e gera os ingressos. Tokens de teste: tok_approved, tok_declined.</summary>
    [HttpPost("{id:int}/payment")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Pay(int id, PayOrderRequest request, CancellationToken cancellationToken)
        => Ok(await orderService.PayAsync(id, request, cancellationToken));

    /// <summary>Consulta um pedido e seus ingressos. Um pedido pendente vencido aparece como Expired.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        => Ok(await orderService.GetAsync(id, cancellationToken));
}
