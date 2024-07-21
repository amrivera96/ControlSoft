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


    public class TiposActividades
    {
        public int idAct { get; set; }
        public string nombreAct { get; set; }
        public string descpAct { get; set; }
        public DateTime fechaCreacion { get; set; }
        public bool estadoAct { get; set; }
    }


    public class RegistroActividades
    {
        public int idRegAct { get; set; }
        public int idGesAct { get; set; }
        public int idAct { get; set; }
        public int idEmp { get; set; }
        public DateTime fechaAct { get; set; }
        public TimeSpan horaInicio { get; set; }
        public TimeSpan horaFinal { get; set; }
        public TimeSpan duracionAct { get; set; }
        public bool estadoReg { get; set; }

        // Propiedad de navegación
        public TiposActividades Actividad { get; set; }
    }

    public class GestionActividades
    {
        public int idGesAct { get; set; }
        public DateTime? fechaGesAct { get; set; }
        public string obserGest { get; set; }
        public bool? estadoGesAct { get; set; }
        public int idJefe { get; set; }
    }
}