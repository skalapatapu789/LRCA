using System;

namespace LRCA.classes
{
	public interface IAuditor
	{
		void Audit(IPersistenceContext persistenceContext);
		void Audit(IPersistenceContext persistenceContext, Guid taskId);
	}
}
