using System;

namespace GestionApp.Models
{
    public class Empleado
    {
        public string IdEmpleado { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public int Edad { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string SucursalId { get; set; }
        public string SucursalNombre { get; set; }
        public string Telefono2 { get; set; }
        public decimal Sueldo { get; set; }
        public string Puesto { get; set; }
        public string Rol { get; set; } // empleado, gerente, administrador de RRHH
    }
}
