using System.ComponentModel.DataAnnotations;

namespace CoinConSodaServer.Models
{
    public class Coins
    {
        public CoinType CoinType { get; set; }
        public int Count { get; set; }

        public int CoinsValue() {
            return (Count * (int)CoinType);
        }
    }
}
