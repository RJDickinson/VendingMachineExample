using CoinConSodaServer.Extensions;
using CoinConSodaServer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CoinConSodaServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendingController : ControllerBase
    {
        private readonly ILogger<VendingController> _logger;
        private static VendingMachine? VendMachine { get; set; }

        //probably doing no logging but 
        public VendingController(ILogger<VendingController> logger)
        {
            _logger = logger;
        }

        [HttpPost("Restock/")]
        public string Restock()
        {
            //just a singleton vending machine for the demo
            VendingMachine VendMachine = GetVendingMachine();
            double cashOut = VendMachine.Restock();
            // are we doing culture stuff in here? Nah probably not for a demo
            FormattableString message = $"The takings for this amazing vending machine are {cashOut:C2}";
            return message.ToString();
        }

        // POST: api/AddCoin
        [HttpPost("AddCoin/")]
        public string AddCoin([FromBody] Coins aCoin)
        {
            // some validation here would be good..
            VendingMachine VendMachine = GetVendingMachine();
            double customerBallance = VendMachine.AddCustomerCoin(aCoin);
            // Logic to create new Employee
            FormattableString message = $"Your balance is {customerBallance:C2}";
            return message.ToString();
        }

        // POST: api/Refund
        [HttpPost("Refund/")]
        public string Refund()
        {
            // some validation here would be good..
            VendingMachine VendMachine = GetVendingMachine();
            double customerBallance = VendMachine.CancelSale();
            // Logic to create new Employee
            FormattableString message = $"Your refund is {customerBallance:C2}";
            return message.ToString();
        }

        // POST: api/BuySnack
        [HttpPost("BuySnack/")]
        public string BuySnack(VendingItemType item)
        {            
            VendingMachine VendMachine = GetVendingMachine();
            string message = VendMachine.ProcessSale(item);
            return message;
        }

        private static VendingMachine GetVendingMachine()
        {
            if (VendMachine == null) { 
                VendMachine = new VendingMachine();
            }
            return VendMachine;
        }
    }
}
