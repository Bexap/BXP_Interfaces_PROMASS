using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.VO
{
    public class DireccionVO
    {
        public string AddressID = "";
        public string Calle = "";
        public string NumeroExterior = "";
        public string NumeroInterior = "";
        public string Colonia = "";
        public string CodigoPostal = "";
        public string Ciudad = "";
        public string Estado = "";
        public string Pais = "";
        public string LugarExpedicion = "";

        public override string ToString()
        {
            return $"AddressId: {AddressID}, Calle: {Calle}, NumeroExterior: {NumeroExterior}, NumeroInterior: {NumeroInterior}, Colonia: {Colonia}, CodigoPostal: {CodigoPostal}, Ciudad: {Ciudad}, Estado: {Estado}, Pais: {Pais}, LugarExpedicion: {LugarExpedicion}";
        }

    } // DireccionVO
}
