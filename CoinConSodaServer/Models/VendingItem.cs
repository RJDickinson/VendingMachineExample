using System.ComponentModel.DataAnnotations;

namespace CoinConSodaServer.Models
{
    // our sugary product stock
    public class VendingItem
    {
        public VendingItemType Type { get; set; }
        public int Quantity { get; set;}
    }
}
