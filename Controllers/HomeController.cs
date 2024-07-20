using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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
}