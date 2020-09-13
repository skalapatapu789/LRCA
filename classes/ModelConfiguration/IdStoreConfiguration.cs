namespace LRCA.classes.ModelConfiguration
{
	using System.Data.Entity.ModelConfiguration;

	public sealed class IdStoreConfiguration : EntityTypeConfiguration<IdStore>
	{
		public IdStoreConfiguration() {
			ToTable("tbl_id_store");
			HasKey(t => t.TableName);
			Property(t => t.TableName).HasColumnName("table_name").HasMaxLength(200).IsUnicode(false).IsRequired();
			Property(t => t.NextId).HasColumnName("next_id");
		}
	}
}
