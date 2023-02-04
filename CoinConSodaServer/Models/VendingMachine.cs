using CoinConSodaServer.Extensions;
using System;
using System.Runtime.CompilerServices;

namespace CoinConSodaServer.Models
{
    public class VendingMachine
    {
        private const int RestockCount = 10;
        // Id for multiple vending machines - probably not in this demo
        //public string Id { get; set; }

        //our stock list
        public List<VendingItem> AllPops { get; set; }
        //our customers coin collection
        private Balance CustomerBallance { get; set; }
        //ou machines coin collection 
        private Balance MachineBallance { get; set; }

        private bool DoingChange { get; set; }
        public VendingMachine() {
            AllPops = new List<VendingItem>();
            CustomerBallance= new Balance();
            MachineBallance = new Balance();
            Restock();
        }

        // restock does the following
        // restock all items to 'full;
        // collects sales money
        // reinitiallises change bank (30 x 10c, 15 x 20c)
        public double Restock() {

            double SalesBalance = 0;

            //ok clear and reset the vending items
            AllPops.Clear();
            foreach (VendingItemType popType in (VendingItemType[])Enum.GetValues(typeof(VendingItemType)))
            {
                AllPops.Add(new VendingItem { Type= popType , Quantity = RestockCount});
            }

            //what to do with the CustomerBalance if its not zero? ignore for now they can get a refund

            //now does 'sales money' mean what we've taken minus the float(init money) probably? =            
            Balance NewChangeBalance = NewChangeBank();
            if (MachineBallance.GetBallance() > NewChangeBalance.GetBallance())
            {
                //if not someone has stolen our coins
                SalesBalance = MachineBallance.GetBallance() - NewChangeBalance.GetBallance();
            }
            MachineBallance = NewChangeBalance;
            return SalesBalance;
        }

        public double CancelSale() { 
            double Refund = CustomerBallance.GetBallance();
            CustomerBallance.ClearCoins();
            return Refund;
        }

        public double AddCustomerCoin(Coins newCoin) {
            CustomerBallance.AddCoins(newCoin);
            return CustomerBallance.GetBallance();
        }

        public string ProcessSale(VendingItemType SaleItem) {
            //ok first check if the customer has enough balance to buy this at all lets step through it
            double HowMuchIsTheSnack = SaleItem.Price();
            double PenniesDeposited = CustomerBallance.GetBallance();

            FormattableString? message;
            //do we have one to sell?
            if (!IsInStock(SaleItem))
            {
                //out of stock!!
                if (AnyStock())
                {
                    message = $"Your tasty snack of choice {SaleItem.GetDescription()} is no longer availabe please select from one of the other tasty snakc choices or select a refund";
                    return message.ToString();
                }
                message = $"Our tasty snacks have proven more popular than expected, your request is in a queue but no one is looking at said queue, please select refund for your money({PenniesDeposited:C2}) back, than you for trying";
                return message.ToString();
            }

            if (HowMuchIsTheSnack > PenniesDeposited ) {
                //oh noes the thing is too expensive.
                //return a deposit more coins message 
                message = $"Your balance of {PenniesDeposited:C2} is insufficient for snack type {SaleItem.GetDescription()} we need an additional {(HowMuchIsTheSnack - PenniesDeposited):C2} deposited";
                return message.ToString();
            }

            //ok we have enough for the sugary snack, but we need to deal with coins.
            if (HowMuchIsTheSnack != PenniesDeposited)
            {
                int price = (int)(SaleItem.Price() * 100);
                DoingChange = false;
                //lets do this on a copy because we might hit change issues
                Balance custCopy = CustomerBallance.DeepCopy();
                Balance MachineCopy = MachineBallance.DeepCopy();
                if (!TransferFunds(custCopy, MachineCopy, price)){
                    message = $"Sadly we have insufficient change for this transation please refund and use smaller coins";
                    return message.ToString();
                }
                //happy times 
                CustomerBallance = custCopy;
                MachineBallance = MachineCopy;
            }
            else {
                //customer has exactly the right change so we dont need to do complex stuff
                foreach (Coins aCoin in CustomerBallance.AllCoins) {
                    PayCoins(CustomerBallance, MachineBallance, aCoin.CoinType, aCoin.Count);  
                }
            }
            //reduce stock levels
            SellItem(SaleItem); // - probably need to check that for an error one day... 

            PenniesDeposited = CustomerBallance.GetBallance();
            if (PenniesDeposited > 0) {
                message = $"Enjoy your tasty {SaleItem.GetDescription()}, you have a remaining balance of {PenniesDeposited:C2}";
            }
            else {
                message = $"Enjoy your tasty {SaleItem.GetDescription()}";
            }
            return message.ToString();

        }

