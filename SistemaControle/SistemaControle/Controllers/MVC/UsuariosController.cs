using SistemaControle.Classes;
using SistemaControle.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SistemaControle.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsuariosController : Controller
    {
        private ControleContext db = new ControleContext();

        // GET: Usuarios
        [Authorize] // <-- Faz com quem somente esteja logado acesse
        public ActionResult Index()
        {
            return View(db.Usuarios.ToList());
        }

        // GET: Usuarios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            return View(usuario);
        }

        // GET: Usuarios/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UsuarioView usuarioView)
        {
            if (ModelState.IsValid)
            {
                db.Usuarios.Add(usuarioView.Usuario);
                try
                {
                    if (usuarioView.Foto != null)
                    {
                        var pic = Utilidades.UploadPhoto(usuarioView.Foto);
                        if (!string.IsNullOrEmpty(pic))
                        {
                            usuarioView.Usuario.Photo = string.Format("~/Content/Fotos/{0}", pic);
                        }
                    }
                    db.SaveChanges();

                    Utilidades.CreateUserASP(usuarioView.Usuario.UserName);

                    if (usuarioView.Usuario.Estudante)
                    {
                        Utilidades.AddRoleToUser(usuarioView.Usuario.UserName, "Estudante");
                    }

                    if (usuarioView.Usuario.Professor)
                    {
                        Utilidades.AddRoleToUser(usuarioView.Usuario.UserName, "Professor");
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View(usuarioView);
        }

        // GET: Usuarios/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }

            var view = new UsuarioView
            {
                Usuario = usuario
            };

            return View(view);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UsuarioView usuarioView)
        {
            if (ModelState.IsValid)
            {
                var db2 = new ControleContext();
                var oldUser = db2.Usuarios.Find(usuarioView.Usuario.UserId);
                db2.Dispose();

                if (usuarioView.Foto != null)
                {
                    var pic = Utilidades.UploadPhoto(usuarioView.Foto);
                    if (!string.IsNullOrEmpty(pic))
                    {
                        usuarioView.Usuario.Photo = string.Format("~/Content/Fotos/{0}", pic);
                    }
                }
                else
                {
                    usuarioView.Usuario.Photo = oldUser.Photo;
                }

                db.Entry(usuarioView.Usuario).State = EntityState.Modified;
                try
                {
                    if (oldUser != null && oldUser.UserName != usuarioView.Usuario.UserName)
                    {
                        Utilidades.ChangeEmailUserASP(oldUser.UserName, usuarioView.Usuario.UserName);
                    }
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(usuarioView);
                }
                return RedirectToAction("Index");
            }
            return View(usuarioView.Usuario);
        }

        // GET: Usuarios/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Usuario usuario = db.Usuarios.Find(id);
            db.Usuarios.Remove(usuario);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
