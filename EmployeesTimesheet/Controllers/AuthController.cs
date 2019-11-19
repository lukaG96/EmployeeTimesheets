using System;
using System.Web;
using System.Web.Mvc;
using BusinessLogic.BusinessObjects;
using BusinessLogic.DAL;
using System.Web.Security;

namespace EmployeesTimesheet.Controllers
{

    public class AuthController : BaseController
    {
        
        [Authorize(Roles = "Admin,Team Leader")]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Team Leader")]
        public ActionResult Register(UserViewModel vm)
        {         
            if (ModelState.IsValid)
            {
                if (BLL.IsUsernameExist(vm.Username))
                {
                    ModelState.AddModelError("UserNameExist", "Username already exist");
                    return View(vm);
                }
                bool IsAdmin = User.IsInRole("Admin");

                if (IsAdmin && (vm.RoleId != 2 && !vm.TeamLeaderId.HasValue))
                {//admin
                    ModelState.AddModelError("", "User Manager And Role is required for Emplyees");
                    return View(vm);
                }
                else if (IsAdmin && ( vm.RoleId == 2 && vm.TeamLeaderId.HasValue))
                {//admin 

                    vm.TeamLeaderId = null;
                }
                else if(!IsAdmin)
                {//team leader 
                    vm.TeamLeaderId = BLL.CurrentUserId(User.Identity.Name);
                    vm.RoleId = 3; //Employee = 3
                }
                               
                var userToCreate = AutoMapper.Mapper.Map<User>(vm);
                BLL.Register(userToCreate, vm.Password, vm.RoleId);
                return RedirectToAction("Index", "Home");
            }            
            return View();
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View();

            var userFromDB = BLL.Login(vm.Username, vm.Password);
            
            if (userFromDB == null)
            {
                ModelState.AddModelError("", "User not found");
                return View();
            }

         
                int timeout = vm.RememberMe ? 525600 : 20; // 525600 min = 1 year
                var ticket = new FormsAuthenticationTicket(vm.Username, vm.RememberMe, timeout);
                string encrypted = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                cookie.Expires = DateTime.Now.AddMinutes(timeout);
                cookie.HttpOnly = true;
                Response.Cookies.Add(cookie);

                return RedirectToAction("Index", "Home");                                     
        }
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Auth");
        }


    }
}