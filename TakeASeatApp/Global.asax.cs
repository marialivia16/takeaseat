using System.Data.SqlClient;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TakeASeatApp.Data;
using TakeASeatApp.Models;

namespace TakeASeatApp
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			App.IsInstalled = false;
            
			#region TEST IF THE DATABASE IS SET UP (with schemas, tables, indexes, functions and stored procedures)
			Repository db = new Repository();
			SqlDataReader rdrReader = db.GetSqlDataReaderFromTextCommand("SELECT * FROM Sys.Objects WHERE [Type] = 'U' AND SCHEMA_NAME(SCHEMA_ID) = N'App' AND [Name] = 'Settings'", false);
			if (rdrReader.Read())
			{
				App.IsInstalled = true;
			}
			rdrReader.Close();
			rdrReader.Dispose();
			db.Dispose();
			#endregion

			//if (App.IsInstalled) App.LoadSettings();
        }
    }
}
