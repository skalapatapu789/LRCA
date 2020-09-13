using System;

namespace LRCA.classes
{
	public interface IUnitOfWork
	{
		void Commit();
		void Commit(Guid taskId);
	}
}