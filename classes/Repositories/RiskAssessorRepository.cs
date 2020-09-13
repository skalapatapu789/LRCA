using LRCA.classes.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace LRCA.classes.Repositories
{
	public class RiskAssessorRepository : IRiskAssessorRepository
	{
		private readonly IGroupDataContext _context;

		public RiskAssessorRepository(IGroupDataContext groupDataContext)
		{
			_context = groupDataContext;
		}
		void IRiskAssessorRepository.Add(Inspector_RiskAssessor riskAccessor)
		{
			_context.Insert(riskAccessor);
		}

		IQueryable<Inspector_RiskAssessor> IRiskAssessorRepository.AssignToMDEApps(int id)
		{
			return _context
				.RiskAssesors
				.Include(i => i.ACRDCat)
				.Where(x => x.Approvals.Select(s => s.MDE_Owner_AuthorisedUserId).FirstOrDefault(e=>e.Value == id) != null && (x.IsActive == 4 || x.IsActive == 2 || x.IsActive == 3));
		}

		Inspector_RiskAssessor IRiskAssessorRepository.Get(int id)
		{
			return _context
				.RiskAssesors
				.Include(i => i.ACRDCat)
                .Include(i => i.Files)
                .FirstOrDefault(x => x.Id == id);
		}

		IQueryable<Inspector_RiskAssessor> IRiskAssessorRepository.PendingApps()
		{
			return _context
				.RiskAssesors
				.Include(i=>i.ACRDCat)
				.Where(x => !x.Approvals.Select(s => s.InspectorRiskAssId).Contains(x.Id));
		}
        IQueryable<Inspector_RiskAssessor> IRiskAssessorRepository.ApprovedApps()
        {
            return _context
                .RiskAssesors
                .Include(i => i.ACRDCat)
                .Where(x => x.IsActive == 1);
        }
        IQueryable<Inspector_RiskAssessor> IRiskAssessorRepository.DisapprovedApps()
        {
            return _context
                .RiskAssesors
                .Include(i => i.ACRDCat)
                .Where(x => x.IsActive == 0);
        }

		void IRiskAssessorRepository.Update(Inspector_RiskAssessor  inspector_RiskAssessor)
		{
			_context.Update(inspector_RiskAssessor);
		}
        void IRiskAssessorRepository.AddFile(RiskAssessor_File RiskAssessor_File)
        {
            _context.Insert(RiskAssessor_File);
        }

        AccreditationResult IRiskAssessorRepository.GetAccreditationById(string roleId, string id)
        {
            var query = @"SELECT        TOP (1) tbl_Category.CatTitle AS 'CourseName', tbl_Accreditations.AccreditationId AS 'Number', CONVERT(varchar(10), CAST(tbl_Accreditations.ExpirationDate AS date), 101) AS 'ExpDate', CONVERT(varchar(10), 
                         CAST(tbl_Accreditations.CreatedDate AS date), 101) AS 'CourseDate', tbl_Inspector_RiskAssessor.InspectorRiskAssId, 
                         tbl_Inspector_RiskAssessor.InspectorFirstName + ' ' + tbl_Inspector_RiskAssessor.InspectorMiddleName + ' ' + tbl_Inspector_RiskAssessor.InspectorLastName AS Name, 
                         tbl_Inspector_RiskAssessor.CourseTPName AS TPName, tbl_Inspector_RiskAssessor.CourseExpirationDate AS CourseDate, CONVERT(varchar(10), CAST(tbl_Inspector_RiskAssessor.CreatedDate AS date), 101) AS Date
                         FROM tbl_Inspector_RiskAssessor INNER JOIN
                         tbl_Category ON tbl_Inspector_RiskAssessor.ACRDCatID = tbl_Category.ACRDCatID INNER JOIN
                         tbl_Accreditations ON tbl_Inspector_RiskAssessor.InspectorRiskAssId = tbl_Accreditations.ApplicationId
                         WHERE  (tbl_Accreditations.RoleId = @roleid) AND (tbl_Accreditations.ApplicationId = @id)";
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