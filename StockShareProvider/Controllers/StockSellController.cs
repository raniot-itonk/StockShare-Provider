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
        public async Task<ActionResult<ValidationResult>> SetSharesForSale(SellRequestModel sellRequestModel)
        {
            // Validate Ownership

            var stock = await _publicShareOwnerControlClient.GetStock(sellRequestModel.StockId, "jwtToken");
            var shareholder = stock.ShareHolders.FirstOrDefault(sh => sh.ShareholderId == sellRequestModel.AccountId);
            if (shareholder == null)
            {
                _logger.LogError("ShareHolder with id {id} Not Found for stock {stockId}", sellRequestModel.AccountId, sellRequestModel.StockId);
                return new ValidationResult { Valid = false, ErrorMessage = $"ShareHolder with id {sellRequestModel.AccountId} Not Found for stock {sellRequestModel.StockId}" };
            }

            var sellRequestModels = await _stockTraderBrokerClient.GetSellRequests(sellRequestModel.AccountId, sellRequestModel.StockId, "jwtToken");
            var sharesAlreadyForSale = sellRequestModels.Sum(model => model.AmountOfShares);

            _logger.LogInformation("ShareHolder requests {requestAmount}, and he currently owns {amount} and already have {sharesAlreadyForSale} for sale", sellRequestModel.AmountOfShares, shareholder.Amount, sharesAlreadyForSale);
            if (shareholder.Amount < sellRequestModel.AmountOfShares + sharesAlreadyForSale)
            {
                return new ValidationResult { Valid = false, ErrorMessage = $"ShareHolder requests {sellRequestModel.AmountOfShares}, but he only has {shareholder.Amount}, and already {sharesAlreadyForSale} for sale" };
            }

            // Set Shares for Sale
            await _stockTraderBrokerClient.PostSellRequest(sellRequestModel, "jwtToken");

            _logger.LogInformation("Successfully set {Amount} shares of stock with Id {stockId} for sale", sellRequestModel.AmountOfShares, sellRequestModel.StockId);
            
            return new ValidationResult{Valid = true, ErrorMessage = ""};
        }
    }
}
