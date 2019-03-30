using System;
using System.Collections.Generic;
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
        Task PostSellRequest(SellRequestModel request, string jwtToken);
        Task<List<SellRequestModel>> GetSellRequests(Guid ownerId, long stockId, string jwtToken);
    }

    public class StockTraderBrokerClient : IStockTraderBrokerClient
    {
        private readonly StockTraderBroker _stockTraderBroker;

        public StockTraderBrokerClient(IOptionsMonitor<Services> serviceOption)
        {
            _stockTraderBroker = serviceOption.CurrentValue.StockTraderBroker ??
                           throw new ArgumentNullException(nameof(serviceOption.CurrentValue.StockTraderBroker));
        }
        public async Task PostSellRequest(SellRequestModel request, string jwtToken)
        {
            await PolicyHelper.ThreeRetriesAsync().ExecuteAsync(() =>
                _stockTraderBroker.BaseAddress
                    .AppendPathSegment(_stockTraderBroker.StockTraderBrokerPath.SellRequest)
                    .WithOAuthBearerToken(jwtToken).PostJsonAsync(request));
        }

        public async Task<List<SellRequestModel>> GetSellRequests(Guid ownerId, long stockId, string jwtToken)
        {
            return await PolicyHelper.ThreeRetriesAsync().ExecuteAsync(() =>
                _stockTraderBroker.BaseAddress
                    .AppendPathSegment(_stockTraderBroker.StockTraderBrokerPath.SellRequest).SetQueryParams(new {ownerId, stockId})
                    .WithOAuthBearerToken(jwtToken).GetJsonAsync<List<SellRequestModel>>());
        }
    }
}
