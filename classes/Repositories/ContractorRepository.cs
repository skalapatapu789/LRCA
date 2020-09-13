using LRCA.classes.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace LRCA.classes.Repositories
{
	public sealed class ContractorRepository : IContractorRepository
	{
		#region Constructor
		public ContractorRepository(IGroupDataContext groupDataContext)
		{
			_context = groupDataContext;
		}
		#endregion

		private readonly IGroupDataContext _context;

		IReadOnlyCollection<SP_Contractor> IContractorRepository.All()
		{
			return _context.Contractors.ToList();
		}

		void IContractorRepository.Add(SP_Contractor contractor)
		{
			_context.Insert(contractor);
		}

		void IContractorRepository.Update(SP_Contractor contractor)
		{
			_context.Update(contractor);
		}

		IReadOnlyCollection<LK_Regions> IContractorRepository.AllRegionByStatus()
		{
			return _context.Regions
				.Where(x => x.Status == 1)
				.ToList();
		}

		IReadOnlyCollection<LK_ServicesOffered> IContractorRepository.AllServicesOfferedByStatus()
		{
			return _context.ServicesOffered
				.Where(x => x.Status == 1)
				.ToList();
		}

		void IContractorRepository.AddEmpList(Contractor_EmpList contractor_EmpList)
		{
			_context.Insert(contractor_EmpList);
		}

		void IContractorRepository.AddRegion(Contractor_Region region)
		{
			_context.Insert(region);
		}

		void IContractorRepository.AddServiceOffered(Contractor_ServiceOffered contractor_ServiceOffered)
		{
			_context.Insert(contractor_ServiceOffered);
		}

		void IContractorRepository.AddAddress(Contractor_Address contractor_Address)
		{
			_context.Insert(contractor_Address);
		}

		void IContractorRepository.AddUser(LK_Contractor_User objConUser)
		{
			_context.Insert(objConUser);
		}

		SP_Contractor IContractorRepository.Get(int strSPContractorID)
		{
			return _context
				.Contractors
				.Include(i => i.ACRDCat)
				.Include(i => i.ContractorRegions)
				.Include(i => i.ContractorRegions.Select(s => s.Region))
				.Include(i => i.ContractorAddresses)
				.Include(i => i.ContractorEmpList)
				.Include(i => i.ContractorServices)
				.Include(i => i.ContractorServices.Select(s => s.ServiceOffer))
				.Include(i => i.ContractorFiles)
				.FirstOrDefault(x => x.Id == strSPContractorID);
		}

		void IContractorRepository.AddFile(Contractor_File contractor_File)
		{
			_context.Insert(contractor_File);
		}

		AccreditationResult IContractorRepository.GetAccreditationById(string roleId, string id)
		{
			var query = @"SELECT TOP (1) tbl_SP_Contractor.IsRenewal, tbl_SP_Contractor.SPName AS 'Name', tbl_Category.CatTitle AS 'CourseName', CONVERT(varchar(10), CAST(tbl_SP_Contractor.AccreditationExpirationDate AS date), 101) AS RefExpireDate, 
                         tbl_SP_Contractor.AccreditationID, tbl_Accreditations.AccreditationId AS 'Number', CONVERT(varchar(10), CAST(tbl_Accreditations.ExpirationDate AS date), 101) AS 'ExpDate', '-' AS 'TP_Name', CONVERT(varchar(10), 
                         CAST(tbl_Accreditations.CreatedDate AS date), 101) AS 'CourseDate'
FROM            tbl_SP_Contractor INNER JOIN
                         tbl_Accreditations ON tbl_SP_Contractor.SPContractorID = tbl_Accreditations.ApplicationId INNER JOIN
                         tbl_Category ON tbl_SP_Contractor.ACRDCatID = tbl_Category.ACRDCatID
WHERE        (tbl_Accreditations.RoleId = @roleid) AND (tbl_SP_Contractor.SPContractorID = @id)";
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