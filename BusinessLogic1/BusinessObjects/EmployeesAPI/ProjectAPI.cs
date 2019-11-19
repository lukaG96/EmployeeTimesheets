using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.BusinessObjects.EmployeesAPI
{
    public class ProjectAPI
    {
        public string Name { get; set; }
        public List<TimeSheetViewModel> listTimesheets { get; set; }
        public string TotalHours { get; set; }
    }
}
