﻿using System;
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
        Task<ValidationResult> PostSellRequest(SellRequestModel request, string jwtToken);
        Task<List<SellRequestModel>> GetSellRequests(Guid ownerId, long stockId, string jwtToken);
        Task<ValidationResult> RemoveSellRequest(long id, string jwtToken);
    }

    public class StockTraderBrokerClient : IStockTraderBrokerClient
    {
        private readonly StockTraderBroker _stockTraderBroker;

        public StockTraderBrokerClient(IOptionsMonitor<Services> serviceOption)
        {
            _stockTraderBroker = serviceOption.CurrentValue.StockTraderBroker ??
                           throw new ArgumentNullException(nameof(serviceOption.CurrentValue.StockTraderBroker));
        }
        public async Task<ValidationResult> PostSellRequest(SellRequestModel request, string jwtToken)
        {
            return await PolicyHelper.ThreeRetriesAsync().ExecuteAsync(() =>
                _stockTraderBroker.BaseAddress
                    .AppendPathSegment(_stockTraderBroker.StockTraderBrokerPath.SellRequest)
                    .WithOAuthBearerToken(jwtToken).PostJsonAsync(request).ReceiveJson<ValidationResult>());
        }

        public async Task<List<SellRequestModel>> GetSellRequests(Guid ownerId, long stockId, string jwtToken)
        {
            return await PolicyHelper.ThreeRetriesAsync().ExecuteAsync(() =>
                _stockTraderBroker.BaseAddress
                    .AppendPathSegment(_stockTraderBroker.StockTraderBrokerPath.SellRequest).SetQueryParams(new {ownerId, stockId})
                    .WithOAuthBearerToken(jwtToken).GetJsonAsync<List<SellRequestModel>>());
        }

        public async Task<ValidationResult> RemoveSellRequest(long id, string jwtToken)
        {
            return await PolicyHelper.ThreeRetriesAsync().ExecuteAsync(() =>
                _stockTraderBroker.BaseAddress
                    .AppendPathSegments(_stockTraderBroker.StockTraderBrokerPath.SellRequest, id)
                    .WithOAuthBearerToken(jwtToken).DeleteAsync().ReceiveJson<ValidationResult>());
        }
    }
}
