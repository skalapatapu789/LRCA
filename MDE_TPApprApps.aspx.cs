using LRCA.classes;
using System;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LRCA.classes.Models;
using LRCA.classes.Repositories;
using LRCA.classes.Entity;
using LRCA.classes.DAL;
using System.Collections.Generic;

namespace LRCA
{
    public partial class MDE_TPApprApps : PageBase
    {
        CryptoJS objcryptoJS = new CryptoJS();
        private readonly ITPRepository _tpRepository;
        public MDE_TPApprApps()
        {
            _tpRepository = new TPRepository(_context);
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
                var pendingApps = _tpRepository.ApprovedApps();
                foreach (var each in pendingApps)
                {
                    showTable(pnlVideos, each);
                }

                var mdeApps = _tpRepository.DisapprovedApps();
                foreach (var each in mdeApps)
                {
                    showTable(pnlDisapproved, each);
                }

            }
        }
        protected void showTable(Panel pnlName, TrainingProvider tp)
        {
            var id = objcryptoJS.AES_encrypt(tp.Id.ToString(), AppConstants.secretKey, AppConstants.initVec);
            var AppId = tp.Id.ToString();
            var AcctNum = "";
            var AcctExp = "";

            List<clsAccreditations> lstAcct = new List<clsAccreditations>();
            lstAcct = AccreditationsDAL.SelectDynamicAccreditations("RoleId = 2 and ApplicationId = " + AppId + "", "AccreditationId");
            if (lstAcct != null)
            {
                if (lstAcct.Count > 0)
                {
                    AcctNum = lstAcct[0].AccreditationId.ToString();
                    AcctExp = lstAcct[0].ExpirationDate.ToString();
                }
            }

            StringBuilder strContent = new StringBuilder("<tr>");
            strContent.Append("<td width='15%' nowrap><a style='text-decoration: underline;' href='MDE_TPAppView.aspx?TPApps=active&cgi=" + System.Web.HttpUtility.UrlEncode(id) + "' >");
            strContent.Append(tp.TP_Name);
            strContent.Append("</a></td>");
            strContent.Append("<td width='15%' nowrap>");
            if(tp.RiskAssessor == 1)
            {
                strContent.Append("Risk Assessor");
            }
            if (tp.InspectorTech == 1)
            {
                strContent.Append("Inspector Technician");
            }
            if (tp.VisualInspector == 1)
            {
                strContent.Append("Visual Inspector");
            }
            if (tp.MainRepaint == 1)
            {
                strContent.Append("Maintenance and Repainting");
            }
            if (tp.RemovalDemo == 1)
            {
                strContent.Append("Removal and Demolition");
            }
            if (tp.ProjectDesign == 1)
            {
                strContent.Append("Project Designer");
            }
            if (tp.AbatWorkerEnglish == 1)
            {
                strContent.Append("Abatement Worker");
            }
            if (tp.AbatWorkerSpanish == 1)
            {
                strContent.Append("Abatement Worker, Spanish");
            }
            if (tp.StructSteelSupervisor == 1)
            {
                strContent.Append("Structural Steel Supervisor");
            }
            if (tp.StructSteelWorker == 1)
            {
                strContent.Append("Structural Steel Worker");
            }
          
            strContent.Append("</td>");
            if (pnlName == pnlDisapproved)
            {
                strContent.Append("<td width='10%'nowrap>");
                strContent.Append(tp.TPContactFirstName + " " + tp.TPContactLastName);
                strContent.Append("</td>");
                strContent.Append("<td width='10%'nowrap>");
                strContent.Append(tp.TP_Telephone);
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
            strContent.Append(tp.CreatedDate.ToShortDateString());
            strContent.Append("</td>");
            if (pnlName != pnlDisapproved)
            {
                strContent.Append("<td width='5%' nowrap>");
                strContent.Append("<a class='btn btn-xs btn-success download' title='Download as PDF' href='/" + objcryptoJS.AES_encrypt("Acct_Certificate_2" + "_" + AppId, AppConstants.secretKey, AppConstants.initVec) + ".cert' target='_blank' >Generate Certificate</a>");
                strContent.Append("</td>");
            }
            //***************************************
            strContent.Append("<td width='5%' nowrap>");
            strContent.Append("<a class='btn btn-xs btn-success' href='MDE_TPAppView.aspx?TPApps=active&cgi=" + System.Web.HttpUtility.UrlEncode(id) + "'>View</a>");
            strContent.Append("</td>");

            pnlName.Controls.Add(new LiteralControl(strContent.ToString()));

        }
    }
}