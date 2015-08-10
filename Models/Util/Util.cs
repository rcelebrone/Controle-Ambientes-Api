using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;
using System.Web.Hosting;

namespace TemGente.Models
{
    public class Util
    {
        /*
        * Propriedades publicas
        */
        public static string Alerta
        {
            get
            {
                return "Por favor, caso isso aconteça novamente, informe esse problema na pagina do aplicativo";
            }
        }
        public static string Base64LocalDefault
        {
            get
            {
                return "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";
            }
        }

        /*
        * Metodos staticos e publicos
        */
        public static string GerarHashMd5(string input)
        {
            MD5 md5Hash = MD5.Create();
            // Converter a String para array de bytes, que é como a biblioteca trabalha.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Cria-se um StringBuilder para recompôr a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop para formatar cada byte como uma String em hexadecimal
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        public static bool EnviaEmail(string para, string assunto, string conteudo)
        {
            var appEmail = "admin@temgente.com";
            var client = new SmtpClient();
            var mmsg = new MailMessage();

            client.UseDefaultCredentials = true;
            client.Credentials = new NetworkCredential(appEmail, "!Tem*0686");
            client.Host = "smtp.temgente.com";
            client.Port = 587;
            client.EnableSsl = false;

            mmsg.From = new MailAddress("admin@temgente.com");
            mmsg.To.Add(para);
            mmsg.Subject = assunto;
            mmsg.Body = conteudo;
            mmsg.IsBodyHtml = true;

            client.Send(mmsg);

            return true;
        }

        public static string Config(string nome) {
            return WebConfigurationManager.AppSettings[nome];
        }

        public static void LogError(Exception exception)
        {
            // You could use any logging approach here

            StringBuilder builder = new StringBuilder();

            string source = string.Empty;
            try
            {
                source = exception.Source;
            }
            catch (Exception)
            {
                source = "";
            }

            builder
                .AppendLine("----------")
                .AppendLine(DateTime.Now.ToString())
                .AppendFormat("Source:\t{0}", source)
                .AppendLine()
                .AppendFormat("Target:\t{0}", exception.TargetSite)
                .AppendLine()
                .AppendFormat("Type:\t{0}", exception.GetType().Name)
                .AppendLine()
                .AppendFormat("Message:\t{0}", exception.Message)
                .AppendLine()
                .AppendFormat("Stack:\t{0}", exception.StackTrace)
                .AppendLine();

            string filePath = HostingEnvironment.MapPath(Config("App:Root:Folder") + "App_Data/Error.log");

            try
            {
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.Write(builder.ToString());
                    writer.Flush();
                }

                EnviaEmail("rcelebrone@gmail.com", "Log Erro temgente", builder.ToString());
            }
            catch (Exception)
            {
                //apenas ignorar, quando cair aqui, é provavel que já exista um processo gravando algo no arquivo
            }
        }
    }
}