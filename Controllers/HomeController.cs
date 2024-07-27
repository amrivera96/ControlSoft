using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
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

        public ActionResult TestDatabaseConnection()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return Content("Conexión a la base de datos exitosa.");
                }
            }
            catch (Exception ex)
            {
                // Registra el error y muestra un mensaje amigable
                System.Diagnostics.Debug.WriteLine("Error de conexión a la base de datos: " + ex.Message);
                return Content("Error de conexión a la base de datos: " + ex.Message);
            }
        }



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

        /*public ActionResult registroEmpleado()
        {
            List<Puesto> puestos = new List<Puesto>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_LeerTodosPuestos", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Puesto puesto = new Puesto
                                {
                                    idPuesto = Convert.ToInt32(reader["idPuesto"]),
                                    nombrePuesto = reader["nombrePuesto"].ToString()
                                };

                                puestos.Add(puesto);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al cargar puestos: " + ex.Message);
                // Considerar un manejo de errores más robusto, como registrar en un archivo o base de datos
            }

            return View(puestos); // Pasar la lista de puestos a la vista
        }*/

        public ActionResult registroEmpleado()
        {
            RegistroEmpleadoViewModel viewModel = new RegistroEmpleadoViewModel
            {
                Puestos = new List<Puesto>(),
                Roles = new List<Rol>()
            };

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Cargar puestos
                    using (SqlCommand cmd = new SqlCommand("sp_LeerTodosPuestos", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Puesto puesto = new Puesto
                                {
                                    idPuesto = Convert.ToInt32(reader["idPuesto"]),
                                    nombrePuesto = reader["nombrePuesto"].ToString()
                                };

                                viewModel.Puestos.Add(puesto);
                            }
                        }
                    }

                    // Cargar roles
                    using (SqlCommand cmd = new SqlCommand("sp_LeerTodosRoles", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Rol rol = new Rol
                                {
                                    idRol = Convert.ToInt32(reader["idRol"]),
                                    nombreRol = reader["nombreRol"].ToString()
                                };

                                viewModel.Roles.Add(rol);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al cargar datos: " + ex.Message);
                // Considerar un manejo de errores más robusto, como registrar en un archivo o base de datos
            }

            return View(viewModel); // Pasar el ViewModel a la vista
        }
        [HttpPost]
        public ActionResult registroEmpleado(RegistroEmpleadoViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Registrar los datos que se enviarán
                    Debug.WriteLine("idEmpleado: " + model.idEmpleado);
                    Debug.WriteLine("NombreCompleto: " + model.NombreCompleto);
                    Debug.WriteLine("ApellidosCompletos: " + model.ApellidosCompletos);
                    Debug.WriteLine("CorreoElectronico: " + model.CorreoElectronico);
                    Debug.WriteLine("Telefono: " + model.Telefono);
                    Debug.WriteLine("nombrePuesto: " + model.nombrePuesto);
                    Debug.WriteLine("FechaIngreso: " + model.FechaIngreso);
                    Debug.WriteLine("nombreRol: " + model.nombreRol);

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_InsertarEmpleado", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@idEmpleado", model.idEmpleado);
                            cmd.Parameters.AddWithValue("@nombre", model.NombreCompleto);
                            cmd.Parameters.AddWithValue("@apellidos", model.ApellidosCompletos);
                            cmd.Parameters.AddWithValue("@correo", model.CorreoElectronico);
                            cmd.Parameters.AddWithValue("@telefono", model.Telefono);
                            cmd.Parameters.AddWithValue("@nombrePuesto", model.nombrePuesto);
                            cmd.Parameters.AddWithValue("@fechaIngreso", model.FechaIngreso);
                            cmd.Parameters.AddWithValue("@nombreRol", model.nombreRol);

                            SqlParameter mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 1000)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(mensajeParam);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            conn.Close();

                            string mensaje = mensajeParam.Value.ToString();
                            if (mensaje.Contains("exitosamente"))
                            {
                                ViewBag.Mensaje = mensaje;
                                ViewBag.AlertType = "success"; // Tipo de alerta para mensajes de éxito
                            }
                            else
                            {
                                ViewBag.Mensaje = mensaje;
                                ViewBag.AlertType = "danger"; // Tipo de alerta para mensajes de error
                            }
                        }
                    }

                    return View("registroEmpleado", model);
                }
                catch (Exception ex)
                {
                    // Registrar el mensaje de la excepción y la traza de pila
                    Debug.WriteLine("Error al registrar empleado: " + ex.Message);
                    Debug.WriteLine("Stack Trace: " + ex.StackTrace);

                    ViewBag.Mensaje = "Error al registrar empleado: " + ex.Message;
                    ViewBag.AlertType = "danger"; // Tipo de alerta para mensajes de error
                }
            }

            return View("registroEmpleado", model);
        }


        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_VerificarLogin", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Agregar parámetros
                        cmd.Parameters.AddWithValue("@usuario", model.Usuario);
                        cmd.Parameters.AddWithValue("@contraseña", model.Contraseña);

                        // Parámetros de salida
                        SqlParameter rolParam = new SqlParameter("@rol", SqlDbType.NVarChar, 50)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(rolParam);

                        SqlParameter estadoParam = new SqlParameter("@estadoCre", SqlDbType.Bit)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(estadoParam);

                        SqlParameter mensajeParam = new SqlParameter("@mensaje", SqlDbType.NVarChar, 100)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(mensajeParam);

                        try
                        {
                            conn.Open();
                            cmd.ExecuteNonQuery();

                            // Obtener los valores de salida
                            string rol = rolParam.Value.ToString();
                            bool estadoCre = (bool)estadoParam.Value;
                            string mensaje = mensajeParam.Value.ToString();

                            // Mostrar valores de salida para depuración
                            Debug.WriteLine($"Valor devuelto para @rol: {rol}");
                            Debug.WriteLine($"Valor devuelto para @estadoCre: {estadoCre}");
                            Debug.WriteLine($"Valor devuelto para @mensaje: {mensaje}");

                            if (estadoCre)
                            {
                                // Redirigir a la vista correspondiente basado en el rol
                                if (rol == "EMPLEADO")
                                {
                                    return RedirectToAction("DashboardInicioEmp");
                                }
                                else if (rol == "SUPERVISOR")
                                {
                                    return RedirectToAction("DashboardInicioSup");
                                }
                                else if (rol == "JEFE")
                                {
                                    return RedirectToAction("DashboardInicioJef");
                                }
                                // Añadir más roles si es necesario
                            }
                            else
                            {
                                // Mostrar mensaje de error
                                ViewBag.Message = mensaje;
                                ViewBag.AlertType = "danger";
                                return View(model);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Manejo de excepciones y errores
                            ViewBag.Message = "Ocurrió un error al procesar su solicitud.";
                            ViewBag.AlertType = "danger";
                            Debug.WriteLine($"Error: {ex.Message}");
                            return View(model);
                        }
                    }
                }
            }

            // Si el modelo no es válido, regresa a la vista con el modelo actual
            return View(model);
        }


        public ActionResult horarios()
        {
            List<UsuarioViewModel> usuarios = new List<UsuarioViewModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_ObtenerUsuarios", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usuarios.Add(new UsuarioViewModel
                            {
                                idEmpleado = reader["idEmpleado"].ToString(),
                                NombreCompleto = reader["NombreCompleto"].ToString(),
                                Puesto = reader["Puesto"].ToString(),
                                FechaIngreso = Convert.ToDateTime(reader["FechaIngreso"])
                            });
                        }
                    }
                }
            }

            return View(usuarios);
        }
        public JsonResult ObtenerTurnosTrabajo()
        {
            List<TurnoTrabajo> turnos = new List<TurnoTrabajo>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_ObtenerTurnosTrabajo", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    TurnoTrabajo turno = new TurnoTrabajo
                    {
                        IdTurno = reader.GetInt32(reader.GetOrdinal("idTurno")),
                        TurnoDescripcion = reader.GetString(reader.GetOrdinal("TurnoDescripcion"))
                    };
                    turnos.Add(turno);
                }
            }

            return Json(turnos, JsonRequestBehavior.AllowGet);
        }

        public ActionResult registrarDepartamento()
        {
            return View();
        }

        


        public ActionResult turnos()
        {
            return View();
        }
        public JsonResult ObtenerDepartamentos()
            {
                List<DepartamentoViewModel> departamentos = new List<DepartamentoViewModel>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT nombreDep FROM Departamentos"; // Asegúrate de que el nombre de columna es correcto

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var departamento = new DepartamentoViewModel
                                {
                                    nombreDep = reader["nombreDep"].ToString() // Asegúrate de que esto coincide con el nombre de columna
                                };
                                departamentos.Add(departamento);

                                // Agregar Debug.WriteLine para depuración
                                Debug.WriteLine($"Departamento: {departamento.nombreDep}");
                            }
                        }
                    }
                }

                // Verificar el contenido completo de la lista para depuración
                foreach (var dep in departamentos)
                {
                    Debug.WriteLine($"Departamento en la lista: {dep.nombreDep}");
                }

                return Json(departamentos, JsonRequestBehavior.AllowGet);
            }


        public ActionResult registrarPuesto()
        {
            return View();
        }

        [HttpPost]
        public ActionResult registrarPuesto(PuestoViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Registrar los datos que se enviarán
                    Debug.WriteLine("NombrePuesto: " + model.NombrePuesto);
                    Debug.WriteLine("EstadoPuesto: " + model.EstadoPuesto);
                    Debug.WriteLine("SalarioHora: " + model.SalarioHora);
                    Debug.WriteLine("NombreDepartamento: " + model.NombreDepartamento);

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_InsertarPuesto", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@nombrePuesto", model.NombrePuesto);
                            cmd.Parameters.AddWithValue("@estadoPuesto", model.EstadoPuesto);
                            cmd.Parameters.AddWithValue("@SalHora", model.SalarioHora);
                            cmd.Parameters.AddWithValue("@nombreDepartamento", model.NombreDepartamento);

                            SqlParameter mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 100)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(mensajeParam);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            conn.Close();

                            string mensaje = mensajeParam.Value.ToString();
                            if (mensaje.Contains("correctamente"))
                            {
                                ViewBag.Mensaje = mensaje;
                                ViewBag.AlertType = "success"; // Tipo de alerta para mensajes de éxito
                            }
                            else
                            {
                                ViewBag.Mensaje = mensaje;
                                ViewBag.AlertType = "danger"; // Tipo de alerta para mensajes de error
                            }
                        }
                    }

                    // Volver a cargar los departamentos
                    //model.Departamentos = ObtenerDepartamentos();

                    return View("registrarPuesto", model);
                }
                catch (Exception ex)
                {
                    // Registrar el mensaje de la excepción y la traza de pila
                    Debug.WriteLine("Error al registrar puesto: " + ex.Message);
                    Debug.WriteLine("Stack Trace: " + ex.StackTrace);

                    ViewBag.Mensaje = "Error al registrar puesto: " + ex.Message;
                    ViewBag.AlertType = "danger"; // Tipo de alerta para mensajes de error

                    // Volver a cargar los departamentos en caso de error
                    //model.Departamentos = ObtenerDepartamentos();
                }
            }

            // Volver a cargar los departamentos en caso de que el modelo no sea válido
            //model.Departamentos = ObtenerDepartamentos();
            return View("registrarPuesto", model);
        }
        [HttpPost]
        public ActionResult registrarDepartamento(DepartamentosViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_InsertarDepartamento", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@nombreDep", model.NombreDep);
                            cmd.Parameters.AddWithValue("@correoDep", model.CorreoDep);
                            cmd.Parameters.AddWithValue("@telefonoDep", model.TelefonoDep);
                            cmd.Parameters.AddWithValue("@estadoDep", model.EstadoDep);
                            cmd.Parameters.AddWithValue("@idJefe", model.IdJefe);

                            SqlParameter mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 100)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(mensajeParam);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                            conn.Close();

                            string mensaje = mensajeParam.Value.ToString();
                            if (mensaje.Contains("correctamente"))
                            {
                                ViewBag.Mensaje = mensaje;
                                ViewBag.AlertType = "success"; // Tipo de alerta para mensajes de éxito
                            }
                            else
                            {
                                ViewBag.Mensaje = mensaje;
                                ViewBag.AlertType = "danger"; // Tipo de alerta para mensajes de error
                            }
                        }
                    }

                    return View("registrarDepartamento", model);
                }
                catch (Exception ex)
                {
                    // Registrar el mensaje de la excepción y la traza de pila
                    Debug.WriteLine("Error al registrar departamento: " + ex.Message);
                    Debug.WriteLine("Stack Trace: " + ex.StackTrace);

                    ViewBag.Mensaje = "Error al registrar departamento: " + ex.Message;
                    ViewBag.AlertType = "danger"; // Tipo de alerta para mensajes de error
                }
            }

            return View("registrarDepartamento", model);
        }

    }

}