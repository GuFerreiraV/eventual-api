using System.ComponentModel.DataAnnotations;

namespace Eventual.Api.Models;

public class Ticket
{
    public int Id {get; set;}
    public int OrderId {get; set;}
    public Order Order {get; set;} = null!;
    public Guid Code {get; set;} = Guid.NewGuid(); // o que vai dentro do qrcode
    public DateTime? CheckedInAt {get; set;}
    [ConcurrencyCheck]
    public Guid Version {get; set;} = Guid.NewGuid(); // trocada no check-in: impede duas entradas com o mesmo QR
}