        //do we have sugary things to sell
        private bool AnyStock()
        {
            int stock = (int)(AllPops.Sum(vt => vt.Quantity));
            return stock > 0;
        }
        //do we have this sugary thing to sell
        private bool IsInStock(VendingItemType type)
        {
            VendingItem? SelectedPop = AllPops.FirstOrDefault(vt => vt.Type == type);
            if (SelectedPop != null)
            {
                return !(SelectedPop.Quantity == 0);
            }
            return false;
        }

        private static Balance NewChangeBank() {
            Balance NewChangeBank = new();
            NewChangeBank.AddCoins(new Coins { CoinType = CoinType.TenCents, Count = 30 });
            NewChangeBank.AddCoins(new Coins { CoinType = CoinType.TwentyCents, Count = 10 });
            return NewChangeBank;
        }

        private bool SellItem(VendingItemType type)
        {
            VendingItem? SelectedPop = AllPops.FirstOrDefault(vt => vt.Type == type);
            if (SelectedPop != null)
            {
                if(SelectedPop.Quantity == 0) {
                    return false;
                }
                SelectedPop.Quantity--;
                return true;
            }
            return false;
        }

        private static void PayCoins(Balance fromBalance, Balance toBalance, CoinType? type, int count)
        {
            if (type == null) return;
            //assume we have these coins 
            Coins fromCoins = fromBalance.AllCoins.Where(c => c.CoinType == type).First();
            //but we may not have this
            Coins? toCoins = toBalance.AllCoins.Where(c => c.CoinType == type).FirstOrDefault();
            fromCoins.Count -= count;
            if (toCoins != null)
            {
                toCoins.Count += count;
            }
            else {
                toBalance.AllCoins.Add(new Coins { CoinType = (CoinType)type, Count = count });
            }            
        }

        private bool TransferFunds(Balance fromBalance, Balance toBalance, int price)
        {
            // we'll start paying with our largest coins
            int remainingPrice = price;            
            CoinType? current = CoinType.TwoDollar;
            do
            {
                Coins? theseCoins = fromBalance.AllCoins.Where(c => c.CoinType == current).FirstOrDefault();
                if (theseCoins != null)
                {
                    // do we have the exact match or enough to cover the whole value 
                    if ((theseCoins.CoinsValue() == remainingPrice) || ((remainingPrice % (int)current) == 0))
                    {
                        // top match we're done
                        int coinsToTransfer = remainingPrice / (int)current;
                        PayCoins(fromBalance, toBalance, current, coinsToTransfer);
                        return true;
                    }
                    //not enough of these, sue them and drop the coin value
                    if (theseCoins.CoinsValue() < remainingPrice)
                    {
                        remainingPrice -= theseCoins.CoinsValue();
                        PayCoins(fromBalance, toBalance, current, theseCoins.Count);
                        current = ((CoinType)current).Prev();
                        continue;
                    }

                    // ok we have 'too much money' in these coins, and not an exact divisible match, do we have smaller coins than this type?
                    bool haveSmaller = fromBalance.HaveSmallerCoinsThanType((CoinType)current);
                    int wholeCoins = 0;                    
                    if (haveSmaller) 
                    {
                        // in which case try using smaller coins on the remainder
                        wholeCoins = remainingPrice / (int)current;
                        PayCoins(fromBalance, toBalance, current, wholeCoins);
                        remainingPrice -= wholeCoins * (int)current;
                        current = ((CoinType)current).Prev();
                        continue;
                    }

                    //dont have smaller so need 1 more and change
                    wholeCoins = (remainingPrice / (int)current) + 1;
                    int changeRequired = (wholeCoins * (int)current) - remainingPrice;
                    remainingPrice = 0; // paid it all plus more at this point
                    PayCoins(fromBalance, toBalance, current, wholeCoins);
                    if (!DoingChange)
                    {                        
                        DoingChange = true;
                        return TransferFunds(toBalance, fromBalance, changeRequired);                        
                    }
                    break;
                }
                current = ((CoinType)current).Prev();
            } while (current != null);

            return (remainingPrice == 0);
        }
    }
}
