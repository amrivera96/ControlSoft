using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace ControlSoft.Models
{

    public class TiposInconsistencias
    {
        public int idTipoInconsistencia { get; set; }
        public string nombreInconsistencia { get; set; }
        public string descInconsistencia { get; set; }
        public bool estadoTipoInconsistencia { get; set; }
        public DateTime fechaCreacion { get; set; }
    }

    public class RegistroInconsistencia
    {
        public int idInconsistencia { get; set; }
        public int idEmpleado { get; set; }
        public int idTipoInconsistencia { get; set; }
        public DateTime fechaInconsistencia { get; set; }
        public bool estadoInconsistencia { get; set; } // 0 = No aprobada, 1 = Aprobada
        public int? idJustificacion { get; set; } // Esta propiedad es nullable porque podría no haber justificación
        public bool? estadoJustificacion { get; set; } // 0 = No justificada, 1 = Justificada
        public GestionInconsistencia Gestion { get; set; } // Agregamos la propiedad de gestión
        public JustificacionInconsistencia Justificacion { get; set; } // Agregamos la propiedad de justificación
    }

    public class GestionInconsistencia
    {
        public int idGestion { get; set; }
        public int idInconsistencia { get; set; }
        public DateTime? fechaGestion { get; set; }
        public bool estadoGestion { get; set; } // 0 = No gestionada, 1 = Gestionada
        public int? idJefe { get; set; }
        public string observacionGestion { get; set; }
    }

    public class JustificacionInconsistencia
    {
        public int idJustificacion { get; set; }
        public int idInconsistencia { get; set; }
        public bool estadoJustificacion { get; set; } // 0 = No justificada, 1 = Justificada
        public DateTime? fechaJustificacion { get; set; }
        public string descripcionJustificacion { get; set; }
        public byte[] adjuntoJustificacion { get; set; }
    }


}