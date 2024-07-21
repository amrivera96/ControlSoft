using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControlSoft.Models;

namespace ControlSoft.Controllers
{
    public class HomeController : Controller
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ControlSoftCrDbContext"].ConnectionString;


        // Acción para mostrar la vista de tipos de inconsistencias
        public ActionResult TiposInconsistencia()
        {
            List<TiposInconsistencias> tiposInconsistencias = new List<TiposInconsistencias>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_LeerTodosTiposInconsistencias", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TiposInconsistencias tipoInconsistencia = new TiposInconsistencias
                            {
                                idTipoInconsistencia = Convert.ToInt32(reader["idTipoInconsistencia"]),
                                nombreInconsistencia = reader["nombreInconsistencia"].ToString(),
                                descInconsistencia = reader["descInconsistencia"].ToString(),
                                estadoTipoInconsistencia = Convert.ToBoolean(reader["estadoTipoInconsistencia"]),
                                fechaCreacion = Convert.ToDateTime(reader["fechaCreacion"])
                            };

                            tiposInconsistencias.Add(tipoInconsistencia);
                        }
                    }
                }
            }

            return View(tiposInconsistencias);
        }

        // Acción para crear un nuevo tipo de inconsistencia
        [HttpPost]
        public ActionResult CrearTipoInconsistencia(TiposInconsistencias tipoInconsistencia)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_CrearTipoInconsistencia", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@nombreInconsistencia", tipoInconsistencia.nombreInconsistencia);
                        cmd.Parameters.AddWithValue("@descInconsistencia", tipoInconsistencia.descInconsistencia);
                        cmd.Parameters.AddWithValue("@estadoTipoInconsistencia", tipoInconsistencia.estadoTipoInconsistencia);
                        cmd.Parameters.AddWithValue("@fechaCreacion", tipoInconsistencia.fechaCreacion);

                        SqlParameter mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 1000)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(mensajeParam);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                        ViewBag.Mensaje = mensajeParam.Value.ToString();
                    }
                }
            }

            return RedirectToAction("TiposInconsistencia");
        }

        // Acción para eliminar un tipo de inconsistencia
        [HttpPost]
        public ActionResult EliminarTipoInconsistencia(int idTipoInconsistencia)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_DeleteTipoInconsistencia", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idTipoInconsistencia", idTipoInconsistencia);

                    SqlParameter mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 1000)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(mensajeParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    ViewBag.Mensaje = mensajeParam.Value.ToString();
                }
            }

            return RedirectToAction("TiposInconsistencia");
        }

        // Acción para activar/desactivar un tipo de inconsistencia
        [HttpPost]
        public ActionResult ActivarDesactivarTipoInconsistencia(int idTipoInconsistencia, bool nuevoEstado)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_ActivarDesactivarTipoInconsistencia", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idTipoInconsistencia", idTipoInconsistencia);
                    cmd.Parameters.AddWithValue("@nuevoEstado", nuevoEstado);

                    SqlParameter mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 1000)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(mensajeParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    ViewBag.Mensaje = mensajeParam.Value.ToString();
                }
            }

            return RedirectToAction("TiposInconsistencia");
        }

        // Acción para ver las inconsistencias del empleado
        public ActionResult HistorialInconsistenciasEmp()
        {
            List<RegistroInconsistencia> inconsistencias = new List<RegistroInconsistencia>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_LeerTodosLosRegistrosInconsistencias", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RegistroInconsistencia inconsistencia = new RegistroInconsistencia
                            {
                                idInconsistencia = Convert.ToInt32(reader["idInconsistencia"]),
                                idEmpleado = Convert.ToInt32(reader["idEmpleado"]),
                                idTipoInconsistencia = Convert.ToInt32(reader["idTipoInconsistencia"]),
                                fechaInconsistencia = Convert.ToDateTime(reader["fechaInconsistencia"]),
                                estadoInconsistencia = Convert.ToBoolean(reader["estadoInconsistencia"]),
                                idJustificacion = reader["idJustificacion"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idJustificacion"]),
                                estadoJustificacion = reader["estadoJustificacion"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(reader["estadoJustificacion"])
                            };

                            inconsistencias.Add(inconsistencia);
                        }
                    }
                }
            }

            return View(inconsistencias);
        }

        // Acción para justificar la inconsistencia del empleado
        [HttpPost]
        public ActionResult JustificarInconsistenciaEmp(int idInconsistencia, string descripcionJustificacion, HttpPostedFileBase adjuntoJustificacion)
        {
            byte[] fileBytes = null;

            if (adjuntoJustificacion != null && adjuntoJustificacion.ContentLength > 0)
            {
                using (var binaryReader = new BinaryReader(adjuntoJustificacion.InputStream))
                {
                    fileBytes = binaryReader.ReadBytes(adjuntoJustificacion.ContentLength);
                }
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_JustificarInconsistencia", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idInconsistencia", idInconsistencia);
                    cmd.Parameters.AddWithValue("@estadoJustificacion", true);
                    cmd.Parameters.AddWithValue("@fechaJustificacion", DateTime.Now);
                    cmd.Parameters.AddWithValue("@descripcionJustificacion", descripcionJustificacion ?? (object)DBNull.Value);

                    SqlParameter adjuntoParam = new SqlParameter("@adjuntoJustificacion", SqlDbType.VarBinary);
                    adjuntoParam.Value = fileBytes ?? (object)DBNull.Value;
                    cmd.Parameters.Add(adjuntoParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("HistorialInconsistenciasEmp");
        }

        // Acción para leer las inconsistencias de los empleados
        public ActionResult BandejaInconsistenciasJefe()
        {
            List<RegistroInconsistencia> inconsistencias = new List<RegistroInconsistencia>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_LeerTodosLosRegistrosGestiones", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RegistroInconsistencia inconsistencia = new RegistroInconsistencia
                            {
                                idInconsistencia = Convert.ToInt32(reader["idInconsistencia"]),
                                idEmpleado = Convert.ToInt32(reader["idEmpleado"]),
                                idTipoInconsistencia = Convert.ToInt32(reader["idTipoInconsistencia"]),
                                fechaInconsistencia = Convert.ToDateTime(reader["fechaInconsistencia"]),
                                estadoInconsistencia = Convert.ToBoolean(reader["estadoInconsistencia"]),
                                idJustificacion = reader["idJustificacion"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idJustificacion"]),
                                estadoJustificacion = reader["estadoJustificacion"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(reader["estadoJustificacion"]),
                                Gestion = new GestionInconsistencia
                                {
                                    idGestion = Convert.ToInt32(reader["idGestion"]),
                                    idInconsistencia = Convert.ToInt32(reader["idInconsistencia"]),
                                    fechaGestion = reader["fechaGestion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["fechaGestion"]),
                                    estadoGestion = Convert.ToBoolean(reader["estadoGestion"]),
                                    idJefe = reader["idJefe"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idJefe"]),
                                    observacionGestion = reader["observacionGestion"].ToString()
                                }
                            };

                            inconsistencias.Add(inconsistencia);
                        }
                    }
                }
            }

            return View(inconsistencias);
        }

        // Acción para gestionar las inconsistencias de los empleados
        [HttpPost]
        public ActionResult GestionInconsistenciasJefe(int idInconsistencia, bool estadoInconsistencia, string observacionGestion)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GestionarInconsistencia", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idInconsistencia", idInconsistencia);
                    cmd.Parameters.AddWithValue("@fechaGestion", DateTime.Now);
                    cmd.Parameters.AddWithValue("@estadoGestion", true); // Siempre será true para indicar que está gestionada
                    cmd.Parameters.AddWithValue("@idJefe", 1); // Asume que el ID del jefe se obtiene de la identidad del usuario actual
                    cmd.Parameters.AddWithValue("@observacionGestion", observacionGestion);
                    cmd.Parameters.AddWithValue("@estadoInconsistencia", estadoInconsistencia); // Agregamos el estado de inconsistencia

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("BandejaInconsistenciasJefe");
        }

        // Acción para mostrar la vista de registro de actividades
        public ActionResult TiposActividades()
        {
            List<TiposActividades> actividades = new List<TiposActividades>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_LeerTodosTipoActividades", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TiposActividades actividad = new TiposActividades
                            {
                                idAct = Convert.ToInt32(reader["idAct"]),
                                nombreAct = reader["nombreAct"].ToString(),
                                descpAct = reader["descpAct"].ToString(),
                                fechaCreacion = Convert.ToDateTime(reader["fechaCreacion"]),
                                estadoAct = Convert.ToBoolean(reader["estadoAct"])
                            };

                            actividades.Add(actividad);
                        }
                    }
                }
            }

            return View(actividades);
        }

        // Acción para crear una nueva actividad
        [HttpPost]
        public ActionResult CrearActividad(TiposActividades actividad)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_CrearTipoActividad", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@nombreAct", actividad.nombreAct);
                        cmd.Parameters.AddWithValue("@descpAct", actividad.descpAct);
                        cmd.Parameters.AddWithValue("@fechaCreacion", DateTime.Now);  // Fecha de creación automática
                        cmd.Parameters.AddWithValue("@estadoAct", actividad.estadoAct);

                        SqlParameter mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 1000)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(mensajeParam);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                        ViewBag.Mensaje = mensajeParam.Value.ToString();
                    }
                }
            }

            return RedirectToAction("TiposActividades");
        }

        // Acción para eliminar una actividad
        [HttpPost]
        public ActionResult EliminarActividad(int idAct)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_DeleteTipoActividad", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idAct", idAct);

                    SqlParameter mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 1000)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(mensajeParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    ViewBag.Mensaje = mensajeParam.Value.ToString();
                }
            }

            return RedirectToAction("TiposActividades");
        }

        // Acción para activar/desactivar una actividad
        [HttpPost]
        public ActionResult ActivarDesactivarActividad(int idAct, bool nuevoEstado)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_ActivarDesactivarTipoActividad", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idAct", idAct);
                    cmd.Parameters.AddWithValue("@nuevoEstado", nuevoEstado);

                    SqlParameter mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 1000)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(mensajeParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    ViewBag.Mensaje = mensajeParam.Value.ToString();
                }
            }

            return RedirectToAction("TiposActividades");
        }

        // Acción para mostrar la vista de registro de actividades
        public ActionResult RegistrarActividadDiariaEmp()
        {
            var viewModel = new RegistrarActividadViewModel
            {
                TiposActividades = ObtenerTiposActividades(),
                RegistroActividades = ObtenerRegistroActividades()
            };

            return View(viewModel);
        }

        private List<TiposActividades> ObtenerTiposActividades()
        {
            List<TiposActividades> actividades = new List<TiposActividades>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_LeerTodosTipoActividades", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TiposActividades actividad = new TiposActividades
                            {
                                idAct = Convert.ToInt32(reader["idAct"]),
                                nombreAct = reader["nombreAct"].ToString(),
                                descpAct = reader["descpAct"].ToString(),
                                fechaCreacion = Convert.ToDateTime(reader["fechaCreacion"]),
                                estadoAct = Convert.ToBoolean(reader["estadoAct"])
                            };

                            actividades.Add(actividad);
                        }
                    }
                }
            }

            return actividades;
        }

        private List<RegistroActividades> ObtenerRegistroActividades()
        {
            List<RegistroActividades> registroActividades = new List<RegistroActividades>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_LeerTodosRegistroActividades", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RegistroActividades registro = new RegistroActividades
                            {
                                idRegAct = Convert.ToInt32(reader["idRegAct"]),
                                idAct = Convert.ToInt32(reader["idAct"]),
                                idEmp = Convert.ToInt32(reader["idEmp"]),
                                fechaAct = Convert.ToDateTime(reader["fechaAct"]),
                                horaInicio = (TimeSpan)reader["horaInicio"],
                                horaFinal = (TimeSpan)reader["horaFinal"],
                                duracionAct = (TimeSpan)reader["duracionAct"],
                                estadoReg = Convert.ToBoolean(reader["estadoReg"]),
                                Actividad = new TiposActividades
                                {
                                    idAct = Convert.ToInt32(reader["idAct"]),
                                    nombreAct = reader["nombreAct"].ToString() // Asegúrate de que este campo existe en la consulta SQL
                                }
                            };

                            registroActividades.Add(registro);
                        }
                    }
                }
            }

            return registroActividades;
        }

        // Acción para crear un nuevo registro de actividad
        [HttpPost]
        public ActionResult CrearRegistroActividad(int idAct, int idEmp, TimeSpan horaInicio, TimeSpan horaFinal)
        {
            DateTime fechaAct = DateTime.Now.Date; // Establecer la fecha actual

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_CrearRegistroActividad", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idAct", idAct);
                    cmd.Parameters.AddWithValue("@idEmp", idEmp);
                    cmd.Parameters.AddWithValue("@fechaAct", fechaAct);
                    cmd.Parameters.AddWithValue("@horaInicio", horaInicio);
                    cmd.Parameters.AddWithValue("@horaFinal", horaFinal);
                    cmd.Parameters.AddWithValue("@estadoReg", 0);

                    SqlParameter mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 1000)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(mensajeParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    ViewBag.Mensaje = mensajeParam.Value.ToString();
                }
            }

            return RedirectToAction("RegistrarActividadDiariaEmp");
        }


        public ActionResult BandejaActividadesJefe()
        {
            List<RegistroActividades> actividades = new List<RegistroActividades>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_LeerTodosRegistroActividades", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RegistroActividades actividad = new RegistroActividades
                            {
                                idRegAct = Convert.ToInt32(reader["idRegAct"]),
                                idGesAct = Convert.ToInt32(reader["idGesAct"]),
                                idAct = Convert.ToInt32(reader["idAct"]),
                                idEmp = Convert.ToInt32(reader["idEmp"]),
                                fechaAct = Convert.ToDateTime(reader["fechaAct"]),
                                horaInicio = (TimeSpan)reader["horaInicio"],
                                horaFinal = (TimeSpan)reader["horaFinal"],
                                duracionAct = (TimeSpan)reader["duracionAct"],
                                estadoReg = Convert.ToBoolean(reader["estadoReg"]),
                                Actividad = new TiposActividades
                                {
                                    nombreAct = reader["nombreAct"].ToString()
                                }
                            };

                            actividades.Add(actividad);
                        }
                    }
                }
            }

            return View(actividades);
        }

        [HttpPost]
        public ActionResult GestionarActividad(int idGesAct, string obserGest, bool estadoGesAct)
        {
            int idJefe = 1; // Este valor debería ser dinámico, dependiendo del jefe autenticado

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GestionarActividad", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idGesAct", idGesAct);
                    cmd.Parameters.AddWithValue("@fechaGesAct", DateTime.Now);
                    cmd.Parameters.AddWithValue("@obserGest", obserGest);
                    cmd.Parameters.AddWithValue("@estadoGesAct", estadoGesAct);
                    cmd.Parameters.AddWithValue("@idJefe", idJefe);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("BandejaActividadesJefe");
        }

        public ActionResult shared()
        {
            return View();
        }

        public ActionResult DashboardInicioAdmin()
        {
            return View();
        }

        public ActionResult DashboardInicioEmp()
        {
            return View();
        }

        public ActionResult DashboardInicioJef()
        {
            return View();
        }

        public ActionResult DashboardInicioSup()
        {
            return View();
        }


        public ActionResult SolicPermisos()
        {
            return View();
        }

        public ActionResult SolicHorasExtra()
        {
            return View();
        }

        public ActionResult ladingpage()
        {
            return View();
        }

        public ActionResult registroEmpleado()
        {
            return View();
        }

        public ActionResult login()
        {
            return View();
        }

    }

    public class RegistrarActividadViewModel
    {
        public List<TiposActividades> TiposActividades { get; set; }
        public List<RegistroActividades> RegistroActividades { get; set; }
    }
}