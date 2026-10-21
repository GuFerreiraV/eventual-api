using System.Text.Json.Serialization;

namespace eventual.Models;

[JsonConverter(typeof(JsonStringEnumConverter<OrderStatus>))]
public enum OrderStatus
{
    Pending,
    Paid,
    Declined,
    Expired
};
