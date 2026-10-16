namespace eventual.Models;

public class OrderStatus
{
    public enum Status
    {
        Pendent, 
        Purchased, 
        Block,
        Expired
    };
}