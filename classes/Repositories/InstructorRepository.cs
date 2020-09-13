﻿using LRCA.classes.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace LRCA.classes.Repositories
{
    public class InstructorRepository : IInstructorRepository
	{
        #region Constructor
        public InstructorRepository(IGroupDataContext groupDataContext)
        {
            _context = groupDataContext;
        }
        #endregion

        private readonly IGroupDataContext _context;

		void IInstructorRepository.Add(Instructor instructor)
		{
			_context.Insert(instructor);
		}

		Instructor IInstructorRepository.Get(int id)
		{
			return _context
				.Instructors
                .Include(i => i.Files)
                .FirstOrDefault(x => x.Id == id);
		}

		IQueryable<Instructor> IInstructorRepository.PendingApps()
		{
			return _context
				.Instructors
				.Include(i=>i.ACRDCat)
				.Where(x => !x.Approvals.Select(s => s.InstructorId).Contains(x.Id));
		}

		IQueryable<Instructor> IInstructorRepository.AssignToMDEApps(int id)
		{
			return _context
				.Instructors
				.Include(i => i.ACRDCat)
				.Where(x => x.Approvals.Select(s => s.MDE_Owner_AuthorisedUserId).FirstOrDefault(e => e.Value == id) != null && (x.IsActive == 4 || x.IsActive == 2 || x.IsActive == 3));
		}
        IQueryable<Instructor> IInstructorRepository.ApprovedApps()
        {
            return _context
               .Instructors
                .Include(i => i.ACRDCat)
                .Where(x => x.IsActive == 1);
        }
        IQueryable<Instructor> IInstructorRepository.DisapprovedApps()
        {
            return _context
               .Instructors
                .Include(i => i.ACRDCat)
                .Where(x => x.IsActive == 0);
        }

		void IInstructorRepository.Update(Instructor instructor)
		{
			_context.Update(instructor);
		}
        void IInstructorRepository.AddFile(Instructor_File Instructor_File)
        {
            _context.Insert(Instructor_File);
        }
        AccreditationResult IInstructorRepository.GetAccreditationById(string roleId, string id)
        {
            var query = @"SELECT  TOP (1) tbl_Instructor.IsRenewal, tbl_Instructor.Instructor_FName + ' ' + tbl_Instructor.Instructor_LName AS 'Name', tbl_Category.CatTitle AS 'CourseName', CONVERT(varchar(10), 
                         CAST(tbl_Instructor.AccreditationExpirationDate AS date), 101) AS RefExpireDate, tbl_Instructor.AccreditationID, tbl_Accreditations.AccreditationId AS 'Number', CONVERT(varchar(10), 
                         CAST(tbl_Accreditations.ExpirationDate AS date), 101) AS 'ExpDate', '-' AS 'TP_Name', CONVERT(varchar(10), CAST(tbl_Accreditations.CreatedDate AS date), 101) AS 'CourseDate'
FROM            tbl_Instructor INNER JOIN
                         tbl_Accreditations ON tbl_Instructor.InstructorId = tbl_Accreditations.ApplicationId INNER JOIN
                         tbl_Category ON tbl_Instructor.ACRDCatID = tbl_Category.ACRDCatID
WHERE        (tbl_Accreditations.RoleId = @roleid) AND (tbl_Instructor.InstructorId = @id)";
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