using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using SAPConnector.Services;
using SAPConnector.VO;

namespace ServiciosEntrada
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class sessionpool
    {
        // Para usar HTTP GET, agregue el atributo [WebGet]. (El valor predeterminado de ResponseFormat es WebMessageFormat.Json)
        // Para crear una operación que devuelva XML,
        //     agregue [WebGet(ResponseFormat=WebMessageFormat.Xml)]
        //     e incluya la siguiente línea en el cuerpo de la operación:
        //         WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
        [OperationContract]
        [WebGet]
        public SessionMetricsVO status()
        {
            SessionMetricsVO metrics = SessionPool.GetMetrics();

            return metrics;
        }

        [OperationContract]
        [WebGet]
        public String refresh()
        {
            int sesionesMuertas = SessionPool.Refresh();

            return "Sesiones recuperadas: " + sesionesMuertas;
        }

        [OperationContract]
        [WebGet]
        public SessionMetricsVO reiniciar()
        {
            return SessionPool.Reiniciar();
        }

    } // sessionpool
}
