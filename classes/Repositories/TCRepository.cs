using LRCA.classes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Data.SqlClient;

namespace LRCA.classes.Repositories
{
    public class TCRepository : ITCRepository
    {
        #region Constructor
        public TCRepository(IGroupDataContext groupDataContext)
        {
            _context = groupDataContext;
        }
        #endregion

        private readonly IGroupDataContext _context;

		void ITCRepository.Add(TrainingCourse tc)
		{
			_context.Insert(tc);
		}

		IQueryable<TrainingCourse> ITCRepository.PendingApps()
		{
			return _context
				.TrainingCourses
                .Where(x => !x.Approvals.Select(s => s.TrainingCourseAppId).Contains(x.Id));
        }

		IQueryable<TrainingCourse> ITCRepository.AssignToMDEApps(int id)
		{
			return _context
				.TrainingCourses
                .Where(x => x.Approvals.Select(s => s.MDE_Owner_AuthorisedUserId).FirstOrDefault(e => e.Value == id) != null && (x.IsActive == 4 || x.IsActive == 2 || x.IsActive == 3)); ;
        }

		TrainingCourse ITCRepository.Get(int id)
		{
			return _context
				.TrainingCourses
                .Include(i => i.Files)
                .FirstOrDefault(x => x.Id == id);
		}
        IQueryable<TrainingCourse> ITCRepository.ApprovedApps()
        {
            return _context
               .TrainingCourses
               .Where(x => x.IsActive == 1);
        }

        IQueryable<TrainingCourse> ITCRepository.DisapprovedApps()
        {
            return _context
               .TrainingCourses
               .Where(x => x.IsActive == 0);
        }

		void ITCRepository.Update(TrainingCourse trainingCourse)
		{
			_context.Update(trainingCourse);
		}
        void ITCRepository.AddFile(TrainingCourse_File TrainingCourse_File)
        {
            _context.Insert(TrainingCourse_File);
        }
        AccreditationResult ITCRepository.GetAccreditationById(string roleId, string id)
        {
            var query = @"SELECT        tbl_TrainingCourse.TrainingCourseAppId, tbl_TrainingCourse.TrainingProviderName AS 'TPName', tbl_TrainingCourse.TC_RiskAssessor, tbl_TrainingCourse.TC_InspectorTech, tbl_TrainingCourse.TC_VisualInspector, 
                         tbl_TrainingCourse.TC_Main_Repair, tbl_TrainingCourse.TC_Removal, tbl_TrainingCourse.TC_ProjectDesign, tbl_TrainingCourse.TC_AbatementWorkerEnglish, tbl_TrainingCourse.TC_AbatementWorkerSpanish, 
                         tbl_TrainingCourse.TC_StructSteelSuper, tbl_TrainingCourse.TC_StructSteelWorker, CONVERT(varchar(10), CAST(tbl_TrainingCourse.CreatedDate AS date), 101) AS 'CourseDate', 
                         'Abatement Worker, Spanish' AS 'CourseName', tbl_TrainingCourse.TPContactFirstName + ' ' + tbl_TrainingCourse.TPContactLastName AS 'Name', tbl_Accreditations.AccreditationId AS 'Number', CONVERT(varchar(10), 
                         CAST(tbl_Accreditations.ExpirationDate AS date), 101) AS 'ExpDate', CONVERT(varchar(10), CAST(tbl_Accreditations.CreatedDate AS date), 101) AS 'Date', tbl_Accreditations.CreatedBy
FROM            tbl_Accreditations INNER JOIN
                         tbl_TrainingCourse ON tbl_Accreditations.ApplicationId = tbl_TrainingCourse.TrainingCourseAppId
WHERE        (tbl_Accreditations.RoleId = @roleid) AND (tbl_TrainingCourse.TrainingCourseAppId = @id)";
            var pRoleID = new SqlParameter();
            pRoleID.ParameterName = "@roleid";
            pRoleID.Value = roleId;
            var pID = new SqlParameter();
            pID.ParameterName = "@id";
            pID.Value = id;
            var result = ((DbContext)_context).Database.SqlQuery<AccreditationResult>(query, new object[] { pRoleID, pID }).FirstOrDefault();
            return result;
        }
    }
}