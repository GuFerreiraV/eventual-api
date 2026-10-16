namespace eventual.Models;

public class Ticket
{
    public int Id {get; set;}
    public int OrderId {get; set;}
    public Order Order {get; set;} = null!;
    public Guid Code {get; set;} = Guid.NewGuid(); // o que vai dentro do qrcode
    public DateTime? CheckedInAt {get; set;}
}