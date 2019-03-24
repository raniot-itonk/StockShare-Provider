using System;

namespace StockShareProvider.Models
{
    public class SellRequestInput
    {
        public Guid AccountId { get; set; }
        public long StockId { get; set; }
        public double Price { get; set; }
        public DateTime TimeOut { get; set; }
        public int AmountOfShares { get; set; }
    }
}