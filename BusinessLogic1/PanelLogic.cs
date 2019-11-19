using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.BusinessObjects;
using BusinessLogic.DAL;
using BusinessLogic.BusinessObjects.Dropdowns;
using BusinessLogic.BusinessObjects.EmployeesAPI;

namespace BusinessLogic
{
    public class PanelLogic : IDisposable
    {
        protected DAL.TimeSheetEntities1 DB;
        public PanelLogic()
        {
            DB = new DAL.TimeSheetEntities1();
        }

        public void Dispose()
        {
            if (DB != null)
                DB.Dispose();
        }

        #region Users

        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {

                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
            }
            return true;
        }
        public void Register(User user, string password, int roleId)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.Active = true;

            DB.Users.Add(user);
            user.UserRoles.Add(new UserRole() { RoleId = roleId });
            DB.SaveChanges();

        }
        public User Login(string username, string password)
        {
            var user = DB.Users.FirstOrDefault(x => x.Username == username && x.Active == true);

            if (user == null)
                return null;

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }
        public int CurrentUserId(string username)
        {
            return DB.Users.FirstOrDefault(x => x.Username == username).Id;
        }
        public bool IsUsernameExist(string username)
        {
            return DB.Users.Any(x => x.Username == username);
        }
        #endregion

        #region Employees
        public List<EmployeeViewModel> GetEmployees(string username, bool isAdmin)
        {
            int id = CurrentUserId(username);
            if (isAdmin)
            {
                return DB.Users.Where(x => x.Id != id && x.Active == true).Select(r => new EmployeeViewModel()
                {
                    Id = r.Id,
                    Username = r.Username,
                    TeamLeader = DB.Users.FirstOrDefault(y => y.Id == r.TeamLeaderId).Username,
                    UserRole = r.UserRoles.FirstOrDefault(x => x.UserId == r.Id).Role.Name
                }).OrderBy(r => r.TeamLeader).ToList();
            }
            return DB.Users.Where(x => x.TeamLeaderId == id && x.Active == true).Select(r => new EmployeeViewModel()
            {
                Id = r.Id,
                Username = r.Username,
            }).ToList();
        }
        public UserEditVIewModel GetEmployee(int id)
        {
            var employee = DB.Users.FirstOrDefault(x => x.Id == id && x.Active == true);
            return new UserEditVIewModel()
            {
                Id = employee.Id,
                Username = employee.Username,
                TeamLeaderId = employee.TeamLeaderId,
                RoleId = DB.UserRoles.FirstOrDefault(x => x.UserId == employee.Id).RoleId
            };
        }
        public void EditEmpoyeeAdmin(int id, UserEditVIewModel vm)
        {
            if (!CheckUserExistForEdit(id, vm.Username))
            {
                var userFromDb = DB.Users.Include("UserRoles").FirstOrDefault(x => x.Id == id && x.Active == true);
                userFromDb.Username = vm.Username;
                //only admin can change teamLeader to employee
                if (!userFromDb.TeamLeaderId.HasValue && vm.TeamLeaderId.HasValue)
                { //change teamLeader to employee
                    if (!CheckTlHaveEmployees(userFromDb.Id))
                    {//can`t change team lider to employee if tl have employees                       
                        userFromDb.UserRoles.FirstOrDefault(x => x.UserId == vm.Id).RoleId = (int)vm.RoleId;
                        userFromDb.TeamLeaderId = vm.TeamLeaderId;
                    }
                }
                else if (userFromDb.TeamLeaderId.HasValue && !vm.TeamLeaderId.HasValue)
                {//change employee to teamLeader
                    userFromDb.TeamLeaderId = null;
                    userFromDb.UserRoles.FirstOrDefault(x => x.UserId == vm.Id).RoleId = (int)vm.RoleId;
                }
                else if (userFromDb.TeamLeaderId.HasValue && vm.TeamLeaderId.HasValue)
                {//set new tl for employee
                    userFromDb.TeamLeaderId = vm.TeamLeaderId;
                }
                DB.SaveChanges();
            };
        }
        public void EditEmpoyeeTeamLeader(int id, UserEditVIewModel vm)
        {
            if (!CheckUserExistForEdit(id, vm.Username))
            {
                DB.Users.FirstOrDefault(x => x.Id == id && x.Active == true).Username = vm.Username;
                DB.SaveChanges();
            };
        }
        public bool CheckTlHaveEmployees(int employeeId)
        {
            return DB.Users.Any(x => x.TeamLeaderId == employeeId && x.Active == true);
        }
        public bool CheckIsTlOfEmployee(int tlId, int employeeId)
        {
            if (CheckUserExist(employeeId))
            {
                if (DB.Users.FirstOrDefault(x => x.Id == employeeId).TeamLeaderId == tlId)
                    return true;
            }
            return false;
        }
        public bool CheckUserExist(int id)
        {
            if (DB.Users.Any(x => x.Id == id))
                return true;

            return false;

        }
        public bool CheckUserExistForEdit(int id, string username)
        {
            if (CheckUserExist(id))
            {
                return DB.Users.Any(x => x.Username == username && x.Id != id && x.Active == true);
            }
            return true;
        }
        #endregion

        #region Projects
        public void CreateNewProject(Project project)
        {
            DB.Projects.Add(project);
            DB.SaveChanges();
        }
        public List<ProjectViewModel> GetProjects(string username, bool isAdmin)
        {
            if (isAdmin)
            {
                return DB.Projects.Where(x => x.Active).Select(x => new ProjectViewModel
                {
                    Id = x.Id,
                    CreatedDate = x.CreatedDate,
                    ProjectType = x.Type.Name,
                    ProjectName = x.Name,
                    CreatedAt = DB.Users.FirstOrDefault(s => s.Id == x.ManagerId).Username
                }).ToList();
            }
            int? tlId = DB.Users.FirstOrDefault(x => x.Username == username).TeamLeaderId;
            int id = DB.Users.FirstOrDefault(x => x.Username == username).Id;
            return DB.Projects.Where(x => (x.ManagerId == tlId || x.ManagerId == id) && x.Active).Select(x => new ProjectViewModel
            {
                Id = x.Id,
                CreatedDate = x.CreatedDate,
                ProjectType = x.Type.Name,
                ProjectName = x.Name

            }).ToList();
        }
        public ProjectViewModel GetProjectWithTimeSheets(int projectId, string username)
        {
            int? userId = DB.Users.FirstOrDefault(x => x.Username == username).Id;
            Project dbProject = DB.Projects.Include("TimeSheets").FirstOrDefault(x => x.Id == projectId && x.Active);
            ProjectViewModel proj = new ProjectViewModel()
            {
                Id = dbProject.Id,
                CreatedDate = dbProject.CreatedDate,
                ProjectType = dbProject.Type.Name,
                ProjectName = dbProject.Name,
                TimeSheets = dbProject.TimeSheets.Where(r => r.EmployeeId == userId && r.Active == true).Select(r => new TimeSheetViewModel
                {
                    Details = r.Details,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    Date = r.Date.ToShortDateString()
                }).ToList()
            };
            proj.TimeSheets = proj.TimeSheets.OrderByDescending(x => x.Date).ToList();


            return proj;
        }

        public ProjectEditViewModel GetProject(int id)
        {

            var projectFromDb = DB.Projects.FirstOrDefault(x => x.Id == id && x.Active);
            return new ProjectEditViewModel()
            {
                Active = projectFromDb.Active,
                Id = projectFromDb.Id,
                Name = projectFromDb.Name,
                TypeId = projectFromDb.TypeId
            };
        }
        public bool UpdateProject(int id, ProjectEditViewModel vm)
        {
            if (CheckProjectExist(id))
            {
                var project = DB.Projects.FirstOrDefault(x => x.Id == id);
                project.TypeId = vm.TypeId;
                project.Name = vm.Name;
                DB.SaveChanges();
                return true;
            }
            return false;

        }
        public void DeleteProject(int id)
        {
            if (CheckProjectExist(id))
            {
                DB.Projects.FirstOrDefault(x => x.Id == id).Active = false;
                DB.SaveChanges();
            }
        }
        public bool CheckProject(int id)
        {
            //check if project have timesheets
            if (DB.TimeSheets.Any(x => x.ProjectId == id))
                return true;

            return false;
        }
        public bool CheckProjectExist(int id)
        {
            if (DB.Projects.Any(x => x.Id == id))
                return true;

            return false;
        }
        public List<ProjectViewModel> DeletedProjects(string username, bool isAdmin)
        {
            if (isAdmin)
            {
                return DB.Projects.Where(x => x.Active == false).Select(r => new ProjectViewModel()
                {
                    CreatedDate = r.CreatedDate,
                    Id = r.Id,
                    ProjectType = r.Type.Name,
                    ProjectName = r.Name,
                    CreatedAt = DB.Users.FirstOrDefault(s => s.Id == r.ManagerId).Username
                }).ToList();
            }
            int id = CurrentUserId(username);
            return DB.Projects.Where(x => x.Active == false && x.ManagerId == id).Select(r => new ProjectViewModel()
            {
                CreatedDate = r.CreatedDate,
                Id = r.Id,
                ProjectType = r.Type.Name,
                ProjectName = r.Name
            }).ToList();
        }
        public void ReturnProjectToActive(int projectId)
        {
            if (CheckProjectExist(projectId))
            {
                DB.Projects.FirstOrDefault(x => x.Id == projectId).Active = true;
                DB.SaveChanges();
            }

        }
        #endregion

        #region Timesheets
        public void CreateNewTimesheet(TimeSheet model)
        {
            DB.TimeSheets.Add(model);
            DB.SaveChanges();
        }
        public List<TimeSheetViewModel> GetTimesheets(int employeeId)
        {
            return DB.TimeSheets.Where(x => x.EmployeeId == employeeId && x.Active == true).Select(a => new TimeSheetViewModel
            {
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Date = a.Date.ToString(),
                Details = a.Details,
                Project = a.Project.Name
            }).OrderBy(a => a.Project).ThenByDescending(a => a.Date).ToList();
        }
        public List<ProjectTotalHoursViewModel> SpentTimeOnProjects(int id, bool isAdmin)
        {
            List<Project> dbProjects = new List<Project>();
            if (isAdmin)
                dbProjects = DB.Projects.Include("TimeSheets").ToList();
            else
                dbProjects = DB.Projects.Include("TimeSheets").Where(x => x.ManagerId == id).ToList();

            return dbProjects
                .Select(x => new ProjectTotalHoursViewModel
                {
                    Id = x.Id,
                    CreatedDate = x.CreatedDate,
                    ProjectType = x.Type.Name,
                    ProjectName = x.Name,
                    Status = x.Active,
                    TimeSheets = x.TimeSheets.Select(r => new Range
                    {
                        Start = r.StartTime,
                        End = r.EndTime,
                    }).ToList()
                }).OrderByDescending(x => x.Status).ThenByDescending(x => x.CreatedDate).ToList();
        }
        public string CalculateTime(List<TimeSheetViewModel> times)
        {
            TimeSpan totalTime = new TimeSpan();
            if (times.Count() == 0)
            {
                return null;
            }
            for (int i = 0; i < times.Count(); i++)
            {
                totalTime = totalTime.Add(times[i].EndTime - times[i].StartTime);
            }
            return totalTime.ToString();
        }
        public string CalculateTotalTime(List<Range> times)
        {
            TimeSpan totalTime = new TimeSpan();
            if (times.Count() == 0)
            {
                return null;
            }
            for (int i = 0; i < times.Count(); i++)
            {
                totalTime = totalTime.Add(times[i].End - times[i].Start);
            }
            return totalTime.ToString();
        }
        #endregion

        #region DataForAPI
        #region ProjectDataAPI
        public List<ProjectTotalHoursViewModel> DataAnalyticsForProjects(DateTime date)
        {
            List<Project> dbProjects = DB.Projects.Include("TimeSheets").Where(x => x.Active /*&& x.CreatedDate > date*/).ToList();
            List<ProjectTotalHoursViewModel> vm = dbProjects
                .Select(x => new ProjectTotalHoursViewModel
                {
                    Id = x.Id,
                    CreatedDate = x.CreatedDate,
                    ProjectType = x.Type.Name,
                    ProjectName = x.Name,
                    Status = x.Active,
                    TimeSheets = x.TimeSheets.Where(s => s.Date >= date).Select(r => new Range
                    {
                        Start = r.StartTime,
                        End = r.EndTime,
                    }).ToList()
                }).ToList();
            SumTotalProjects(vm);
            return vm;
        }
        public List<ProjectTotalHoursViewModel> GetProjectDataBetweenDates(DateTime fromDate, DateTime toDate)
        {
            List<Project> dbProjects = DB.Projects.Include("TimeSheets").Where(x => x.Active /*&& x.CreatedDate > date*/).ToList();
            List<ProjectTotalHoursViewModel> vm = dbProjects
                .Select(x => new ProjectTotalHoursViewModel
                {
                    Id = x.Id,
                    CreatedDate = x.CreatedDate,
                    ProjectType = x.Type.Name,
                    ProjectName = x.Name,
                    Status = x.Active,
                    TimeSheets = x.TimeSheets.Where(s => s.Date >= fromDate && s.Date <= toDate).Select(r => new Range
                    {
                        Start = r.StartTime,
                        End = r.EndTime,
                    }).ToList()
                }).ToList();
            SumTotalProjects(vm);
            return vm;
        }
        public List<ProjectTotalHoursViewModel> DataAnalyticsForDay(DateTime date)
        {
            List<Project> dbProjects = DB.Projects.Include("TimeSheets").Where(x => x.Active /*&& x.CreatedDate > date*/).ToList();
            List<ProjectTotalHoursViewModel> vm = dbProjects
                .Select(x => new ProjectTotalHoursViewModel
                {
                    Id = x.Id,
                    CreatedDate = x.CreatedDate,
                    ProjectType = x.Type.Name,
                    ProjectName = x.Name,
                    Status = x.Active,
                    TimeSheets = x.TimeSheets.Where(s => s.Date == date).Select(r => new Range
                    {
                        Start = r.StartTime,
                        End = r.EndTime,
                    }).ToList()
                }).ToList();
            SumTotalProjects(vm);
            return vm;
        }
        private void SumTotalProjects(List<ProjectTotalHoursViewModel> vm)
        {
            foreach (var project in vm)
            {
                project.TotalHours = CalculateTotalTime(project.TimeSheets);
            }
        }
        #endregion
        #region EmployeeDataAPI
        public List<UserAPI> DataAnalyticsForEmployees(DateTime date)
        {
            List<UserAPI> vm = DB.Users.Where(x => x.Active  == true)
            .Select(x => new UserAPI
            {
                Id = x.Id,
                Username = x.Username,
                listProjects = x.TimeSheets.GroupBy(e => e.Project.Name).Select(grp => new ProjectAPI
                {
                    Name = grp.Key,
                    listTimesheets = grp.Where(e => e.Date >= date).Select(r => new TimeSheetViewModel
                    {
                        Details = r.Details,
                        StartTime = r.StartTime,
                        EndTime = r.EndTime,
                        CreatedDate = r.Date,
                        TotalHours = "Hours: " + (r.EndTime.Hour - r.StartTime.Hour).ToString() + " Minutes: " + (r.EndTime.Minute - r.StartTime.Minute).ToString()
                    }).ToList()
                }).ToList()
            }).ToList();
            SumTotalHours(vm);
            return vm;

        }
        public List<UserAPI> DataAnalyticsEmployeesBetweenDates(DateTime fromDate, DateTime toDate)
        {
            List<UserAPI> vm = DB.Users.Where(x => x.Active == true)
            .Select(x => new UserAPI
            {
                Id = x.Id,
                Username = x.Username,
                listProjects = x.TimeSheets.GroupBy(e => e.Project.Name).Select(grp => new ProjectAPI
                {
                    Name = grp.Key,
                    listTimesheets = grp.Where(s => s.Date >= fromDate && s.Date <= toDate).Select(r => new TimeSheetViewModel
                    {
                        Details = r.Details,
                        StartTime = r.StartTime,
                        EndTime = r.EndTime,
                        CreatedDate = r.Date,
                        TotalHours = " " + (r.EndTime.Hour - r.StartTime.Hour).ToString() + " Hours " + (r.EndTime.Minute - r.StartTime.Minute).ToString() + " Minutes"
                    }).ToList()
                }).ToList()
            }).ToList();
            SumTotalHours(vm);
            return vm;
        }
        private void SumTotalHours(List<UserAPI> vm)
        {
            foreach (var employee in vm)
            {
                foreach (var project in employee.listProjects)
                {
                    project.TotalHours = CalculateTime(project.listTimesheets);
                }

            }
        }
        #endregion
        #endregion

        #region Dropdowns
        public TimesheetDropdown GetTimesheetDropdowns()
        {
            return new BusinessObjects.Dropdowns.TimesheetDropdown()
            {
                ProjectTypes = (from t in DB.Types
                                orderby t.Id
                                select new Generic()
                                {
                                    id = t.Id,
                                    text = t.Name
                                }).ToList(),
            };
        }
        public UsersRolesDropdowns GetUsersRolesDropdowns()
        {
            return new UsersRolesDropdowns()
            {
                UsersList = (from role in DB.UserRoles
                             join users in DB.Users
                             on role.UserId equals users.Id
                             where role.RoleId == 2
                             select new Generic()
                             {
                                 id = users.Id,
                                 text = users.Username
                             }).ToList(),
                UsersRoleList = (from t in DB.Roles
                                 where t.Id != 1
                                 orderby t.Name
                                 select new Generic()
                                 {
                                     id = t.Id,
                                     text = t.Name
                                 }).ToList(),
            };
        }

        #endregion
    }
}
