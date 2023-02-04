using System.ComponentModel;

namespace CoinConSodaServer.Models
{
    //the kinds of sugary things we are selling and nice descriptions
    public enum VendingItemType
    {
        [Description("Coke")]
        Coke,
        [Description("Juice")]
        Juice,
        [Description("Chocolate Bar")]
        ChocolateBar
    }
}
