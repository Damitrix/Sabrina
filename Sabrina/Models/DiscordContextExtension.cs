using Microsoft.EntityFrameworkCore;

namespace Sabrina.Models
{
	public partial class DiscordContext : DbContext
	{
		private int _CommandTimeout = 10;
		private int _MaxBatchSize = 100;

		public DiscordContext(int timeout, int maxBatchSize)
		{
			_CommandTimeout = timeout;
			_MaxBatchSize = maxBatchSize;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseSqlServer(Configuration.Config.DatabaseConnectionString, o =>
				{
					o.CommandTimeout(_CommandTimeout);
					o.MaxBatchSize(_MaxBatchSize);
				});
			}
		}
	}
}