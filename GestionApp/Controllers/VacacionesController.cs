using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using GestionApp.Models;

namespace GestionApp.Controllers
{
    public class VacacionesController : Controller
    {
        private IFirebaseClient cliente;

        public VacacionesController()
        {
            IFirebaseConfig config = new FirebaseConfig
            {
                AuthSecret = "IiakBzZVLb8CI7nyhHKBV2AogHe2yRyOX1yqZ3BR",
                BasePath = "https://bdfirebase-88369-default-rtdb.firebaseio.com/"
            };

            cliente = new FireSharp.FirebaseClient(config);

            if (cliente == null)
            {
                throw new Exception("No se pudo inicializar el cliente de Firebase.");
            }
        }

        public ActionResult Index()
        {
            try
            {
                FirebaseResponse response = cliente.Get("vacaciones");

                if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var lista = JsonConvert.DeserializeObject<Dictionary<string, Vacacion>>(response.Body);
                    var listaVacaciones = lista?.Select(v =>
                    {
                        var vacacion = v.Value;
                        vacacion.IdVacacion = v.Key;
                        return vacacion;
                    }).ToList() ?? new List<Vacacion>();

                    return View(listaVacaciones);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error retrieving data from Firebase.");
                    return View(new List<Vacacion>());
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return View(new List<Vacacion>());
            }
        }

        public ActionResult Crear()
        {
            ViewBag.Empleados = new SelectList(ObtenerEmpleados(), "IdEmpleado", "Nombre");
            return View();
        }

        [HttpPost]
        public ActionResult Crear(Vacacion vacacion)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    vacacion.IdVacacion = Guid.NewGuid().ToString("N");
                    var empleado = ObtenerEmpleadoPorId(vacacion.IdEmpleado);
                    vacacion.NombreEmpleado = empleado.Nombre;
                    vacacion.SucursalId = empleado.SucursalId;
                    vacacion.SucursalNombre = empleado.SucursalNombre;
                    vacacion.Aprobada = false;
                    vacacion.EsPagada = false;
                    vacacion.EsCompletada = false;

                    SetResponse response = cliente.Set($"vacaciones/{vacacion.IdVacacion}", vacacion);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error saving data to Firebase.");
                    }
                }

                ViewBag.Empleados = new SelectList(ObtenerEmpleados(), "IdEmpleado", "Nombre");
                return View(vacacion);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                ViewBag.Empleados = new SelectList(ObtenerEmpleados(), "IdEmpleado", "Nombre");
                return View(vacacion);
            }
        }

        public ActionResult Editar(string idVacacion)
        {
            try
            {
                if (string.IsNullOrEmpty(idVacacion))
                {
                    ModelState.AddModelError(string.Empty, "ID de la vacación no puede estar vacío.");
                    return RedirectToAction("Index");
                }

                FirebaseResponse response = cliente.Get($"vacaciones/{idVacacion}");

                if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var vacacion = response.ResultAs<Vacacion>();
                    ViewBag.Empleados = new SelectList(ObtenerEmpleados(), "IdEmpleado", "Nombre", vacacion.IdEmpleado);
                    return View(vacacion);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error retrieving data from Firebase.");
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Editar(Vacacion vacacion)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = cliente.Update($"vacaciones/{vacacion.IdVacacion}", vacacion);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error updating data in Firebase.");
                    }
                }

                ViewBag.Empleados = new SelectList(ObtenerEmpleados(), "IdEmpleado", "Nombre", vacacion.IdEmpleado);
                return View(vacacion);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                ViewBag.Empleados = new SelectList(ObtenerEmpleados(), "IdEmpleado", "Nombre", vacacion.IdEmpleado);
                return View(vacacion);
            }
        }

        public ActionResult Eliminar(string idVacacion)
        {
            try
            {
                if (string.IsNullOrEmpty(idVacacion))
                {
                    ModelState.AddModelError(string.Empty, "ID de la vacación no puede estar vacío.");
                    return RedirectToAction("Index");
                }

                var response = cliente.Delete($"vacaciones/{idVacacion}");

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error deleting data from Firebase.");
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult MarcarComoPagada(string idVacacion)
        {
            try
            {
                var response = cliente.Get($"vacaciones/{idVacacion}");
                if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var vacacion = response.ResultAs<Vacacion>();
                    vacacion.EsPagada = true;
                    cliente.Update($"vacaciones/{idVacacion}", vacacion);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult MarcarComoCompletada(string idVacacion)
        {
            try
            {
                var response = cliente.Get($"vacaciones/{idVacacion}");
                if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var vacacion = response.ResultAs<Vacacion>();
                    vacacion.EsCompletada = true;
                    cliente.Update($"vacaciones/{idVacacion}", vacacion);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        public ActionResult Aprobar(string idVacacion)
        {
            try
            {
                var response = cliente.Get($"vacaciones/{idVacacion}");
                if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var vacacion = response.ResultAs<Vacacion>();
                    vacacion.Aprobada = true;
                    cliente.Update($"vacaciones/{idVacacion}", vacacion);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        private Empleado ObtenerEmpleadoPorId(string idEmpleado)
        {
            FirebaseResponse response = cliente.Get($"empleados/{idEmpleado}");
            return response.ResultAs<Empleado>();
        }

        private List<Empleado> ObtenerEmpleados()
        {
            FirebaseResponse response = cliente.Get("empleados");
            var lista = JsonConvert.DeserializeObject<Dictionary<string, Empleado>>(response.Body);
            return lista?.Select(e =>
            {
                var empleado = e.Value;
                empleado.IdEmpleado = e.Key;
                return empleado;
            }).ToList() ?? new List<Empleado>();
        }
    }
}
