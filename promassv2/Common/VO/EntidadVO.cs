using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Common.VO
{
    public class EntidadVO
    {
        public static EntidadVO EntidadActual = null;

        public string Id = "";
        public string ConnectionString = "";
        public string CompanyDB = "";
        public string SAPPassword = "";
        public string RFC = "";
        public bool IsServiciosSalida = true;
        public bool IsServiciosEntrada = true;

        private static Dictionary<string, EntidadVO> entidadesList = null;

        public static Dictionary<string, EntidadVO> getEntidades()
        {
            if (entidadesList == null)
            {
                entidadesList = new Dictionary<string, EntidadVO>();
                EntidadVO entidadVO = null;

                // Produccion

                entidadVO = new EntidadVO();
                entidadVO.Id = "10";
                entidadVO.CompanyDB = "CAS";
                entidadVO.ConnectionString = "Server=promass-saphana:30015;CURRENTSCHEMA=CAS;UserID=SYSTEM;Password=Promass2017;Application Name=InterfasesSAP";
                entidadVO.RFC = "CAS981016P46";
                entidadVO.SAPPassword = ConfigurationManager.AppSettings["SAPPassword_" + entidadVO.Id];
                entidadesList.Add(entidadVO.Id, entidadVO);

                entidadVO = new EntidadVO();
                entidadVO.Id = "46";
                entidadVO.CompanyDB = "H_B";
                entidadVO.ConnectionString = "Server=promass-saphana:30015;CURRENTSCHEMA=H_B;UserID=SYSTEM;Password=Promass2017;Application Name=InterfasesSAP";
                entidadVO.RFC = "ARD041119HY8";
                entidadVO.SAPPassword = ConfigurationManager.AppSettings["SAPPassword_" + entidadVO.Id];
                entidadesList.Add(entidadVO.Id, entidadVO);

                entidadVO = new EntidadVO();
                entidadVO.Id = "50";
                entidadVO.CompanyDB = "VALUAD";
                entidadVO.ConnectionString = "Server=promass-saphana:30015;CURRENTSCHEMA=VALUAD;UserID=SYSTEM;Password=Promass2017;Application Name=InterfasesSAP";
                entidadVO.RFC = "VMC1211238F1";
                entidadVO.SAPPassword = ConfigurationManager.AppSettings["SAPPassword_" + entidadVO.Id];
                entidadesList.Add(entidadVO.Id, entidadVO);

                entidadVO = new EntidadVO();
                entidadVO.Id = "60";
                entidadVO.CompanyDB = "ASISVIAL";
                entidadVO.ConnectionString = "Server=promass-saphana:30015;CURRENTSCHEMA=ASISVIAL;UserID=SYSTEM;Password=Promass2017;Application Name=InterfasesSAP";
                entidadVO.RFC = "ASI040130SK7";
                entidadVO.SAPPassword = ConfigurationManager.AppSettings["SAPPassword_" + entidadVO.Id];
                entidadesList.Add(entidadVO.Id, entidadVO);

                entidadVO = new EntidadVO();
                entidadVO.Id = "65";
                entidadVO.CompanyDB = "PROTEC";
                entidadVO.ConnectionString = "Server=promass-saphana:30015;CURRENTSCHEMA=PROTEC;UserID=SYSTEM;Password=Promass2017;Application Name=InterfasesSAP";
                entidadVO.RFC = "PTP091002HX6";
                entidadVO.SAPPassword = ConfigurationManager.AppSettings["SAPPassword_" + entidadVO.Id];
                entidadesList.Add(entidadVO.Id, entidadVO);


                //// Aparentemente ya no existe para el desarrollo (fecha: 27/02/2026) pero en lugar de borrar mejor la dejamos 
                //// comentado, falta su credenciales de SAP en el web.config del "Servicios Entrada"
                ////entidadVO = new EntidadVO();
                ////entidadVO.Id = "70";
                ////entidadVO.CompanyDB = "BESTEAM";
                ////entidadVO.ConnectionString = "Server=promass-saphana:30015;CURRENTSCHEMA=BESTEAM;UserID=SYSTEM;Password=Promass2017;Application Name=InterfasesSAP";
                ////entidadVO.RFC = "BAH170718GK8";
                ////entidadVO.SAPPassword = ConfigurationManager.AppSettings["SAPPassword_" + entidadVO.Id];
                ////entidadesList.Add(entidadVO.Id, entidadVO);

                entidadVO = new EntidadVO();
                entidadVO.Id = "75";
                entidadVO.CompanyDB = "WOW_DB";
                entidadVO.ConnectionString = "Server=promass-saphana:30015;CURRENTSCHEMA=WOW_DB;UserID=SYSTEM;Password=Promass2017;Application Name=InterfasesSAP";
                entidadVO.RFC = "TBW200623FC4";
                entidadVO.SAPPassword = ConfigurationManager.AppSettings["SAPPassword_" + entidadVO.Id];
                entidadVO.IsServiciosSalida = false;
                entidadesList.Add(entidadVO.Id, entidadVO);

                entidadVO = new EntidadVO();
                entidadVO.Id = "80";
                entidadVO.CompanyDB = "GRUPO_PROMASS";
                entidadVO.ConnectionString = "Server=promass-saphana:30015;CURRENTSCHEMA=GRUPO_PROMASS;UserID=SYSTEM;Password=Promass2017;Application Name=InterfasesSAP";
                entidadVO.RFC = "GPM070122BL8";
                entidadVO.SAPPassword = ConfigurationManager.AppSettings["SAPPassword_" + entidadVO.Id];
                entidadVO.IsServiciosSalida = false;
                entidadesList.Add(entidadVO.Id, entidadVO);

                // Pruebas
                //entidadVO = new EntidadVO();
                //entidadVO.Id = "99";
                //entidadVO.ConnectionString = "Server=promass-saphana:30015;CURRENTSCHEMA=CAS_PRUEBAS;UserID=SYSTEM;Password=Promass2017;Application Name=InterfasesSAP";
                //entidadVO.CompanyDB = "CAS_PRUEBAS";
                //entidadVO.RFC = "CAS981016P46";
                //entidadVO.SAPPassword = ConfigurationManager.AppSettings["SAPPassword_" + entidadVO.Id];
                //entidadesList.Add(entidadVO.Id, entidadVO);

                //entidadVO = new EntidadVO();
                //entidadVO.Id = "200";
                //entidadVO.ConnectionString = "Server=promass-saphana:30015;CURRENTSCHEMA=PRUEBAS_GPO;UserID=SYSTEM;Password=Promass2017;Application Name=InterfasesSAP";
                //entidadVO.CompanyDB = "PRUEBAS_GPO";
                //entidadVO.RFC = "GPM070122BL8";
                //entidadVO.SAPPassword = ConfigurationManager.AppSettings["SAPPassword_" + entidadVO.Id];
                //entidadesList.Add(entidadVO.Id, entidadVO);

                //entidadVO = new EntidadVO();
                //entidadVO.Id = "201";
                //entidadVO.ConnectionString = "Server=promass-saphana:30015;CURRENTSCHEMA=PRUEBAS_WOW;UserID=SYSTEM;Password=Promass2017;Application Name=InterfasesSAP";
                //entidadVO.CompanyDB = "PRUEBAS_WOW";
                //entidadVO.RFC = "TBW200623FC4";
                //entidadVO.SAPPassword = ConfigurationManager.AppSettings["SAPPassword_" + entidadVO.Id];
                //entidadesList.Add(entidadVO.Id, entidadVO);

                //entidadVO = new EntidadVO();
                //entidadVO.Id = "202";
                //entidadVO.ConnectionString = "Server=promass-saphana:30015;CURRENTSCHEMA=WOW_DB;UserID=SYSTEM;Password=Promass2017;Application Name=InterfasesSAP";
                //entidadVO.CompanyDB = "WOW_DB";
                //entidadVO.RFC = "TBW200623FC4";
                //entidadVO.SAPPassword = ConfigurationManager.AppSettings["SAPPassword_" + entidadVO.Id];
                //entidadesList.Add(entidadVO.Id, entidadVO);
            }

            return entidadesList;
        }
        
    } // EntidadVO
}
