
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LRCA.classes.Models
{
		public class TrainingCards : DomainObject
    {
		#region Public Properties
		
		public int? RoleId { get; set; }
		public int? ApplicationId { get; set; }
		public string Notes { get; set; }
		public int? IsActive { get; set; }
		
		#endregion
	}
}
		
		
