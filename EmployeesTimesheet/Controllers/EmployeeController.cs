﻿using System.Collections.Generic;
using System.Web.Mvc;
using BusinessLogic.BusinessObjects;

namespace EmployeesTimesheet.Controllers
{
    [Authorize]
    public class EmployeeController : BaseController
    {
        [Authorize(Roles = "Admin,Team Leader")]
        public ActionResult Employees()
        {
            bool IsAdmin = User.IsInRole("Admin");
            return View(BLL.GetEmployees(User.Identity.Name, IsAdmin));
        }
        [Authorize(Roles = "Admin,Team Leader")]
        public ActionResult Edit(int id)
        {
            if (User.IsInRole("Admin"))
            {
                return View(BLL.GetEmployee(id));
            }
            else if (BLL.CheckIsTlOfEmployee(BLL.CurrentUserId(User.Identity.Name), id))
            {
                return View(BLL.GetEmployee(id));
            }
            else
            {
                return RedirectToAction("Index","Home");
            }
        }
       
        [HttpPost]
        [Authorize(Roles = "Admin,Team Leader")]
        public ActionResult Edit(int id, UserEditVIewModel vm)
        {
            try
            {               
                bool IsAdmin = User.IsInRole("Admin");
                if (!IsAdmin)
                {
                    BLL.EditEmpoyeeTeamLeader(id, vm);
                    return RedirectToAction("Employees");
                }
                if (vm.RoleId != 2 && !vm.TeamLeaderId.HasValue)
                {                        
                    return View(vm);
                }
                BLL.EditEmpoyeeAdmin(id, vm);
                return RedirectToAction("Employees");                               
            }
            catch
            {
                return View();
            }
        }       
        // GET: Employee/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Employee/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        [Authorize(Roles = "Admin,Team Leader")]
        public ActionResult EmployeeTimesheets(int id)
        {
            List<TimeSheetViewModel> list = new List<TimeSheetViewModel>();
            if (BLL.CheckUserExist(id))
            {
                if (User.IsInRole("Admin"))
                {
                    list = BLL.GetTimesheets(id);
                }
                else if (BLL.CheckIsTlOfEmployee(BLL.CurrentUserId(User.Identity.Name), id))
                {
                    list = BLL.GetTimesheets(id);
                }
                
            }
            else
            {
                ModelState.AddModelError("", "User Does Not Exist");
            }
            return View(list);
        }
        public JsonResult LoadDropdowns()
        {
            return Json(new { Response = "OK", data = BLL.GetUsersRolesDropdowns() }, JsonRequestBehavior.AllowGet);
        }
        #region API Analytics
        public ActionResult AdminEmployeeDataDaily()
        {
            return View();
        }
        public ActionResult AdminEmployeeDataMonthly()
        {
            return View();
        }
        public ActionResult AdminEmployeeDataWeekly()
        {
            return View();
        }


        #endregion
    }
}
