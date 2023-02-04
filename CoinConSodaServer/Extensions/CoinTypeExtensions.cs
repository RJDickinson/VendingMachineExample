using CoinConSodaServer.Models;
using System;

namespace CoinConSodaServer.Extensions
{
    public static class CoinTypeExtensions
    {
        // find the next or pr3evious sized coin type - could be clever with this but this is quicker.
        public static CoinType? Next(this CoinType myEnum)
        {
            switch (myEnum)
            {
                case CoinType.TenCents:
                    return CoinType.TwentyCents;
                case CoinType.TwentyCents:
                    return CoinType.FiftyCents;
                case CoinType.FiftyCents:
                    return CoinType.OneDollar;
                case CoinType.OneDollar:
                    return CoinType.TwoDollar;
                default:
                    return null;
            }
        }
        public static CoinType? Prev(this CoinType myEnum)
        {
            switch (myEnum)
            {
                case CoinType.TwoDollar:
                    return CoinType.OneDollar;
                case CoinType.OneDollar:
                    return CoinType.FiftyCents;
                case CoinType.FiftyCents:
                    return CoinType.TwentyCents;
                case CoinType.TwentyCents:
                    return CoinType.TenCents;
                default:
                    return null;
            }
        }
    }
}
