﻿using LRCA.classes;
using System;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LRCA.classes.Models;
using LRCA.classes.Repositories;
using LRCA.classes.Entity;
using System.Collections.Generic;
using LRCA.classes.DAL;

namespace LRCA
{
    public partial class MDE_RiskApprApps : PageBase
    {
        CryptoJS objcryptoJS = new CryptoJS();
        private readonly IRiskAssessorRepository _riskAssessorRepository;
        public MDE_RiskApprApps()
        {
            _riskAssessorRepository = new RiskAssessorRepository(_context);
        }
        protected void Page_init(object sender, EventArgs e)
        {
            string EmpId = HttpContext.Current.Session["UserAuthId"] == null ? string.Empty : HttpContext.Current.Session["UserAuthId"].ToString();
            if (EmpId.Length == 0)
            {
                GlobalMethods.logout();
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var pendingApps = _riskAssessorRepository.ApprovedApps();
                foreach (var each in pendingApps)
                {
                    showTable(pnlVideos, each);
                }

                var mdeApps = _riskAssessorRepository.DisapprovedApps();
                foreach (var each in mdeApps)
                {
                    showTable(pnlDisapproved, each);
                }

            }
        }
        protected void showTable(Panel pnlName, Inspector_RiskAssessor inspector_RiskAssessor)
        {
            var id = objcryptoJS.AES_encrypt(inspector_RiskAssessor.Id.ToString(), AppConstants.secretKey, AppConstants.initVec);
            var AppId = inspector_RiskAssessor.Id.ToString();
            var AcctNum = "";
            var AcctExp = "";

            List<clsAccreditations> lstAcct = new List<clsAccreditations>();
            lstAcct = AccreditationsDAL.SelectDynamicAccreditations("RoleId = 15 and ApplicationId = "+ AppId +"", "AccreditationId");
            if(lstAcct != null)
            {
                if(lstAcct.Count > 0)
                {
                    AcctNum = lstAcct[0].AccreditationId.ToString();
                    AcctExp = lstAcct[0].ExpirationDate.ToString();
                }
            }
            StringBuilder strContent = new StringBuilder("<tr>");
            strContent.Append("<td width='15%' nowrap><a style='text-decoration: underline;' href='MDERiskAppView.aspx?RiskApps=active&cgi=" + System.Web.HttpUtility.UrlEncode(id) + "' >");
            strContent.Append(inspector_RiskAssessor.InspectorFirstName + " " + inspector_RiskAssessor.InspectorLastName);
            strContent.Append("</a></td>");
            strContent.Append("<td width='15%' nowrap>");
            strContent.Append(inspector_RiskAssessor.ACRDCat.CatTitle);
            strContent.Append("</td>");
            strContent.Append("<td width='10%'nowrap>");
            strContent.Append(AcctNum);
            strContent.Append("</td>");
            strContent.Append("<td width='10%' nowrap>");
            if(AcctExp.Length > 0)
            {
                strContent.Append(Convert.ToDateTime(AcctExp).ToShortDateString());
            }
            else
            {
                strContent.Append(AcctExp);
            }
            
            strContent.Append("</td>");
            strContent.Append("<td width='10%'nowrap>");
            strContent.Append(inspector_RiskAssessor.CreatedDate.ToShortDateString());
            strContent.Append("</td>");
            //***************************************
            if (pnlName != pnlDisapproved)
            {
                strContent.Append("<td width='5%' nowrap>");
                strContent.Append("<a class='btn btn-xs btn-success download' title='Download as PDF' href='/" + objcryptoJS.AES_encrypt("Acct_Certificate_15" + "_" + AppId, AppConstants.secretKey, AppConstants.initVec) + ".cert' target='_blank' >Generate Certificate</a>");
                strContent.Append("</td>");
            }
            //***************************************
            strContent.Append("<td width='5%' nowrap>");
            strContent.Append("<a class='btn btn-xs btn-success' href='MDERiskAppView.aspx?RiskApps=active&cgi=" + System.Web.HttpUtility.UrlEncode(id) + "'>View</a>");
            strContent.Append("</td>");

            pnlName.Controls.Add(new LiteralControl(strContent.ToString()));

        }
        protected void showTable(Panel pnlName, string CompName, string Category, string ACRDID, string ACRDDateExpire, string DateCreated, int SPContractorID)
        {
            string strSPContractorID = objcryptoJS.AES_encrypt(SPContractorID.ToString(), AppConstants.secretKey, AppConstants.initVec).ToString();

            StringBuilder strContent = new StringBuilder("<tr>");
            strContent.Append("<td width='15%' nowrap><a href='MDEContAppView.aspx?contapps=active&cgi=" + System.Web.HttpUtility.UrlEncode(strSPContractorID) + "' >");
            strContent.Append(CompName);
            strContent.Append("</a></td>");
            strContent.Append("<td width='15%' nowrap>");
            strContent.Append(Category);
            strContent.Append("</td>");
            strContent.Append("<td width='10%'nowrap>");
            strContent.Append(ACRDID);
            strContent.Append("</td>");
            strContent.Append("<td width='10%' nowrap>");
            strContent.Append(Convert.ToDateTime(ACRDDateExpire).ToShortDateString());
            strContent.Append("</td>");
            strContent.Append("<td width='10%'nowrap>");
            strContent.Append(Convert.ToDateTime(DateCreated).ToShortDateString());
            strContent.Append("</td>");
            //***************************************
            strContent.Append("<td width='5%' nowrap>");
            strContent.Append("<a class='btn btn-xs btn-success' href='MDEContAppView.aspx?contapps=active&cgi=" + System.Web.HttpUtility.UrlEncode(strSPContractorID) + "'>View</a>");
            strContent.Append("</td>");

            pnlName.Controls.Add(new LiteralControl(strContent.ToString()));

        }
    }
}