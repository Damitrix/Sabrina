using Microsoft.EntityFrameworkCore;

namespace Sabrina.SankakuModel
{
	public partial class SankakuContext : DbContext
	{
		private int _CommandTimeout = 60;
		private int _MaxBatchSize = 200;

		public SankakuContext(int timeout, int maxBatchSize)
		{
			_CommandTimeout = timeout;
			_MaxBatchSize = maxBatchSize;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseSqlServer(Configuration.Config.SankakuConnectionString, o =>
				{
					o.CommandTimeout(_CommandTimeout);
					o.MaxBatchSize(_MaxBatchSize);
				});
			}
		}
	}
}