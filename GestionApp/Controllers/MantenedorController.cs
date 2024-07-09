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
    public class MantenedorController : Controller
    {
        private IFirebaseClient cliente;
        private readonly List<Sucursal> sucursales = new List<Sucursal>
        {
            new Sucursal { Id = "1", Nombre = " JALAPA" },
            new Sucursal { Id = "2", Nombre = " POPTUN" },
            new Sucursal { Id = "3", Nombre = "ZACAPA" },
            new Sucursal { Id = "4", Nombre = " Pinula" },
            new Sucursal { Id = "5", Nombre = " Santa Elena" },
            new Sucursal { Id = "6", Nombre = " ESKALA" }
        };

        public MantenedorController()
        {
            try
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
            catch (Exception ex)
            {
                // Handle initialization exception
                ModelState.AddModelError(string.Empty, $"Error initializing Firebase client: {ex.Message}");
            }
        }

        public ActionResult Inicio()
        {
            try
            {
                Dictionary<string, Empleado> lista = new Dictionary<string, Empleado>();
                FirebaseResponse response = cliente.Get("empleados");

                if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    lista = JsonConvert.DeserializeObject<Dictionary<string, Empleado>>(response.Body);
                    if (lista != null)
                    {
                        List<Empleado> listaEmpleados = new List<Empleado>();

                        foreach (KeyValuePair<string, Empleado> elemento in lista)
                        {
                            Empleado empleado = elemento.Value;
                            empleado.IdEmpleado = elemento.Key;
                            empleado.SucursalNombre = sucursales.FirstOrDefault(s => s.Id == empleado.SucursalId)?.Nombre;
                            empleado.Edad = CalculateAge(empleado.FechaNacimiento);

                            listaEmpleados.Add(empleado);
                        }

                        return View(listaEmpleados);
                    }
                    else
                    {
                        return View(new List<Empleado>());
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error retrieving data from Firebase.");
                    return View(new List<Empleado>());
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return View(new List<Empleado>());
            }
        }

        public ActionResult Crear()
        {
            ViewBag.Sucursales = new SelectList(sucursales, "Id", "Nombre");
            ViewBag.Roles = new SelectList(new List<string> { "Empleado", "Gerente", "Administrador de RRHH" });
            return View();
        }

        [HttpPost]
        public ActionResult Crear(Empleado oEmpleado)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string IdGenerado = Guid.NewGuid().ToString("N");
                    oEmpleado.Edad = CalculateAge(oEmpleado.FechaNacimiento);
                    SetResponse response = cliente.Set("empleados/" + IdGenerado, oEmpleado);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return RedirectToAction("Inicio");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error saving data to Firebase.");
                        ViewBag.Sucursales = new SelectList(sucursales, "Id", "Nombre");
                        ViewBag.Roles = new SelectList(new List<string> { "Empleado", "Gerente", "Administrador de RRHH" });
                        return View(oEmpleado);
                    }
                }
                else
                {
                    ViewBag.Sucursales = new SelectList(sucursales, "Id", "Nombre");
                    ViewBag.Roles = new SelectList(new List<string> { "Empleado", "Gerente", "Administrador de RRHH" });
                    return View(oEmpleado);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                ViewBag.Sucursales = new SelectList(sucursales, "Id", "Nombre");
                ViewBag.Roles = new SelectList(new List<string> { "Empleado", "Gerente", "Administrador de RRHH" });
                return View(oEmpleado);
            }
        }

        public ActionResult Editar(string idEmpleado)
        {
            try
            {
                if (string.IsNullOrEmpty(idEmpleado))
                {
                    ModelState.AddModelError(string.Empty, "ID del empleado no puede estar vacío.");
                    return RedirectToAction("Inicio");
                }

                FirebaseResponse response = cliente.Get("empleados/" + idEmpleado);

                if (response == null || response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    ModelState.AddModelError(string.Empty, "Error retrieving data from Firebase.");
                    return RedirectToAction("Inicio");
                }

                Empleado oEmpleado = response.ResultAs<Empleado>();
                if (oEmpleado == null)
                {
                    ModelState.AddModelError(string.Empty, "Empleado no encontrado.");
                    return RedirectToAction("Inicio");
                }

                oEmpleado.IdEmpleado = idEmpleado;
                oEmpleado.Edad = CalculateAge(oEmpleado.FechaNacimiento);

                ViewBag.Sucursales = new SelectList(sucursales, "Id", "Nombre", oEmpleado.SucursalId);
                ViewBag.Roles = new SelectList(new List<string> { "Empleado", "Gerente", "Administrador de RRHH" }, oEmpleado.Rol);
                return View(oEmpleado);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return RedirectToAction("Inicio");
            }
        }

        [HttpPost]
        public ActionResult Editar(Empleado oEmpleado)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string idEmpleado = oEmpleado.IdEmpleado;
                    oEmpleado.IdEmpleado = null;
                    oEmpleado.Edad = CalculateAge(oEmpleado.FechaNacimiento);

                    FirebaseResponse response = cliente.Update("empleados/" + idEmpleado, oEmpleado);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return RedirectToAction("Inicio");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error updating data in Firebase.");
                        ViewBag.Sucursales = new SelectList(sucursales, "Id", "Nombre", oEmpleado.SucursalId);
                        ViewBag.Roles = new SelectList(new List<string> { "Empleado", "Gerente", "Administrador de RRHH" }, oEmpleado.Rol);
                        return View(oEmpleado);
                    }
                }
                else
                {
                    ViewBag.Sucursales = new SelectList(sucursales, "Id", "Nombre", oEmpleado.SucursalId);
                    ViewBag.Roles = new SelectList(new List<string> { "Empleado", "Gerente", "Administrador de RRHH" }, oEmpleado.Rol);
                    return View(oEmpleado);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                ViewBag.Sucursales = new SelectList(sucursales, "Id", "Nombre", oEmpleado.SucursalId);
                ViewBag.Roles = new SelectList(new List<string> { "Empleado", "Gerente", "Administrador de RRHH" }, oEmpleado.Rol);
                return View(oEmpleado);
            }
        }

        public ActionResult Eliminar(string idEmpleado)
        {
            try
            {
                if (string.IsNullOrEmpty(idEmpleado))
                {
                    ModelState.AddModelError(string.Empty, "ID del empleado no puede estar vacío.");
                    return RedirectToAction("Inicio");
                }

                FirebaseResponse response = cliente.Delete("empleados/" + idEmpleado);

                if (response == null || response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    ModelState.AddModelError(string.Empty, "Error deleting data from Firebase.");
                    return RedirectToAction("Inicio");
                }

                return RedirectToAction("Inicio");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return RedirectToAction("Inicio");
            }
        }

        private int CalculateAge(DateTime birthDate)
        {
            int age = DateTime.Today.Year - birthDate.Year;
            if (birthDate.Date > DateTime.Today.AddYears(-age)) age--;
            return age;
        }
    }
}
