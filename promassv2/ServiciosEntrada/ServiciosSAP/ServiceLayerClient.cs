using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Common.VO;
using Newtonsoft.Json;
using System.IO;
using log4net;

namespace ServiciosSAP
{
    public class ServiceLayerClient
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ServiceLayerClient));

        public ServiceLayerClient()
        {
            ServicePointManager.ServerCertificateValidationCallback =
                (sender, cert, chain, sslPolicyErrors) => true;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.Expect100Continue = false;
        }

        private string Login(EntidadVO entidad)
        {
            var url = entidad.ServiceLayerUrl.TrimEnd('/') + "/Login";

            var body = new
            {
                CompanyDB = entidad.CompanyDB,
                UserName = entidad.SAPUser,
                Password = entidad.SAPPassword,
                Language = "23"
            };

            var json = JsonConvert.SerializeObject(body);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var cookies = response.Headers["Set-Cookie"];

            return cookies;
        }

        private void Logout(EntidadVO entidad, CookieContainer cookies)
        {
            try
            {
                var url = entidad.ServiceLayerUrl.TrimEnd('/') + "/Logout";

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.CookieContainer = cookies;
                request.Accept = "application/json";
                request.ProtocolVersion = HttpVersion.Version11;

                var response = (HttpWebResponse)request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var result = reader.ReadToEnd();
                }
            }
            catch
            {
                // no romper flujo
            }
        }

        public string Post(EntidadVO entidad, string endpoint, object data)
        {
            string cookieHeader = null;

            try
            {
                cookieHeader = Login(entidad);

                var url = entidad.ServiceLayerUrl.TrimEnd('/') + "/" + endpoint;

                var json = JsonConvert.SerializeObject(data);
                log.Info("Payload enviado a SAP: " + json);
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Accept = "application/json";

                request.Headers.Add("Cookie", cookieHeader);

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(json);
                }

                try
                {
                    var response = (HttpWebResponse)request.GetResponse();

                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var result = reader.ReadToEnd();
                        
                        return result;
                    }
                }
                catch (WebException ex)
                {
                    log.Error("Error en Service Layer (WebException)", ex);

                    if (ex.Response != null)
                    {
                        using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                        {
                            var error = reader.ReadToEnd();
                            log.Error("Error SAP response: " + error);
                            throw new Exception(error);
                        }
                    }
                    
                    throw;
                }
            }
            finally
            {
                try
                {
                    if (!string.IsNullOrEmpty(cookieHeader))
                    {
                        var logoutRequest = (HttpWebRequest)WebRequest.Create(
                            entidad.ServiceLayerUrl.TrimEnd('/') + "/Logout");

                        logoutRequest.Method = "POST";
                        logoutRequest.Headers.Add("Cookie", cookieHeader);

                        logoutRequest.GetResponse().Close();
                    }
                }
                catch { }
            }
        }
    }
}