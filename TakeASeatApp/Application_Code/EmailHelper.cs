using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace TakeASeatApp.Utils
{
	public class EmailHelper
	{

		public static string EmailAddressRegularExpression = @"^(?("")(""[^""]+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,17}))$";
		public static string ApplicationUrl
		{
			get
			{
				if (_applicationUrl == null)
				{
					_applicationUrl = new StringBuilder();
					_applicationUrl.Append(HttpContext.Current.Request.Url.AbsoluteUri.Substring(0, HttpContext.Current.Request.Url.AbsoluteUri.IndexOf(':') + 3));
					_applicationUrl.Append(HttpContext.Current.Request.Url.Host);
					_applicationUrl.Append(HttpContext.Current.Request.Url.Port == 80 ? string.Empty : ":" + HttpContext.Current.Request.Url.Port.ToString());
					_applicationUrl.Append(HttpContext.Current.Request.ApplicationPath);
				}
				return _applicationUrl.ToString();
			}
		}

		public bool IsValidEmailAddress(string emailAddressToTest)
		{

			_invalid = false;
			if (string.IsNullOrEmpty(emailAddressToTest)) return false;

			// Use IdnMapping class to convert Unicode domain names. 
			try
			{
				emailAddressToTest = Regex.Replace(emailAddressToTest, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));
			}
			catch (RegexMatchTimeoutException)
			{
				return false;
			}
			if (_invalid) return false;

			// Return true if strIn is in valid e-mail format. 
			try
			{
				return Regex.IsMatch(emailAddressToTest, EmailAddressRegularExpression, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
			}
			catch (RegexMatchTimeoutException)
			{
				return false;
			}
		}

		/// <summary>
		/// Sends an e-mail message based on a template html code that can be customized with 'ReplaceStringsInTemplate'. 
		/// </summary>
		/// <param name="templateHtmlCode">The template html code must be a valid XHTML document. The Subject line will be taken from TITLE tag.</param>
		/// <param name="toAddresses"></param>
		/// <param name="ccAddresses"></param>
		/// <param name="bccAddresses"></param>
		/// <param name="replaceStringsInTemplate"></param>
		/// <returns>Returns a list of errors; if empty, everything was successfull.</returns>
		public List<string> SendEmailMessage(StringBuilder templateHtmlCode, string[] toAddresses, string[] ccAddresses = null, string[] bccAddresses = null, Dictionary<string, string> replaceStringsInTemplate = null)
		{
			List<string> errorsList = new List<string>();
			if (toAddresses == null || toAddresses.Length == 0) throw new Exception("'ToAddresses' is null or empty. 'TakeASeatApp.Email.SendEmailMessage(...)' requires at least a TO address for sending an e-mail.");
			if (templateHtmlCode == null || templateHtmlCode.Length < 10) throw new Exception("'TemplateHtmlCode' is null or appears empty. 'TakeASeatApp.Email.SendEmailMessage(...)' a template code for sending an e-mail.");
			SmtpClient theSmtpEmailService = new SmtpClient();
			MailMessage emailMessage = new MailMessage();
			emailMessage.SubjectEncoding = Encoding.UTF8;
			emailMessage.IsBodyHtml = true;
			emailMessage.BodyEncoding = Encoding.UTF8;
			#region populate e-mail addresses
			//theEmailMessage.From = new MailAddress("No-Reply@TakeASeatApp.ro");
			foreach (string addr in toAddresses)
			{
				if (IsValidEmailAddress(addr))
				{
					emailMessage.To.Add(new MailAddress(addr));
				}
			}
			if (ccAddresses != null)
			{
				foreach (string addr in ccAddresses)
				{
					if (IsValidEmailAddress(addr))
					{
						emailMessage.CC.Add(new MailAddress(addr));
					}
				}
			}
			if (bccAddresses != null)
			{
				foreach (string addr in bccAddresses)
				{
					if (IsValidEmailAddress(addr))
					{
						emailMessage.Bcc.Add(new MailAddress(addr));
					}
				}
			}
			if (emailMessage.To.Count == 0) throw new Exception("No valid 'ToAddresses' is null or empty. 'TakeASeatApp.Email.SendEmailMessage(...)' requires at least a TO address for sending an e-mail.");
			#endregion
			templateHtmlCode.Replace("[#_Application_Full_URL_#]", ApplicationUrl);
			if (replaceStringsInTemplate != null)
			{
				foreach (string key in replaceStringsInTemplate.Keys)
				{
					templateHtmlCode.Replace(key, replaceStringsInTemplate[key]);
				}
			}
			emailMessage.Body = templateHtmlCode.ToString();
			emailMessage.Subject = Strings.EmailHelper_SendEmailMessage_NoSubject;
			int subjectLineStartIndex = emailMessage.Body.IndexOf("<title>", StringComparison.Ordinal) + ("<title>").Length;
			int subjectLineFinishIndex = emailMessage.Body.IndexOf("</title>", StringComparison.Ordinal);
			if (subjectLineStartIndex > 7 && subjectLineFinishIndex > subjectLineStartIndex)
			{
				emailMessage.Subject = emailMessage.Body.Substring(subjectLineStartIndex, subjectLineFinishIndex - subjectLineStartIndex);
			}
			try
			{
				theSmtpEmailService.Send(emailMessage);
			}
			catch (Exception patanie)
			{
				errorsList.Add(Strings.EmailHelper_SendEmailMessage_Failed);
				errorsList.Add("<i>" + patanie.Message + "</i>");
			}
			emailMessage.Dispose();
			theSmtpEmailService.Dispose();
			return errorsList;
		}
		/// <summary>
		/// Sends an e-mail message based on a template file that can be customized with 'ReplaceStringsInTemplate'.
		/// </summary>
		/// <param name="templateFileLocation">The relative path of a file containing a valid XHTML document. The Subject line will be taken from TITLE tag.</param>
		/// <param name="toAddresses"></param>
		/// <param name="ccAddresses"></param>
		/// <param name="bccAddresses"></param>
		/// <param name="replaceStringsInTemplate"></param>
		/// <returns>Returns a list of errors; if empty, everything was successfull.</returns>
		public List<string> SendEmailMessage(string templateFileLocation, string[] toAddresses, string[] ccAddresses = null, string[] bccAddresses = null, Dictionary<string, string> replaceStringsInTemplate = null)
		{
			if (string.IsNullOrEmpty(templateFileLocation)) throw new Exception("'TemplateFileLocation' is null or empty. 'TakeASeatApp.Email.SendEmailMessage(...)' needs an e-mail template file to send the e-mail.");
			FileInfo templateFileInfo = new FileInfo(HttpContext.Current.Server.MapPath(templateFileLocation));
			if (!templateFileInfo.Exists) throw new Exception("File does not exist. The relative path referenced by 'TemplateFileLocation' when calling 'TakeASeatApp.Email.SendEmailMessage(...)' does not point to a physical file.");
			StreamReader templateFileStreamReader = new StreamReader(templateFileInfo.FullName);
			StringBuilder templateHtmlCode = new StringBuilder(templateFileStreamReader.ReadToEnd());
			templateFileStreamReader.Close();
			templateFileStreamReader.Dispose();
			return SendEmailMessage(templateHtmlCode, toAddresses, ccAddresses, bccAddresses, replaceStringsInTemplate);
		}
		/// <summary>
		/// Sends an e-mail message based on a template file that is injected into a master layout file. The message can be customized with 'ReplaceStringsInTemplate'. 
		/// </summary>
		/// <param name="masterLayoutFileLocation">The relative path of a file containing a valid XHTML document. Must contain an [#_Content_Placeholder_#] and an [#_Title_Placeholder_#]. Its TITLE tag will be replaced with the TITLE tag from MessageContentFile.</param>
		/// <param name="messageContentFileLocation">The relative path of a file containing a valid XHTML document. The Subject line will be taken from TITLE tag.</param>
		/// <param name="toAddresses"></param>
		/// <param name="ccAddresses"></param>
		/// <param name="bccAddresses"></param>
		/// <param name="replaceStringsInTemplate"></param>
		/// <returns>Returns a list of errors; if empty, everything was successfull.</returns>
		public List<string> SendEmailMessage(string masterLayoutFileLocation, string messageContentFileLocation, string[] toAddresses, string[] ccAddresses = null, string[] bccAddresses = null, Dictionary<string, string> replaceStringsInTemplate = null)
		{
			List<string> retVal = new List<string>();
			if (string.IsNullOrEmpty(masterLayoutFileLocation)) throw new Exception("'MasterLayoutFileLocation' is null or empty. 'TakeASeatApp.Email.SendEmailMessage(...)' needs an e-mail master layout file to send the e-mail.");
			if (string.IsNullOrEmpty(messageContentFileLocation)) throw new Exception("'MessageContentFileLocation' is null or empty. 'TakeASeatApp.Email.SendEmailMessage(...)' needs an e-mail message content template file to send the e-mail.");
			FileInfo masterLayoutFileInfo = new FileInfo(HttpContext.Current.Server.MapPath(masterLayoutFileLocation));
			FileInfo messageContentFileInfo = new FileInfo(HttpContext.Current.Server.MapPath(messageContentFileLocation));
			if (!masterLayoutFileInfo.Exists) throw new Exception("File does not exist. The relative path referenced by 'MasterLayoutFileLocation' when calling 'TakeASeatApp.Email.SendEmailMessage(...)' does not point to a physical file.");
			if (!messageContentFileInfo.Exists) throw new Exception("File does not exist. The relative path referenced by 'MessageContentFileLocation' when calling 'TakeASeatApp.Email.SendEmailMessage(...)' does not point to a physical file.");
			StreamReader masterLayoutFileStreamReader = new StreamReader(masterLayoutFileInfo.FullName);
			string masterLayoutHtmlCode = masterLayoutFileStreamReader.ReadToEnd();
			masterLayoutFileStreamReader.Close();
			masterLayoutFileStreamReader.Dispose();
			StreamReader messageContentFileStreamReader = new StreamReader(messageContentFileInfo.FullName);
			string messageContentHtmlCode = messageContentFileStreamReader.ReadToEnd();
			messageContentFileStreamReader.Close();
			messageContentFileStreamReader.Dispose();

			StringBuilder templateHtmlCode = new StringBuilder(masterLayoutHtmlCode);
		    int stringStartIndex = messageContentHtmlCode.IndexOf("<title>", StringComparison.Ordinal) + ("<title>").Length;
			int stringFinishIndex = messageContentHtmlCode.IndexOf("</title>", StringComparison.Ordinal);
			if (stringStartIndex > 7 && stringFinishIndex > stringStartIndex)
			{
				string subjectLine = messageContentHtmlCode.Substring(stringStartIndex, stringFinishIndex - stringStartIndex);
				templateHtmlCode.Replace("[#_Title_Placeholder_#]", subjectLine);

				stringStartIndex = messageContentHtmlCode.IndexOf("<title>", StringComparison.Ordinal);
				stringFinishIndex = messageContentHtmlCode.IndexOf("</title>", StringComparison.Ordinal) + ("</title>").Length;
				string messageContentTitleElement = messageContentHtmlCode.Substring(stringStartIndex, stringFinishIndex - stringStartIndex);
				stringStartIndex = masterLayoutHtmlCode.IndexOf("<title>", StringComparison.Ordinal);
				stringFinishIndex = masterLayoutHtmlCode.IndexOf("</title>", StringComparison.Ordinal) + ("</title>").Length;
				if (stringStartIndex > 0 && stringFinishIndex > stringStartIndex)
				{
				    string masterLayoutTitleElement = masterLayoutHtmlCode.Substring(stringStartIndex, stringFinishIndex - stringStartIndex);
				    templateHtmlCode.Replace(masterLayoutTitleElement, messageContentTitleElement);
				}
			}
			stringStartIndex = messageContentHtmlCode.IndexOf("<body>", StringComparison.Ordinal) + ("<body>").Length;
			stringFinishIndex = messageContentHtmlCode.IndexOf("</body>", StringComparison.Ordinal);
			if (stringStartIndex > 6 && stringFinishIndex > stringStartIndex)
			{
			    string messageContent = messageContentHtmlCode.Substring(stringStartIndex, stringFinishIndex - stringStartIndex);
			    templateHtmlCode.Replace("[#_Content_Placeholder_#]", messageContent);
			}
		    return SendEmailMessage(templateHtmlCode, toAddresses, ccAddresses, bccAddresses, replaceStringsInTemplate);
		}


		private string DomainMapper(Match match)
		{
			// IdnMapping class with default property values.
			IdnMapping idn = new IdnMapping();
			string domainName = match.Groups[2].Value;
			try
			{
				domainName = idn.GetAscii(domainName);
			}
			catch (ArgumentException)
			{
				_invalid = true;
			}
			return match.Groups[1].Value + domainName;
		}

		private bool _invalid = false;
		private static StringBuilder _applicationUrl = null;
	}
}