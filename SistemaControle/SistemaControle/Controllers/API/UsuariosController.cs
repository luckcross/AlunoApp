using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using SistemaControle.Models;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SistemaControle.Classes;

namespace SistemaControle.Controllers.API
{
    [RoutePrefix("API/Usuarios")]
    public class UsuariosController : ApiController
    {
        private ControleContext db = new ControleContext();

        [HttpPost]
        [Route("Login")]
        public IHttpActionResult Login(JObject form)
        {
            string email = string.Empty;
            string password = string.Empty;
            dynamic jsonObject = form;

            try
            {
                email = jsonObject.Email.Value;
                password = jsonObject.Senha.Value;
            }
            catch
            {
                return this.BadRequest("Chamada Incorreta");
            }

            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var userASP = userManager.Find(email, password);

            if (userASP == null)
            {
                return this.BadRequest("Usuário ou Senha incorretos");
            }

            var user = db.Usuarios
                .Where(u => u.UserName == email)
                .FirstOrDefault();

            if (user == null)
            {
                return this.BadRequest("Usuário ou Senha incorretos ");
            }

            return this.Ok(user);
        }

        // GET: api/Usuarios
        public List<Usuario> GetUsuarios()
        {
            var usuarios = db.Usuarios.ToList();
            return usuarios;
        }

        // GET: api/Usuarios/5
        [ResponseType(typeof(Usuario))]
        public IHttpActionResult GetUsuario(int id)
        {
            Usuario usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }

        [HttpPut]
        [Route("MudarSenha")]
        public IHttpActionResult TrocarSenha(JObject form)
        {
            string userName = string.Empty;
            string novaSenha = string.Empty;
            string velhaSenha = string.Empty;
            dynamic jsonObject = form;

            try
            {
                userName = jsonObject.UserName.Value;
                novaSenha = jsonObject.NovaSenha.Value;
                velhaSenha = jsonObject.VelhaSenha.Value;
            }
            catch
            {
                return this.BadRequest("Chamada Incorreta");
            }

            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));
            var userASP = userManager.Find(userName, velhaSenha);

            if (userASP == null)
            {
                return this.BadRequest("Usuário ou Senha incorretos");
            }

            IdentityResult resposta = userManager.ChangePassword(userASP.Id, velhaSenha, novaSenha);
            if (resposta.Succeeded)
            {
                return Ok<object>(new
                {
                    Message = "A senha foi alterada"
                });
            }
            else
            {
                return BadRequest(resposta.Errors.ToString());
            }
        }


        // PUT: api/Usuarios/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUsuario(int id, Usuario usuario)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != usuario.UserId)
                return BadRequest();

            var db2 = new ControleContext();
            var oldUser = db2.Usuarios.Find(id);
            db2.Dispose();

            db.Entry(usuario).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
                if (oldUser != null && oldUser.UserName != usuario.UserName)
                {
                    Utilidades.ChangeEmailUserASP(oldUser.UserName, usuario.UserName);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                    return NotFound();
                else
                    throw;
            }

            return this.Ok(usuario);
        }

        // POST: api/Usuarios
        [ResponseType(typeof(Usuario))]
        public IHttpActionResult PostUsuario(UsuarioSenha usuarioSenha)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = new Usuario
            {
                Endereco = usuarioSenha.Endereco,
                Professor = false,
                Estudante = true,
                Sobrenome = usuarioSenha.Sobrenome,
                Telefone = usuarioSenha.Telefone,
                UserName = usuarioSenha.UserName,
                Nome = usuarioSenha.Nome
            };

            try
            {
                db.Usuarios.Add(usuario);
                db.SaveChanges();
                Utilidades.CreateUserASP(usuarioSenha.UserName, "Estudante", usuarioSenha.Senha);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            usuarioSenha.UserId = usuario.UserId;
            usuarioSenha.Professor = false;
            usuarioSenha.Estudante = true;

            return this.Ok(usuarioSenha);
        }

        // DELETE: api/Usuarios/5
        [ResponseType(typeof(Usuario))]
        public IHttpActionResult DeleteUsuario(int id)
        {
            Usuario usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return NotFound();
            }

            db.Usuarios.Remove(usuario);
            db.SaveChanges();

            return Ok(usuario);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UsuarioExists(int id)
        {
            return db.Usuarios.Count(e => e.UserId == id) > 0;
        }
    }
}