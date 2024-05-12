using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;


using Newtonsoft.Json;

using GestionApp.Models;
using System.Diagnostics.Contracts;



namespace GestionApp.Controllers
{
    public class MantenedorController : Controller
    {
        IFirebaseClient cliente;
        public MantenedorController() { 
        

            IFirebaseConfig config = new  FirebaseConfig
            {
                AuthSecret = "IiakBzZVLb8CI7nyhHKBV2AogHe2yRyOX1yqZ3BR",
                BasePath = "https://bdfirebase-88369-default-rtdb.firebaseio.com/"

            };



            cliente = new FirebaseClient(config);
        }

        // GET: Mantenedort


        public ActionResult Inicio()
        {
            Dictionary<string,Contacto> lista = new Dictionary<string,Contacto>();
            FirebaseResponse response = cliente.Get("contactos");

            if(response.StatusCode == System.Net.HttpStatusCode.OK)
                lista = JsonConvert.DeserializeObject< Dictionary<string, Contacto> >(response.Body);


           List<Contacto> listaContactos = new List<Contacto>();
            foreach (KeyValuePair<string,Contacto> elemento in lista)
            {
                listaContactos.Add(new Contacto()
                {
                    IdContacto = elemento.Key,
                    Nombre = elemento.Value.Nombre,
                    Correo = elemento.Value.Correo,
                    Telefono = elemento.Value.Telefono
                });



            }
            return View(listaContactos);
        }

        public ActionResult Crear()
        {
            return View();
        }

        public ActionResult Editar(string idcontacto)
        {
            FirebaseResponse response = cliente.Get("contactos/" + idcontacto);

            Contacto ocontacto =response.ResultAs<Contacto>();
            ocontacto.IdContacto = idcontacto;
 


            return View(ocontacto);
        }

        public ActionResult Eliminar(string idcontacto)
        {
            FirebaseResponse response = cliente.Delete("contactos/" + idcontacto);

            return RedirectToAction("Inicio", "Mantenedor");
        }


       [HttpPost]
        public ActionResult Crear(Contacto oContacto)
        {
            String IdGenerado = Guid.NewGuid().ToString("N");

            SetResponse response = cliente.Set("contactos/" + IdGenerado, oContacto);
            if (response.StatusCode == System.Net.HttpStatusCode.OK) {
                return RedirectToAction("Inicio", "Mantenedor");
            }
            else
            {
                return View();
            }


            
        }
        [HttpPost]
        public ActionResult Editar(Contacto oContacto)
        {

            string idcontacto = oContacto.IdContacto;
            oContacto.IdContacto = null;

            FirebaseResponse response = cliente.Update("contactos/" + idcontacto,oContacto);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("Inicio", "Mantenedor");
            }
            else
            {
                return View();
            }
        }





    }
}