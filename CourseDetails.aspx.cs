using Dapper;
using LRCA.classes;
using LRCA.classes.DAL;
using LRCA.classes.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LRCA
{
    public partial class CourseDetails : System.Web.UI.Page
    {
        CryptoJS objcryptoJS = new CryptoJS();
        public static string CourseTitle = string.Empty;
        public static string CourseDesc = string.Empty;
        public static string AttendenceReq = string.Empty;

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
                string strCourseId = string.Empty;
                try
                {
                    strCourseId = Request["cgi"].ToString() == null ? string.Empty : Request["cgi"].ToString();
                    if (GlobalMethods.ValueIsNull(strCourseId).Length > 0)
                    {
                        strCourseId = objcryptoJS.AES_decrypt(HttpUtility.UrlEncode(Request["cgi"].ToString()), AppConstants.secretKey, AppConstants.initVec).ToString();
                        Session["ID"] = strCourseId;
                    }

                    #region Getting Course Information
                    clsCourseSchedule objCourse = new clsCourseSchedule();
                    objCourse = CourseScheduleDAL.SelectCourseScheduleById(Convert.ToInt32(strCourseId));
                    if(objCourse != null)
                    {
                        CourseTitle = objCourse.CourseDescription;
                        CourseDesc = objCourse.Notes;
                        AttendenceReq = objCourse.InstructionLanguage;
                    }
                    #endregion

                    #region Showing all the Courses added to the list. 
                    List<dynamic> lstValues;
                    string strSQL = @"SELECT DISTINCT 
                        tbl_CourseSchedule.InstructorId, tbl_CourseSchedule.TPLocationId, tbl_TP_Location.TP_Address_Line_1 as [Location], tbl_TrainingProvider.TP_Name, 
                        tbl_TrainingProvider.TP_Email, tbl_TrainingProvider.TP_Telephone,
                        tbl_CourseSchedule.CourseCost,tbl_CourseSchedule.ClassTitle, tbl_CourseSchedule.StartDate, tbl_CourseSchedule.EndDate, tbl_CourseSchedule.InstructionLanguage, 
                        tbl_Instructor.Instructor_FName, tbl_Instructor.Instructor_LName, tbl_TrainingProvider.TP_Name, tbl_CourseSchedule.TrainingCourseScheduleId
                        FROM            tbl_CourseSchedule INNER JOIN
                        tbl_TrainingProvider ON tbl_CourseSchedule.TPId = tbl_TrainingProvider.TPId INNER JOIN
                        tbl_Instructor ON tbl_CourseSchedule.InstructorId = tbl_Instructor.InstructorId INNER JOIN
                        tbl_TP_Location ON tbl_CourseSchedule.TPLocationId = tbl_TP_Location.TPLocationId
                         WHERE        (tbl_CourseSchedule.TrainingCourseScheduleId = @strCourseId)";
                    var objPar = new DynamicParameters();
                    objPar.Add("@strCourseId", strCourseId, DbType.Int32);
                    try
                    {
                        using (IDbConnection db = new SqlConnection(System.Configuration.ConfigurationManager.AppSettings["databaseConnection"]))
                        {
                            lstValues = db.Query<dynamic>(strSQL, objPar, commandType: CommandType.Text).ToList();
                            if (lstValues != null)
                            {
                                if (lstValues.Count > 0)
                                {
                                    for (int i = 0; i < lstValues.Count; i++)
                                    {
                                        showTable(pnlVideos, GlobalMethods.ValueIsNull(lstValues[i].ClassTitle), 
                                            GlobalMethods.ValueIsNull(lstValues[i].StartDate) + " - " + GlobalMethods.ValueIsNull(lstValues[i].EndDate), 
                                            GlobalMethods.ValueIsNull(lstValues[i].InstructionLanguage), GlobalMethods.ValueIsNull(lstValues[i].Location), 
                                            GlobalMethods.ValueIsNull(lstValues[i].TP_Name), GlobalMethods.ValueIsNull(lstValues[i].CourseCost),
                                            GlobalMethods.ValueIsNull(lstValues[i].TP_Telephone), GlobalMethods.ValueIsNull(lstValues[i].TP_Email), "", 
                                            GlobalMethods.ValueIsNull(lstValues[i].Instructor_FName) +" "+ GlobalMethods.ValueIsNull(lstValues[i].Instructor_LName), 
                                            Convert.ToInt32(GlobalMethods.ValueIsNull(lstValues[i].TrainingCourseScheduleId)));
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.ErrorLogging(ex, false);
                        ErrorHandler.ReadError();
                    }
                    #endregion

                }
                catch (Exception)
                {
                    ErrorHandler.ErrorPage();
                }
            }
        }
        protected void showTable(Panel pnlName, string ClassTitle, string StartDateEndDate, string InstructionLanguage, string Location, string TP_Name, string CourseCost, string TP_Telephone, string TP_Email, string Loc_Ti,string InstName, int CourseId)
        {
            string strSPContractorID = objcryptoJS.AES_encrypt(CourseId.ToString(), AppConstants.secretKey, AppConstants.initVec).ToString();
            bool IsEnrolled = false;

            StringBuilder strContent = new StringBuilder("<tr>");
            strContent.Append("<td width='15%' nowrap><a href='ClassDetails.aspx?dash=active&cgi=" + System.Web.HttpUtility.UrlEncode(strSPContractorID) + "' >");
            strContent.Append(ClassTitle);
            strContent.Append("</a></td>");
            strContent.Append("<td width='10%'nowrap>");
            strContent.Append(StartDateEndDate);
            strContent.Append("</td>");
            strContent.Append("<td width='10%'nowrap>");
            strContent.Append(InstructionLanguage);
            strContent.Append("</td>");
            strContent.Append("<td width='10%'nowrap>");
            strContent.Append(Location);
            strContent.Append("</td>");
            strContent.Append("<td width='10%'nowrap>");
            strContent.Append(TP_Name);
            strContent.Append("</td>");
            strContent.Append("<td width='10%'nowrap>");
            strContent.Append(CourseCost);
            strContent.Append("</td>");
            
            strContent.Append("<td width='10%'nowrap>");
            strContent.Append(TP_Telephone);
            strContent.Append("</td>");
            strContent.Append("<td width='10%'nowrap>");
            strContent.Append(TP_Email);
            strContent.Append("</td>");

            //strContent.Append("<td width='10%'nowrap>");
            //strContent.Append(TP_Name);
            //strContent.Append("</td>");
            //strContent.Append("<td width='10%'nowrap>");
            //strContent.Append("");
            //strContent.Append("</td>");
            //strContent.Append("<td width='10%'nowrap>");
            //strContent.Append(InstName);
            //strContent.Append("</td>");
            //***************************************
            strContent.Append("<td width='10%' nowrap>");

            #region Checking if this course is already registered.
            List<clsLK_Inst_CourseSchedule> objInstCS = new List<clsLK_Inst_CourseSchedule>();
            objInstCS = LK_Inst_CourseScheduleDAL.SelectDynamicLK_Inst_CourseSchedule("TrainingCourseScheduleId = " + CourseId + " and AuthorisedUserId = " + HttpContext.Current.Session["UserAuthId"].ToString() + "", "Inst_CourseSchId");
            Session["CourseID"] = CourseId;
            if (objInstCS != null)
            {
                if (objInstCS.Count > 0)
                {
                    if (objInstCS[0].IsApproved == 0)
                    {
                        // Applied but not approved
                        strContent.Append("Applied but Pending Approval   ");
                        //strContent.Append("<a href='#' class='btn btn-xs btn-success open-Enroll' data-id='" + System.Web.HttpUtility.UrlEncode(strSPContractorID) + "' data-toggle='modal'>Cancel</a>");
                        //strContent.Append("<a href='#' class='btn btn-xs btn-warning2'  >Applied but Pending Approval</a>");
                    }
                    else if (objInstCS[0].IsApproved == 1)
                    {
                        // This has been enrolled.
                        strContent.Append("Enrolled   ");
                    }
                }
                else
                {
                    // User should apply for the class.
                    strContent.Append("New   ");
                   // strContent.Append("<a href='#' class='btn btn-xs btn-success open-Enroll' data-id='" + System.Web.HttpUtility.UrlEncode(strSPContractorID) + "' data-toggle='modal'>Apply for Enrollment</a>");                   
                }
            }

            #endregion
            strContent.Append("</td>");
            pnlName.Controls.Add(new LiteralControl(strContent.ToString()));
            StringBuilder strContentbtn = new StringBuilder("<tr>");
            strContent.Append("<td width='10%' nowrap>");            
            if (objInstCS != null)
            {
                if (objInstCS.Count > 0)
                {
                    if (objInstCS[0].IsApproved == 0)
                    {
                        strContentbtn.Append("<a href='#' class='btn btn-xs btn-success open-Cancel' data-id='" + System.Web.HttpUtility.UrlEncode(strSPContractorID) + "' data-toggle='modal'>Cancel the Enrollment</a>");
                    }
                    else if (objInstCS[0].IsApproved == 1)
                    {

                        strContentbtn.Append("<a href='#' class='btn btn-xs btn-success open-Cancel' data-id='" + System.Web.HttpUtility.UrlEncode(strSPContractorID) + "' data-toggle='modal'>Cancel the Enrollment</a>");
                    }
                }
                else
                {
                    strContentbtn.Append("<a href='#' class='btn btn-xs btn-success open-Enroll' data-id='" + System.Web.HttpUtility.UrlEncode(strSPContractorID) + "' data-toggle='modal'>Apply for Enrollment</a>");
                }
            }
            strContentbtn.Append("</td>");
            pnlName.Controls.Add(new LiteralControl(strContentbtn.ToString()));
        }

        [System.Web.Services.WebMethod(EnableSession = false)]
        public static string Enroll(string cgi)
        {
            CryptoJS objcryptoJS = new CryptoJS();
            string strURL = string.Empty;
            string CourseSchdId = string.Empty;
            string CourseId = string.Empty;
            try
            {
                CourseSchdId = cgi.ToString() == null ? string.Empty : cgi.ToString();
                if (GlobalMethods.ValueIsNull(CourseSchdId).Length > 0)
                {
                    CourseSchdId = objcryptoJS.AES_decrypt(HttpUtility.UrlEncode(cgi), AppConstants.secretKey, AppConstants.initVec).ToString();
                }
                int intInstId = 0;

                #region Getting Instructor Id.
                clsCourseSchedule objCourseSch = new clsCourseSchedule();
                objCourseSch = CourseScheduleDAL.SelectCourseScheduleById(Convert.ToInt32(CourseSchdId));
                if(objCourseSch != null)
                {
                    intInstId = objCourseSch.InstructorId.HasValue ? objCourseSch.InstructorId.Value : 0;
                    CourseId = objCourseSch.TrainingCourseScheduleId.ToString();
                }
                #endregion

                CourseId = objcryptoJS.AES_encrypt(HttpUtility.UrlEncode(CourseId), AppConstants.secretKey, AppConstants.initVec).ToString();

                #region Adding to LK_Inst_CourseSchedule
                clsLK_Inst_CourseSchedule objInstCS = new clsLK_Inst_CourseSchedule();
                objInstCS.AuthorisedUserId = Convert.ToInt32(HttpContext.Current.Session["UserAuthId"]);
                objInstCS.TrainingCourseScheduleId = Convert.ToInt32(CourseSchdId);
                objInstCS.InstructorId = intInstId;
                objInstCS.TP_AuthorisedUserId = 0;
                objInstCS.IsApproved = 0;
                objInstCS.CreatedDate = DateTime.Now;
                objInstCS.ApprovedOn = Convert.ToDateTime("1/1/1900");
                if (!LK_Inst_CourseScheduleDAL.InsertLK_Inst_CourseSchedule(objInstCS))
                {  }
                #endregion

            }
            catch (Exception)
            {
                ErrorHandler.ErrorPage();
            }
            return "CourseDetails.aspx?dash=active&cgi="+ CourseId + "";
        }

        [System.Web.Services.WebMethod(EnableSession = false)]
        public static string Cancel(string cgi)
        {
            CryptoJS objcryptoJS = new CryptoJS();
            string strURL = string.Empty;
            string CourseSchdId = string.Empty;
            string CourseId = string.Empty;

            try
            {

                CourseSchdId = cgi.ToString() == null ? string.Empty : cgi.ToString();
                if (GlobalMethods.ValueIsNull(CourseSchdId).Length > 0)
                {
                    CourseSchdId = objcryptoJS.AES_decrypt(HttpUtility.UrlEncode(cgi), AppConstants.secretKey, AppConstants.initVec).ToString();
                }
                int intInstId = 0;

                #region Getting Instructor Id.
                clsCourseSchedule objCourseSch = new clsCourseSchedule();
                objCourseSch = CourseScheduleDAL.SelectCourseScheduleById(Convert.ToInt32(CourseSchdId));
                if (objCourseSch != null)
                {
                    intInstId = objCourseSch.InstructorId.HasValue ? objCourseSch.InstructorId.Value : 0;
                    CourseId = objCourseSch.TrainingCourseScheduleId.ToString();
                }
                #endregion

                CourseId = objcryptoJS.AES_encrypt(HttpUtility.UrlEncode(CourseId), AppConstants.secretKey, AppConstants.initVec).ToString();


                List<clsLK_Inst_CourseSchedule> objInstCS = new List<clsLK_Inst_CourseSchedule>();
                objInstCS = LK_Inst_CourseScheduleDAL.SelectDynamicLK_Inst_CourseSchedule("TrainingCourseScheduleId = " + CourseSchdId + " and AuthorisedUserId = " + HttpContext.Current.Session["UserAuthId"].ToString() + "", "Inst_CourseSchId");

                #region Adding to LK_Inst_CourseSchedule
                clsLK_Inst_CourseSchedule objCanCS = new clsLK_Inst_CourseSchedule();
                objCanCS.Inst_CourseSchId = objInstCS[0].Inst_CourseSchId;
               
                if (!LK_Inst_CourseScheduleDAL.DeleteLK_Inst_CourseSchedule(objCanCS.Inst_CourseSchId))
                { }
                #endregion

            }
            catch (Exception)
            {
                ErrorHandler.ErrorPage();
            }
            return "CourseDetails.aspx?dash=active&cgi=" + CourseId + "";
        }
    }
}