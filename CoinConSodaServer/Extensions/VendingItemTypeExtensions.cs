using CoinConSodaServer.Models;

namespace CoinConSodaServer.Extensions
{
    public static class VendingItemTypeExtensions
    {
        //sets the price for items, obviously would be data driven but demo
        public static double Price(this VendingItemType itemType)
        {
            double price = 0;
            switch (itemType)
            {
                case VendingItemType.Coke:
                    {
                        price = 1.80;
                        break;
                    }
                case VendingItemType.Juice:
                    {
                        price = 2.20;
                        break;
                    }
                case VendingItemType.ChocolateBar:
                    {
                        price = 3.00;
                        break;
                    }
                default: break;
            }
            return price;
        }
    }
}
