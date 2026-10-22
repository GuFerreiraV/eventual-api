using System.ComponentModel.DataAnnotations;

namespace Eventual.Api.Models;

public class Order {
    public int Id {get; set;}
    public int LoteId {get; set;}
    public int Quantity {get; set;}
    public Lote Lote {get; set;} = null!;
    public decimal TotalAmount {get; set;}
    public string BuyerName {get; set;} = string.Empty;  // owner do pedido
    public string BuyerEmail {get; set;} = string.Empty;  // owner do pedido
    public OrderStatus Status {get; set;} = OrderStatus.Pending;
    public DateTime CreatedAt {get; set;}
    public DateTime ExpiresAt {get; set;}
    [ConcurrencyCheck]
    public Guid Version {get; set;} = Guid.NewGuid(); // trocada ao pagar: impede pagamento duplo
    public List<Ticket> Tickets {get; set;} = [];
}