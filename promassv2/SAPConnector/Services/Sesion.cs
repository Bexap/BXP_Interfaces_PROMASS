using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbobsCOM;

namespace SAPConnector.Services
{
    public class Sesion
    {
        public Guid uui = Guid.NewGuid();
        public DateTime born = DateTime.Now;
        public Company company = null;

        public void Close()
        {
            SessionPool.DevolverSesion(uui);
        }
    } // Sesion
}
