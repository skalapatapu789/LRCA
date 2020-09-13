using LRCA.classes.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace LRCA.classes.Repositories
{
	public class TPRepository : ITPRepository
	{
		#region Constructor
		public TPRepository(IGroupDataContext groupDataContext)
		{
			_context = groupDataContext;
		}
		#endregion

		private readonly IGroupDataContext _context;

		void ITPRepository.Add(TrainingProvider tp)
		{
			_context.Insert(tp);
		}

		void ITPRepository.AddLocation(TP_Location tP_Location)
		{
			_context.Insert(tP_Location);
		}

		void ITPRepository.AddInstrutor(TP_Instructors tP_Instructors)
		{
			_context.Insert(tP_Instructors);
		}

		IQueryable<TrainingProvider> ITPRepository.PendingApps()
		{
			return _context
				.TrainingProviders
				.Where(x => !x.Approvals.Select(s => s.TPId).Contains(x.Id));
		}

		TrainingProvider ITPRepository.Get(int id)
		{
			return _context
				.TrainingProviders
				.Include(i => i.Locations)
				.Include(i => i.Instructors)
				 .Include(i => i.Files)
				.FirstOrDefault(x => x.Id == id);
		}

		IQueryable<TrainingProvider> ITPRepository.AssignToMDEApps(int id)
		{
			return _context
				.TrainingProviders
				.Where(x => x.Approvals.Select(s => s.MDE_Owner_AuthorisedUserId).FirstOrDefault(e => e.Value == id) != null && (x.IsActive == 4 || x.IsActive == 2 || x.IsActive == 3)); ;
		}

		IQueryable<TrainingProvider> ITPRepository.ApprovedApps()
		{
			return _context
			   .TrainingProviders
			   .Where(x => x.IsActive == 1);
		}

		IQueryable<TrainingProvider> ITPRepository.DisapprovedApps()
		{
			return _context
			   .TrainingProviders
			   .Where(x => x.IsActive == 0);
		}

		void ITPRepository.Update(TrainingProvider trainingProvider)
		{
			_context.Update(trainingProvider);
		}
		void ITPRepository.AddFile(TP_File TP_File)
		{
			_context.Insert(TP_File);
		}

		AccreditationResult ITPRepository.GetAccreditationById(string roleId, string id)
		{
			var query = @"SELECT tbl_TrainingProvider.TPId, 
						 'TPName' = tbl_TrainingProvider.TP_Name, 
						 tbl_TrainingProvider.RiskAssessor, 
						 tbl_TrainingProvider.InspectorTech, 
						 tbl_TrainingProvider.VisualInspector, 
						 tbl_TrainingProvider.MainRepaint,
                         tbl_TrainingProvider.RemovalDemo, 
						 tbl_TrainingProvider.ProjectDesign, 
						 tbl_TrainingProvider.AbatWorkerEnglish, 
						 tbl_TrainingProvider.AbatWorkerSpanish, 
						 tbl_TrainingProvider.StructSteelSupervisor,
                         tbl_TrainingProvider.StructSteelWorker, 
                         'CourseDate' =  CONVERT(varchar(10), CAST(tbl_TrainingProvider.CreatedDate AS date), 101),
                         'CourseName' = 'Abatement Worker, Spanish (CW2sp)',
						 'Name' = tbl_TrainingProvider.TPContactFirstName +' '+ tbl_TrainingProvider.TPContactLastName, 
						 'Number' = tbl_Accreditations.AccreditationId, 
						 'ExpDate' = CONVERT(varchar(10), CAST(tbl_Accreditations.ExpirationDate AS date), 101), 
						 'Date' = CONVERT(varchar(10), CAST(tbl_Accreditations.CreatedDate AS date), 101),
                         tbl_Accreditations.CreatedBy
					FROM tbl_Accreditations INNER JOIN
                         tbl_TrainingProvider ON tbl_Accreditations.ApplicationId = tbl_TrainingProvider.TPId
					WHERE (tbl_Accreditations.RoleId = @roleid) AND (tbl_TrainingProvider.TPId = @id)";
			var pRoleID = new SqlParameter();
			pRoleID.ParameterName = "@roleid";
			pRoleID.Value = roleId;
			var pID = new SqlParameter();
			pID.ParameterName = "@id";
			pID.Value = id;
			var result = ((DbContext)_context).Database.SqlQuery<AccreditationResult>(query, new object[] { pRoleID, pID }).FirstOrDefault();
			return result;
		}

		TrainingCardResult ITPRepository.GetTrainingCardById(string id)
		{
			var query = @"SELECT        
tbl_TrainingProvider.TP_Name AS ProviderName, 
tbl_Course_Result.AuthorisedUserId, '9/7/1978' AS DOB, 
tbl_User.FName + ' ' + tbl_User.LName AS Name, 
tbl_CourseSchedule.StartDate, 
tbl_CourseSchedule.EndDate,
tbl_MDE_Courses.CourseDescription AS ClassCode, 
tbl_TrainingCards.TrainingCardId, 
cast(tbl_TrainingCards.ExpirationDate as varchar(50)) AS ExpDate
FROM  tbl_Course_Result INNER JOIN
                         tbl_TrainingProvider ON tbl_Course_Result.TPId = tbl_TrainingProvider.TPId INNER JOIN
                         tbl_LK_Inst_CourseSchedule ON tbl_Course_Result.AuthorisedUserId = tbl_LK_Inst_CourseSchedule.AuthorisedUserId INNER JOIN
                         tbl_User ON tbl_Course_Result.AuthorisedUserId = tbl_User.AuthorisedUserId INNER JOIN
                         tbl_TP_Location ON tbl_Course_Result.TPLocationId = tbl_TP_Location.TPLocationId INNER JOIN
                         tbl_CourseSchedule ON tbl_Course_Result.TrainingCourseScheduleId = tbl_CourseSchedule.TrainingCourseScheduleId INNER JOIN
                         tbl_MDE_Courses ON tbl_CourseSchedule.CourseId = tbl_MDE_Courses.CourseId INNER JOIN
                         tbl_TrainingCards ON tbl_User.AuthorisedUserId = tbl_TrainingCards.AuthorisedUserId
WHERE        (tbl_TrainingProvider.TPId = @id) AND (tbl_LK_Inst_CourseSchedule.IsApproved = 1)";
			var pID = new SqlParameter();
			pID.ParameterName = "@id";
			pID.Value = id;
			var result = ((DbContext)_context).Database.SqlQuery<TrainingCardResult>(query, new object[] { pID }).FirstOrDefault();
			return result;
		}
	}
}