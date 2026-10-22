using System.Text.Json.Serialization;

namespace Eventual.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter<OrderStatus>))]
public enum OrderStatus
{
    Pending,
    Paid,
    Declined,
    Expired
};
