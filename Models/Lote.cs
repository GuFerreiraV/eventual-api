using System.ComponentModel.DataAnnotations;

namespace eventual.Models;

public class Lote
{
    public int Id {get; set;}
    public int EventId {get; set;}
    public Event Event {get; set;} = null!;
    public string Name {get; set;} = string.Empty;
    public decimal Price {get; set;}
    public int Quantity {get; set;}
    [ConcurrencyCheck]
    public Guid Version {get; set;} = Guid.NewGuid();
    public List<Order> Orders {get; set;} = [];
}