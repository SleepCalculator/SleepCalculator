using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SleepCalculatorServer
{
	class SleepCalculationRequest
	{
		public int Id { get; set; }
		public DateTime GoToBed { get; set; }
		public DateTime WakeUp { get; set; }
		public int SleepMinutes { get; set; }
		public SleepCalculateValue CalculatedValue { get; set; }

		[ForeignKey("Requester")]
		public int RequesterId { get; set; }
		public virtual SleepCalculationRequester Requester { get; set; }
	}
}
