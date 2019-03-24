using System;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using StockShareProvider.Helpers;
using StockShareProvider.Models;
using StockShareProvider.OptionModels;

namespace StockShareProvider.Clients
{
    public interface IStockTraderBrokerClient
    {
        Task PostSellRequest(SellRequestInput request, string jwtToken);
    }

    public class StockTraderBrokerClient : IStockTraderBrokerClient
    {
        private readonly StockTraderBroker _publicShareOwnerControl;

        public StockTraderBrokerClient(IOptionsMonitor<Services> serviceOption)
        {
            _publicShareOwnerControl = serviceOption.CurrentValue.StockTraderBroker ??
                           throw new ArgumentNullException(nameof(serviceOption.CurrentValue.StockTraderBroker));
        }
        public async Task PostSellRequest(SellRequestInput request, string jwtToken)
        {
            await PolicyHelper.ThreeRetriesAsync().ExecuteAsync(() =>
                _publicShareOwnerControl.BaseAddress
                    .AppendPathSegment(_publicShareOwnerControl.StockTraderBrokerPath.SellRequest)
                    .WithOAuthBearerToken(jwtToken).PostJsonAsync(request));
        }
    }
}
