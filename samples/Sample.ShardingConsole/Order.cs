namespace Sample.ShardingConsole;


/// <summary>
/// order table
/// </summary>
public class Order
{
    /// <summary>
    /// order Id
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// payer id
    /// </summary>
    public string Payer { get; set; }
    /// <summary>
    /// pay money cent
    /// </summary>
    public long Money { get; set; }
    /// <summary>
    /// area
    /// </summary>
    public string Area { get; set; }
    /// <summary>
    /// order status
    /// </summary>
    public OrderStatusEnum OrderStatus { get; set; }
    /// <summary>
    /// CreationTime
    /// </summary>
    public DateTime CreationTime { get; set; }
}
public enum OrderStatusEnum
{
    NoPay=1,
    Paying=2,
    Payed=3,
    PayFail=4
}