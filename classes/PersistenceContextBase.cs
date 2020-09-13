using LRCA.classes.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;

namespace LRCA.classes
{
	public abstract class PersistenceContextBase: DbContext, IPersistenceContext
	{
		#region Constructor
		protected PersistenceContextBase()
			: base()
		{
			Configuration.LazyLoadingEnabled = false;
			Configuration.ProxyCreationEnabled = false;
		}
		protected PersistenceContextBase(string nameOrConnectionString)
			: base(nameOrConnectionString)
		{
			Configuration.LazyLoadingEnabled = false;
			Configuration.ProxyCreationEnabled = false;
			_connectionString = nameOrConnectionString;
		}
		#endregion

		#region OnModelCreating
		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			var configurations = modelBuilder.Configurations;
			var nullTypes = Assembly
				.GetAssembly(typeof(DomainObject))
				.ExportedTypes
				.Where(each => each.Name.StartsWith("Null", StringComparison.Ordinal));
			modelBuilder.Ignore(nullTypes);
			
		}
		#endregion

		#region Insert
		protected virtual TEntity Insert<TEntity>(TEntity entity) where TEntity : class
		{
			var entityType = entity.GetType();
			var baseType = entityType.BaseType;
			var domainObject = entity as DomainObject;
			if (EntityNamesWithManualIdentityGeneration.Contains(entityType.Name))
			{
				if (!ReferenceEquals(null, domainObject) && domainObject.Id == default(int))
				{
					PropertyInfo propertyInfo = entityType.BaseType.GetProperty("Id");
					propertyInfo.SetValue(domainObject, NextId(entity.GetType().Name));
				}
			}
			else if (EntityNamesWithManualIdentityGeneration.Contains(baseType.Name))
			{
				if (!ReferenceEquals(null, domainObject) && domainObject.Id == default(int))
				{
					PropertyInfo propertyInfo = baseType.BaseType.GetProperty("Id");
					propertyInfo.SetValue(domainObject, NextId(baseType.Name));
				}
			}
			return Set<TEntity>().Add(entity);
		}
		#endregion

		#region Update
		protected virtual TEntity Update<TEntity>(TEntity entity) where TEntity : class
		{
			var obj = entity as DomainObject;
			if (!ReferenceEquals(null, obj))
			{
				obj.SetModified();
			}
			Entry(entity).State = EntityState.Modified;
			return entity;
		}
		#endregion

		#region FindAsync
		protected virtual async Task<TEntity> FindAsync<TEntity>(int id) where TEntity : class
		{
			return await Set<TEntity>().FindAsync(id);
		}
		#endregion

		#region Delete
		protected virtual TEntity Delete<TEntity>(TEntity entity, bool force = false) where TEntity : class
		{
			var obj = entity as DomainObject;
			if (!ReferenceEquals(null, obj) && !force)
			{
				obj.SetModified();
				Entry(entity).State = EntityState.Modified;
			}
			else
			{
				Set<TEntity>().Remove(entity);
			}
			return entity;
		}
		#endregion

		private int NextId(string entityName)
		{
			var connectionStringBuilder = _dbProviderFactory.CreateConnectionStringBuilder();
			connectionStringBuilder.ConnectionString = _connectionString;
			connectionStringBuilder["Pooling"] = false;
			const string SelectUpdateCommandText = @"
                DECLARE @next_id INT = 0
                UPDATE [tbl_id_store] SET @next_id = [next_id], [next_id] = @next_id + 1 WHERE [table_name] = @table_name
                SELECT @next_id";
			var metadataWorkspace = ((IObjectContextAdapter)this).ObjectContext.MetadataWorkspace;
			var transactionOptions = new TransactionOptions
			{
				IsolationLevel = System.Transactions.IsolationLevel.Serializable
			};
			using (var connection = _dbProviderFactory.CreateConnection())
			using (var command = connection.CreateCommand())
			{
				connection.ConnectionString = connectionStringBuilder.ConnectionString;
				connection.Open();
				command.CommandText = SelectUpdateCommandText;
				AddTableNameParameter(metadataWorkspace.GetTableName(entityName), command);
				var result = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
				return result;
			}
		}

		private static void AddTableNameParameter(string tableName, DbCommand command)
		{
			var parameter = command.CreateParameter();
			parameter.ParameterName = "@table_name";
			parameter.DbType = DbType.AnsiString;
			parameter.Size = 200;
			parameter.Value = tableName;
			command.Parameters.Add(parameter);
		}

		#region IPersistenceContext Members
		TEntity IPersistenceContext.Insert<TEntity>(TEntity entity)
		{
			return Insert(entity);
		}
		TEntity IPersistenceContext.Update<TEntity>(TEntity entity)
		{
			return Update(entity);
		}
		async Task<TEntity> IPersistenceContext.FindAsync<TEntity>(int id)
		{
			return await FindAsync<TEntity>(id);
		}
		TEntity IPersistenceContext.Delete<TEntity>(TEntity entity, bool force)
		{
			return Delete(entity, force);
		}
		int IPersistenceContext.SaveChanges()
		{
			return SaveChanges();
		}
		#endregion

		#region Fields
		private static readonly HashSet<String> EntityNamesWithManualIdentityGeneration = new HashSet<string> {
			nameof(TrainingCourse),
			nameof(Instructor),
			nameof(SP_Contractor),
			nameof(TrainingProvider),
			nameof(Inspector_RiskAssessor),
			nameof(Supervisor)
		};
		private readonly string _connectionString;
		private static readonly DbProviderFactory _dbProviderFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
		#endregion
	}
}