using System;

namespace GestionApp.Models
{
    public class Vacacion
    {
        public string IdVacacion { get; set; }
        public string IdEmpleado { get; set; }
        public string NombreEmpleado { get; set; }
        public string SucursalId { get; set; }
        public string SucursalNombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Motivo { get; set; }
        public bool EsPagada { get; set; }
        public bool EsCompletada { get; set; }
        public bool Aprobada { get; set; }
        public DateTime FechaSolicitud { get; set; }
    }
}
