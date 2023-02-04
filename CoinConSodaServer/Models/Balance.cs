using CoinConSodaServer.Extensions;
using Newtonsoft.Json.Linq;

namespace CoinConSodaServer.Models
{
    public class Balance
    {
        //collection of coins and counts, as we're tracking all that stuff
        public List<Coins> AllCoins { get; set; }        

        //how many virtual dollars does our customer have invested in our tasty snack scheme
        public double GetBallance () {            
            return (double)(AllCoins?.Sum(coins => coins.CoinsValue()) ?? 0) / 100;            
        }

        public Balance() {
            AllCoins = new();
        }
        public void ClearCoins() {
            AllCoins?.Clear();
        }
        public void AddCoins(Coins newCoin)
        {
            //find our coinf record of this type
            Coins? coin = AllCoins?.Where(c => c.CoinType == newCoin.CoinType).FirstOrDefault();
            if (coin != null)
            {
                //and add the new coins
                coin.Count += newCoin.Count;
            }
            else {
                //or a new record because we didnt have those before
                AllCoins?.Add(newCoin);
            }
        }
        public void RemoveCoins(Coins goneCoins)
        {
            Coins coin = AllCoins.Where(c => c.CoinType == goneCoins.CoinType).First();
            //going to assume we dont go negative here
            if (coin != null)
            {
                coin.Count -= goneCoins.Count;
            }
            else { 
                //oh no we didnt have those coins!?
            }

        }

        //do we have any smaller coins than this type?
        public bool HaveSmallerCoinsThanType(CoinType type) {
            CoinType? previousType = type.Prev();
            while (previousType != null)
            {
                Coins? coin = AllCoins?.Where(c => c.CoinType == previousType).FirstOrDefault();
                if ((coin != null) && (coin.Count > 0)) {
                    return true;
                }
                previousType = ((CoinType)previousType).Prev();
            }
            return false;
        }
    }
}
