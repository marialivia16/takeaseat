using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace TakeASeatApp.Utils
{
	public enum AppMessageType : byte
	{
		Info = 1,
		Success = 2,
		Warning = 3,
		Danger = 4,
		Error = 5
	}

	public class AppMessage : List<string>
	{
		public AppMessageType Type;
		public bool IsEmpty { get { return this.Count == 0 ? true : false; } }
		public int Length { get { return this.Count; } }
		public string Title;
		public List<string> Buttons;
		public bool IsDismisable;
		public string CssClassButtonsWrapper;
		public string CssClass
		{
			get
			{
				if (string.IsNullOrEmpty(this._cssClass))
				{
					switch (this.Type)
					{
						case AppMessageType.Info: return "alert-info";
						case AppMessageType.Success: return "alert-success";
						case AppMessageType.Warning: return "alert-warning";
						case AppMessageType.Danger: return "alert-danger";
						case AppMessageType.Error: return "alert-danger";
						default: return this._cssClass;
					}
				}
				else return this._cssClass;
			}
			set
			{
				this._cssClass = value;
			}
		}

		public AppMessage()
		{
			this.Type = AppMessageType.Info;
			this.IsDismisable = true;
			this.CssClassButtonsWrapper = "Submit";
		}

		public AppMessage(AppMessageType messageType, bool isDismisable = true, string buttonWrapperCssClass = "Submit")
		{
			this.Type = messageType;
			this.IsDismisable = isDismisable;
			this.CssClassButtonsWrapper = buttonWrapperCssClass;
		}

		public int AppendMessage(string message)
		{
			this.Add(string.Format("<p>{0}</p>", message));
			return this.Count;
		}
		public int AppendRaw(object rawObject)
		{
			this.Add(rawObject.ToString());
			return this.Count;
		}
		/// <summary>
		/// Appends a link button <a class='Button [buttonClass]'> inside a <div class='[Submit]'> to the list of messages
		/// </summary>
		/// <param name="linkButtonText"></param>
		/// <param name="linkButtonUrl"></param>
		/// <param name="buttonClass"></param>
		/// <returns></returns>
		public int AppendLinkButton(string linkButtonText, string linkButtonUrl, string buttonClass = "btn-primary")
		{
			if (this.Buttons == null) this.Buttons = new List<string>();
			this.Buttons.Add(string.Format("<a href='{0}' class='btn btn-sm {1}'>{2}</a>", linkButtonUrl, buttonClass, linkButtonText));
			return this.Buttons.Count;
		}
		/// <summary>
		/// Appends a simple link <a> inside a <div class='...'> to the list of messages
		/// </summary>
		/// <param name="linkButtonText"></param>
		/// <param name="linkButtonUrl"></param>
		/// <param name="buttonClass"></param>
		/// <returns></returns>
		public int AppendLink(string linkText, string linkUrl, string linkClass = "btn btn-sm btn-default")
		{
			if (this.Buttons == null) this.Buttons = new List<string>();
			this.Buttons.Add(string.Format("<a href='{0}' class='{1}'>{2}</a>", linkUrl, linkClass, linkText));
			return this.Buttons.Count;
		}

		public string GetString()
		{
			if (this.Count == 0) return null;
			StringBuilder stb = new StringBuilder();
			foreach (string msg in this) stb.Append(msg);
			return stb.ToString();
		}
		public string ToHtml()
		{
			if (string.IsNullOrEmpty(this.Title))
			{
				switch (this.Type)
				{
					case AppMessageType.Success: this.Title = _strSuccess; break;
					case AppMessageType.Error: this.Title = _strError; break;
					case AppMessageType.Warning: this.Title = _strWarning; break;
					case AppMessageType.Danger: this.Title = _strDanger; break;
					default: this.Title = _strInfo; break;
				}
			}
			return this.ToHtml(this.Title);
		}
		public string ToHtml(string title)
		{
			if (this.Count == 0) return null;
			StringBuilder stb = new StringBuilder();
			stb.Append(string.Format("<div class='alert {0}'>", this.CssClass));
			if (this.IsDismisable) stb.Append("<a class='close' data-dismiss='alert'>&times;</a>");
			if (!string.IsNullOrEmpty(title)) stb.Append(string.Format("<h4>{0}</h4>", title));
			foreach (string msg in this) stb.Append(msg);
			if (this.Buttons != null && this.Buttons.Count > 0)
			{
				stb.Append(string.Format("<div class='{0}'>", this.CssClassButtonsWrapper));
				foreach (string msg in this.Buttons)
				{
					stb.Append(msg);
				}
				stb.Append("</div>");
			}
			stb.Append("</div>");
			return stb.ToString();
		}
		public string ToHtmlSuccess()
		{
			this.Type = AppMessageType.Success;
			return this.ToHtml(string.IsNullOrEmpty(this.Title) ? _strSuccess : this.Title);
		}
		public string ToHtmlSuccess(string title)
		{
			this.Type = AppMessageType.Success;
			return this.ToHtml(title);
		}
		public string ToHtmlWarning()
		{
			this.Type = AppMessageType.Warning;
			return this.ToHtml(string.IsNullOrEmpty(this.Title) ? _strWarning : this.Title);
		}
		public string ToHtmlWarning(string title)
		{
			this.Type = AppMessageType.Warning;
			return this.ToHtml(title);
		}
		public string ToHtmlDanger()
		{
			this.Type = AppMessageType.Danger;
			return this.ToHtml(string.IsNullOrEmpty(this.Title) ? _strDanger : this.Title);
		}
		public string ToHtmlDanger(string title)
		{
			this.Type = AppMessageType.Danger;
			return this.ToHtml(title);
		}
		public string ToHtmlError()
		{
			this.Type = AppMessageType.Error;
			return this.ToHtml(string.IsNullOrEmpty(this.Title) ? _strError : this.Title);
		}
		public string ToHtmlError(string title)
		{
			this.Type = AppMessageType.Error;
			return this.ToHtml(title);
		}
		public string ToHtmlError(ModelStateDictionary modelStateContainer, string title = null)
		{
			this.Type = AppMessageType.Error;
			foreach (string key in modelStateContainer.Keys)
			{
				ModelState modelState = modelStateContainer[key];
				foreach (ModelError modelError in modelState.Errors)
				{
					this.AppendMessage(modelError.ErrorMessage);
				}
			}
			return this.ToHtml(title);
		}

		private string _cssClass;
		private static string _strSuccess = Strings.AppMessage_Success;
		private static string _strError = Strings.AppMessage_Error;
		private static string _strWarning = Strings.AppMessage_Warning;
		private static string _strDanger = Strings.AppMessage_Danger;
		private static string _strInfo = Strings.AppMessage_Info;
	}
}