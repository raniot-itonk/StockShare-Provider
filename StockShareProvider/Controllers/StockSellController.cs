using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StockShareProvider.Clients;
using StockShareProvider.Models;

namespace StockShareProvider.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockSellController : ControllerBase
    {
        private readonly ILogger<StockSellController> _logger;
        private readonly IPublicShareOwnerControlClient _publicShareOwnerControlClient;
        private readonly IStockTraderBrokerClient _stockTraderBrokerClient;

        public StockSellController(ILogger<StockSellController> logger, IStockTraderBrokerClient stockTraderBrokerClient, IPublicShareOwnerControlClient publicShareOwnerControlClient)
        {
            _logger = logger;
            _publicShareOwnerControlClient = publicShareOwnerControlClient;
            _stockTraderBrokerClient = stockTraderBrokerClient;
        }

        [HttpPost]
        public async Task<ActionResult> SetSharesForSale(SellRequestInput sellRequestInput)
        {
            // Validate Ownership
            try
            {
                var stock = await _publicShareOwnerControlClient.GetStock(sellRequestInput.StockId, "jwtToken");
                var shareholder = stock.ShareHolders.FirstOrDefault(sh => sh.ShareholderId == sellRequestInput.AccountId);
                if (shareholder == null)
                {
                    _logger.LogError("ShareHolder with id {id} Not Found for stock {stockId}", sellRequestInput.AccountId, sellRequestInput.StockId);
                    return BadRequest($"ShareHolder with id {sellRequestInput.AccountId} Not Found for stock {sellRequestInput.StockId}");
                }
                if (shareholder.Amount < sellRequestInput.AmountOfShares)
                {
                    _logger.LogInformation("ShareHolder requests {requestAmount}, but he only has {amount}", sellRequestInput.AmountOfShares, shareholder.Amount);
                    return BadRequest($"ShareHolder requests {sellRequestInput.AmountOfShares}, but he only has {shareholder.Amount}");
                }
                //Maybe add reserve logic cause right now this is a bit broken, since you can put your shares for sale unlimited times

                // Set Shares for Sale
                await _stockTraderBrokerClient.PostSellRequest(sellRequestInput, "jwtToken");

                _logger.LogInformation("Successfully set {Amount} shares of stock with Id {stockId} for sale", sellRequestInput.AmountOfShares, sellRequestInput.StockId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            return Ok();
        }
    }
}
