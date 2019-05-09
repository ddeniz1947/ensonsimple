using NHibernate;
using NHibernate.Linq;
using SBlogA.Areas.Admin.ViewModels;
using SBlogA.Infrastructure;
using SBlogA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace SBlogA.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]

    public class UsersController : Controller
    {
        // GET: Admin/Users

        [SelectedTabAttiribute("users list")]
        public ActionResult Index()
        {
            return View(new UsersIndex()
            {
                Users = Database.Session.Query<User>().ToList()
            }
                );
        }
        public ActionResult New()
        {
            return View(new UsersNew
            {
                Roles = Database.Session.Query<Role>().Select(role => new RoleCheckBox
                {
                    Id = role.Id,
                    IsChecked = false,
                    Name= role.Name
                     
                }).ToList()
            });
        }
      
        [HttpPost]
        public ActionResult New(UsersNew formData)
        {
            if (Database.Session.Query<User>().Any(x => x.Username == formData.Username))
                ModelState.AddModelError("Username", "Username Must Be Unique");

            if (!ModelState.IsValid)
                return View(formData);

            var user = new User()
            {
                Email = formData.Email,
                PasswordHash = formData.Password,
                Username = formData.Username
            };
            // user.Id = 6;
            SyncRoles(formData.Roles, user.Roles);
            user.SetPassword(formData.Password);
            Database.Session.Save(user);

            return RedirectToAction("index");

        }
        private void SyncRoles(IList<RoleCheckBox> checkBoxes, IList<Role> roles)
        {
            var selectedRoles = new List<Role>();

            foreach (var role in Database.Session.Query<Role>())
            {
                var checkbox = checkBoxes.Single(c => c.Id == role.Id);
                checkbox.Name = role.Name;

                if (checkbox.IsChecked)
                {
                    selectedRoles.Add(role);
                }
            }
            foreach (var toAdd in selectedRoles.Where(t => !roles.Contains(t)))
            {
                roles.Add(toAdd);

            }
            foreach (var toRemove in roles.Where(t => !selectedRoles.Contains(t)).ToList())
            {
                roles.Add(toRemove);

            }
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var user = Database.Session.Load<User>(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(new UsersEdit
            {
                Username = user.Username,
                Email = user.Email
            });
        }

        [HttpPost]
        public ActionResult Edit(int id, UsersEdit form)
        {
            var user = Database.Session.Get<User>(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            if (Database.Session.Query<User>().Any(u => u.Username == form.Username && u.Id != id))
                ModelState.AddModelError("Username", "Username must be unique");
            if (!ModelState.IsValid)
                return View(form);
            user.Username = form.Username;
            user.Email = form.Email;

            Database.Session.Update(user);
            Database.Session.Flush();
            Database.Session.Clear();
            return RedirectToAction("index");

        }
        public ActionResult ResetPassword(int id)
        {
            var user = Database.Session.Load<User>(id);
            if(user == null)
            {
                return HttpNotFound();

            }
            return View(new UsersResetPassword()
            {
                Username = user.Username
            });
        }
        [HttpPost]
        public ActionResult ResetPassword(int id, UsersResetPassword formData)
        {
            var user = Database.Session.Get<User>(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            formData.Username = user.Username;

           
            if (!ModelState.IsValid)
                return View(formData);
            user.SetPassword(formData.Password);
       

            Database.Session.Update(user);
            Database.Session.Flush();
            Database.Session.Clear();
            return RedirectToAction("index");

        }
        [HttpPost]
        public bool Delete(int id)
        {
            var user = Database.Session.Load<User>(id);
            if (user == null)
            {
                return false;
            }
            try
            {
                Database.Session.Delete(user);
                Database.Session.Flush();
                Database.Session.Clear();
            }
            catch (Exception)
            {

                return false;
            }

            return true;
            //return RedirectToAction("index");
        }

    }
}