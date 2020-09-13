using LRCA.classes;
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
    public partial class MDE_TCourseApprApps : PageBase
    {
        CryptoJS objcryptoJS = new CryptoJS();
        private readonly ITCRepository _tcRepository;
        public MDE_TCourseApprApps()
        {
            _tcRepository = new TCRepository(_context);
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
                var pendingApps = _tcRepository.ApprovedApps();
                foreach (var each in pendingApps)
                {
                    showTable(pnlVideos, each);
                }

                var mdeApps = _tcRepository.DisapprovedApps();
                foreach (var each in mdeApps)
                {
                    showTable(pnlDisapproved, each);
                }

            }
        }
        protected void showTable(Panel pnlName, TrainingCourse instructor)
        {
            var id = objcryptoJS.AES_encrypt(instructor.Id.ToString(), AppConstants.secretKey, AppConstants.initVec);
            var AppId = instructor.Id.ToString();
            var AcctNum = "";
            var AcctExp = "";

            List<clsAccreditations> lstAcct = new List<clsAccreditations>();
            lstAcct = AccreditationsDAL.SelectDynamicAccreditations("RoleId = 17 and ApplicationId = " + AppId + "", "AccreditationId");
            if (lstAcct != null)
            {
                if (lstAcct.Count > 0)
                {
                    AcctNum = lstAcct[0].AccreditationId.ToString();
                    AcctExp = lstAcct[0].ExpirationDate.ToString();
                }
            }

            StringBuilder strContent = new StringBuilder("<tr>");
            strContent.Append("<td width='15%' nowrap><a style='text-decoration: underline;' href='MDE_TCourseAppView.aspx?TCourseApps=active&cgi=" + System.Web.HttpUtility.UrlEncode(id) + "' >");
            strContent.Append(instructor.TrainingProviderName);
            strContent.Append("</a></td>");
            strContent.Append("<td width='15%' nowrap>");
            strContent.Append("Risk Assessor");
            strContent.Append("</td>");
            if (pnlName == pnlDisapproved)
            {
                strContent.Append("<td width='10%'nowrap>");
                strContent.Append(instructor.TPContactFirstName + " " + instructor.TPContactLastName);
                strContent.Append("</td>");
                strContent.Append("<td width='10%'nowrap>");
                strContent.Append(instructor.TP_Telephone);
                strContent.Append("</td>");
            }
            else
            {
                strContent.Append("<td width='10%'nowrap>");
                strContent.Append(AcctNum);
                strContent.Append("</td>");
                strContent.Append("<td width='10%'nowrap>");
                strContent.Append(Convert.ToDateTime(AcctExp).ToShortDateString());
                strContent.Append("</td>");
            }
            strContent.Append("<td width='10%'nowrap>");
            strContent.Append(instructor.CreatedDate.ToShortDateString());
            strContent.Append("</td>");
            if (pnlName != pnlDisapproved)
            {
                strContent.Append("<td width='5%' nowrap>");
                strContent.Append("<a class='btn btn-xs btn-success download' title='Download as PDF' href='/" + objcryptoJS.AES_encrypt("Acct_Certificate_17" + "_" + AppId, AppConstants.secretKey, AppConstants.initVec) + ".cert' target='_blank' >Generate Certificate</a>");
                strContent.Append("</td>");
            }

            //***************************************
            strContent.Append("<td width='5%' nowrap>");
            strContent.Append("<a class='btn btn-xs btn-success' href='MDE_TCourseAppView.aspx?TCourseApps=active&cgi=" + System.Web.HttpUtility.UrlEncode(id) + "'>View</a>");
            strContent.Append("</td>");

            pnlName.Controls.Add(new LiteralControl(strContent.ToString()));

        }
    }
}