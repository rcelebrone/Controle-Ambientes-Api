using System;
using System.IO;
using System.Text;
using System.Web.Mvc;
using TemGente.Models;

namespace TemGente.Controllers
{
    public class ErrorLoggerAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            LogError(filterContext);
            base.OnException(filterContext);
        }

        public void LogError(ExceptionContext filterContext)
        {
            // You could use any logging approach here

            StringBuilder builder = new StringBuilder();

            string source = string.Empty;
            try{
                source = filterContext.Exception.Source;
            }catch(Exception){
                source = "";
            }

            builder
                .AppendLine("----------")
                .AppendLine(DateTime.Now.ToString())
                .AppendFormat("Source:\t{0}", source)
                .AppendLine()
                .AppendFormat("Target:\t{0}", filterContext.Exception.TargetSite)
                .AppendLine()
                .AppendFormat("Type:\t{0}", filterContext.Exception.GetType().Name)
                .AppendLine()
                .AppendFormat("Message:\t{0}", filterContext.Exception.Message)
                .AppendLine()
                .AppendFormat("Stack:\t{0}", filterContext.Exception.StackTrace)
                .AppendLine();

            string filePath = filterContext.HttpContext.Server.MapPath(Util.Config("App:Root:Folder") + "App_Data/Error.log");

            try
            {
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.Write(builder.ToString());
                    writer.Flush();
                }
            }
            catch (Exception)
            {
                //apenas ignorar, quando cair aqui, é provavel que já exista um processo gravando algo no arquivo
            }
        }
    }
}