using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;

namespace LRCA.classes
{
	public class UnitOfWork : IUnitOfWork
	{
		#region Constructor
		public UnitOfWork(IGroupDataContext groupDataContext, IAuditor auditor)
		{
			PersistenceContexts = new IPersistenceContext[] { groupDataContext };
			Auditor = auditor;
		}
		#endregion

		#region Commit
		void IUnitOfWork.Commit()
		{
			using (var transaction = new TransactionScope())
			{
				Array.ForEach(PersistenceContexts, each =>
				{
					Auditor.Audit(each);
					each.SaveChanges();
				});
				transaction.Complete();
			}
		}
		void IUnitOfWork.Commit(Guid taskId)
		{
			using (var transaction = new TransactionScope())
			{
				Array.ForEach(PersistenceContexts, each =>
				{
					Auditor.Audit(each);
					each.SaveChanges();
				});
				transaction.Complete();
			}
		}
		#endregion

		#region Fields
		private readonly IPersistenceContext[] PersistenceContexts;
		private readonly IAuditor Auditor;
		#endregion
	}
}