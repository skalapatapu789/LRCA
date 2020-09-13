using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LRCA.classes
{
	public class TrainingCardResult
	{
		public string Name { get; internal set; }
		public string ClassCode { get; internal set; }
		public string DOB { get; internal set; }
		public string ProviderName { get; internal set; }
		public string ExpDate { get; internal set; }
		public int TrainingCardId { get; internal set; }
	}
}