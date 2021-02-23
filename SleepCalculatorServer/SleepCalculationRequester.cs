using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SleepCalculatorServer
{
	class SleepCalculationRequester
	{
		public int Id { get; set; }
		[Required, MinLength(4), MaxLength(16)]
		public byte[] IPAddressBytes { get; set; }

		public virtual ICollection<SleepCalculationRequest> Requests { get; set; }
	}
}
