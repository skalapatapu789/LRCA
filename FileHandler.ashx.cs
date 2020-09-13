using LRCA.classes;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Linq;
using LRCA.classes.Models;
using LRCA.classes.Repositories;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LRCA
{
	/// <summary>
	/// Summary description for FileHandler
	/// </summary>
	public class FileHandler : IHttpHandler, IRequiresSessionState
	{
		CryptoJS objcryptoJS = new CryptoJS();
		public readonly IUnitOfWork _uow;
		public readonly IGroupDataContext _context;
		private readonly IAuditor _auditor;
		private readonly ITPRepository _tPRepository;
		private readonly IContractorRepository _contractorRepository;
        private readonly IRiskAssessorRepository _riskAssessorRepository;
        private readonly ISupervisorRepository _supervisorRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly ITCRepository _tcRepository;
        public FileHandler()
		{
			_context = new GroupDbContext();
			_auditor = new Auditor();
			_uow = new UnitOfWork(_context, _auditor);
			_tPRepository = new TPRepository(_context);
			_contractorRepository = new ContractorRepository(_context);
            _riskAssessorRepository = new RiskAssessorRepository(_context);
            _supervisorRepository = new SupervisorRepository(_context);
            _instructorRepository = new InstructorRepository(_context);
            _tcRepository = new TCRepository(_context);
        }
		public void ProcessRequest(HttpContext context)
		{
			if (!context.User.Identity.IsAuthenticated)
			{
				FormsAuthentication.RedirectToLoginPage();
				return;
			}
			if (context.Request.Url.AbsoluteUri.IndexOf(".pdf") > -1)
			{
				using (var stream = new MemoryStream())
				{
					var url = objcryptoJS.AES_decrypt(context.Request.Url.AbsoluteUri.Split('/').Last().Replace(".pdf", ""), AppConstants.secretKey, AppConstants.initVec);
					var fileName = url;
					using (FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uf", url + ".pdf"), FileMode.Create))
					{
						stream.CopyTo(fs);
					}
					context.Response.BinaryWrite(stream.ToArray());
				}
			}
			else if (context.Request.Url.AbsoluteUri.IndexOf(".cert") > -1)
			{
				var userId = HttpContext.Current.Session["UserAuthId"];
				var values = objcryptoJS.AES_decrypt(context.Request.Url.AbsoluteUri.Split('/').Last().Replace(".cert", ""), AppConstants.secretKey, AppConstants.initVec).Split('_');
				var template = string.Empty;
				var roleId = string.Empty;
				var id = string.Empty;
				if (values.Length == 2)
				{
					template = values[0];
					id = values[1];
				}
				if (values.Length == 3)
				{
					template = values[0];
					roleId = values[1];
					id = values[2];
				}
				if (values.Length == 4)
				{
					template = values[0] + "_" + values[1];
					roleId = values[2];
					id = values[3];
				}
				var content = new StringBuilder(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_data", template + ".htm")));
				if (template == "Acct_Certificate")
				{
					var data = default(AccreditationResult);
					switch (roleId)
					{
						case "2":
							data = _tPRepository.GetAccreditationById(roleId, id);
							break;
						case "7":
							data = _contractorRepository.GetAccreditationById(roleId, id);
							break;
                        case "15":
                            data = _riskAssessorRepository.GetAccreditationById(roleId, id);
                            break;
                        case "5":
                            data = _supervisorRepository.GetAccreditationById(roleId, id);
                            break;
                        case "3":
                            data = _instructorRepository.GetAccreditationById(roleId, id);
                            break;
                        case "17":
                            data = _tcRepository.GetAccreditationById(roleId, id);
                            break;
                        default:
							break;
					}

					content.Replace("{{Name}}", ConvertTextToBase64Image("				" + data.Name, "Arial", 14, Color.White, Color.Black, 446, 62));
					content.Replace("{{CourseName}}", ConvertTextToBase64Image("		" + data.CourseName, "Arial", 14, Color.White, Color.Black, 758, 62));
					content.Replace("{{ExpDate}}", ConvertTextToBase64Image(data.ExpDate, "Arial", 18, Color.White, Color.Black, 206, 38));
					content.Replace("{{TPName}}", ConvertTextToBase64Image(data.TP_Name, "Arial", 18, Color.White, Color.Black, 206, 56));
					content.Replace("{{CourseDate}}", ConvertTextToBase64Image(data.CourseDate, "Arial", 18, Color.White, Color.Black, 236, 32));
					content.Replace("{{Number}}", ConvertTextToBase64Image(data.Number.ToString(), "Arial", 18, Color.White, Color.Black, 194, 34));
					content.Replace("{{Date}}", ConvertTextToBase64Image(data.Date, "Arial", 18, Color.White, Color.Black, 106, 48));
					content.Replace("{{Empty}}", ConvertTextToBase64Image("				", "Arial", 18, Color.White, Color.Black, 262, 32));
				}
				else if (template == "TrainingCard")
				{
					var data = default(TrainingCardResult);
					data = _tPRepository.GetTrainingCardById(id);
					content.Replace("{{Name}}", ConvertTextToBase64Image(data.Name, "Arial", 18, Color.White, Color.Black, 262, 32));
					content.Replace("{{ClassCode}}", ConvertTextToBase64Image(data.ClassCode, "Arial", 18, Color.White, Color.Black, 262, 32));
					content.Replace("{{DOB}}", ConvertTextToBase64Image(data.DOB, "Arial", 18, Color.White, Color.Black, 262, 32));
					content.Replace("{{ProviderName}}", ConvertTextToBase64Image(data.ProviderName, "Arial", 18, Color.White, Color.Black, 262, 32));
					content.Replace("{{ExpDate}}", ConvertTextToBase64Image(data.ExpDate, "Arial", 18, Color.White, Color.Black, 262, 32));
					content.Replace("{{Number}}", ConvertTextToBase64Image(data.TrainingCardId.ToString(), "Arial", 18, Color.White, Color.Black, 262, 32));
				}
				var fileName = Guid.NewGuid();
				context.Response.Headers.Add("Content-Type", "text/pdf");
				context.Response.Headers.Add("Content-Disposition", "attachment; filename=" + objcryptoJS.AES_encrypt(userId.ToString(), AppConstants.secretKey, AppConstants.initVec) + "_" + template + ".pdf");
				using (var stream = new MemoryStream())
				{
					new PdfConverter().Convert(stream, content.ToString());
					var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Attachments");
					var dir = new DirectoryInfo(path);
					if (!dir.Exists)
						dir.Create();
					using (FileStream fs = new FileStream(string.Format("{0}\\{1}.pdf", path, fileName), FileMode.Create))
					{
						stream.CopyTo(fs);
					}
					context.Response.BinaryWrite(stream.ToArray());
					stream.Flush();

				}
			}

		}
		private readonly Random _random = new Random();
		public int RandomNumber(int min, int max)
		{
			return _random.Next(min, max);
		}
		public string ConvertTextToBase64Image(string txt, string fontname, int fontsize, Color bgcolor, Color fcolor, int width, int Height)
		{
			var result = string.Empty;
			Bitmap bmp = new Bitmap(width, Height);
			using (var ms = new MemoryStream())
			using (Graphics graphics = Graphics.FromImage(bmp))
			{

				Font font = new Font(fontname, fontsize);
                //graphics.FillRectangle(new SolidBrush(bgcolor), 0, 0, bmp.Width, bmp.Height);
                graphics.FillRectangle(new SolidBrush(bgcolor), 0, 0, bmp.Width, bmp.Height);
                graphics.DrawString(txt, font, new SolidBrush(fcolor), 0, 0);
				graphics.Flush();
				font.Dispose();
				graphics.Dispose();
				bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
				result = Convert.ToBase64String(ms.GetBuffer()); //Get Base64


			}
			return result;
		}

		#region GetPlaceHolders
		private IEnumerable<string> GetPlaceHolders(string template)
		{
			var result = new HashSet<string>();
			var markup = template;
			if (markup.Length > 0)
			{
				var regex = new Regex(@"\{{(.*?)\}}", RegexOptions.IgnorePatternWhitespace);
				var matches = regex.Matches(markup);
				foreach (Match each in matches)
				{
					result.Add(each.Value);
				}
			}
			return result;
		}
		#endregion
		public bool IsReusable
		{
			get
			{
				return false;
			}
		}
	}
}