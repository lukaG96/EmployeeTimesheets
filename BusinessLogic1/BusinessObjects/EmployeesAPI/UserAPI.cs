using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.BusinessObjects.EmployeesAPI
{
    public class UserAPI
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public List<ProjectAPI> listProjects { get; set; }
    }
}
