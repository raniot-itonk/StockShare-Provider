using Microsoft.EntityFrameworkCore;

namespace StockShareProvider.DB
{
    public class StockShareProviderContext : DbContext
    {
        public StockShareProviderContext(DbContextOptions<StockShareProviderContext> options)
            : base(options)
        {
        }
    }
}
