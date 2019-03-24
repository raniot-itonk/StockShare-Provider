using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using StockShareProvider.Helpers;
using StockShareProvider.OptionModels;

namespace StockShareProvider.Clients
{
    public interface IPublicShareOwnerControlClient
    {
        Task<Stock> GetStock(long id, string jwtToken);
    }

    public class PublicShareOwnerControlClient : IPublicShareOwnerControlClient
    {
        private readonly PublicShareOwnerControl _publicShareOwnerControl;

        public PublicShareOwnerControlClient(IOptionsMonitor<Services> serviceOption)
        {
            _publicShareOwnerControl = serviceOption.CurrentValue.PublicShareOwnerControl ??
                           throw new ArgumentNullException(nameof(serviceOption.CurrentValue.PublicShareOwnerControl));
        }
        public async Task<Stock> GetStock(long id, string jwtToken)
        {
            return await PolicyHelper.ThreeRetriesAsync().ExecuteAsync(() =>
                _publicShareOwnerControl.BaseAddress
                    .AppendPathSegments(_publicShareOwnerControl.PublicSharePath.Stock,id, "ownership")
                    .WithOAuthBearerToken(jwtToken).GetJsonAsync<Stock>());
        }
    }

    public class Stock
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public double LastTradedValue { get; set; }
        public List<Shareholder> ShareHolders { get; set; }
    }

    public class Shareholder
    {
        public Guid Id { get; set; }
        public int Amount { get; set; }
    }

    public class OwnershipRequest
    {
        public Guid Seller { get; set; }
        public Guid Buyer { get; set; }
        public int Amount { get; set; }
    }
}
