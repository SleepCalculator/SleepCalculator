using System.Data.Entity;

namespace SleepCalculatorServer
{
	class SleepCalculationContext : DbContext
	{
		public SleepCalculationContext() : base("DbConnection") { }
		public DbSet<SleepCalculationRequest> Requests { get; set; }
		public DbSet<SleepCalculationRequester> Requesters { get; set; }
	}
}
