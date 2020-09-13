
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;

namespace LRCA.classes.Entity
{
	
    public class clsTrainingCards
    {
		#region Public Properties
		public int? TrainingCardId { get; set; }
		public int? AuthorisedUserId { get; set; }
		public int? TPId { get; set; }
		public int? CourseId { get; set; }
		public int? Inst_CourseSchId { get; set; }
		public DateTime? ExpirationDate { get; set; }
		public DateTime? CreatedDate { get; set; }
		public string CreatedBy { get; set; }
		public DateTime? UpdatedDate { get; set; }
		public string UpdatedBy { get; set; }
		public string Notes { get; set; }
		public int? IsActive { get; set; }
		#endregion
	}
}
		
		
