
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using LRCA.classes.Models;

namespace LRCA.classes.ModelConfiguration
{
	public class AccreditationsConfiguration : DomainObjectConfiguration<Accreditations>
	{

		public AccreditationsConfiguration() : base("AccreditationId")
		{
			ToTable("tbl_Accreditations");
			Property(t => t.Id)
				.HasColumnName("AccreditationId")
				.HasColumnOrder(1)
				.HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
			Property(t => t.RoleId).HasColumnName("RoleId");
			Property(t => t.ApplicationId).HasColumnName("ApplicationId");
			Property(t => t.CreatedDate).HasColumnName("CreatedDate");
			Property(t => t.CreatedBy).HasColumnName("CreatedBy").HasMaxLength(255).IsOptional();
			Property(t => t.UpdatedDate).HasColumnName("UpdatedDate");
			Property(t => t.UpdatedBy).HasColumnName("UpdatedBy").HasMaxLength(255).IsOptional();
			Property(t => t.Notes).HasColumnName("Notes").HasColumnType("varchar(max)").IsOptional();
			Property(t => t.IsActive).HasColumnName("IsActive");
		}
	}
}


