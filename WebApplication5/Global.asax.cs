using BusinessLayer1.Helpers;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebApplication5
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Error()
                .WriteTo.Async(a => a.File(
                    @"D:\C#Practice\StudentManagementSystem\logs\enrollment-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30))
                .WriteTo.MSSqlServer(
                    connectionString: "Data Source=VPNSERVER1\\SQLEXPRESS;Initial Catalog=Training_DB_Harsh_Patil;User ID=Training_DB_Harsh_Patil;Password=Training_DB_Harsh_Patil",
                    restrictedToMinimumLevel: LogEventLevel.Error,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "Logs",
                        AutoCreateSqlTable = false
                    })
                .CreateLogger();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_End()
        {
            Log.CloseAndFlush();
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var cookie = Context.Request.Cookies["jwt_token"];
            if (cookie != null)
            {
                try
                {
                    var principal = JwtHelper.ValidateToken(cookie.Value);
                    Context.User = principal;
                }
                catch { }
            }
        }

        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            if (ex != null)
                Log.Error(ex, "Unhandled exception caught in Application_Error");
        }
    }
}
