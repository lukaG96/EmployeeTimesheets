using System;
using System.Globalization;
using System.Web.Http;


namespace EmployeesTimesheet.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class AnalyticController : ApiController
    {
        protected BusinessLogic.PanelLogic BLL;
        public AnalyticController()
        {
            BLL = new BusinessLogic.PanelLogic();
        }
        #region ProjectData
        [Route("api/Analytic/GetProjectDataLastDay")]
        [HttpGet]
        public IHttpActionResult GetProjectDataLastDay()
        {
            var date = DateTime.Now.AddDays(-1).Date;
            return Ok(BLL.DataAnalyticsForProjects(date));
        }
        [Route("api/Analytic/GetProjectDataForDay")]
        [HttpGet]
        public IHttpActionResult GetProjectDataForDay(string date)
        {
            var dd = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            return Ok(BLL.DataAnalyticsForDay(dd));
        }
        [Route("api/Analytic/GetProjectDataLastWeek")]
        [HttpGet]
        public IHttpActionResult GetProjectDataLastWeek()
        {
            var date = DateTime.Now.AddDays(-7);
            return Ok(BLL.DataAnalyticsForProjects(date));

        }
        [Route("api/Analytic/GetProjectDataBetweenDates")]
        [HttpGet]
        public IHttpActionResult GetProjectDataBetweenDates(DateTime fromDate, DateTime toDate)
        {           
            if (fromDate <= toDate)
            {
                return Ok(BLL.GetProjectDataBetweenDates(fromDate,toDate));
            }
            return BadRequest("The End date must be greater than the Start date");

        }
        [Route("api/Analytic/GetProjectDataLastMonth")]
        [HttpGet]
        public IHttpActionResult GetProjectDataLastMonth()
        {
            var date = DateTime.Now.AddMonths(-1).Date;
            return Ok(BLL.DataAnalyticsForProjects(date));
        }
        

        #endregion

        #region EmployeeData
        [Route("api/Analytic/EmployeeProjectDataLastMonth")]
        [HttpGet]
        public IHttpActionResult EmployeeProjectDataLastMonth()
        {
            var date = DateTime.Now.AddMonths(-1);
            return Ok(BLL.DataAnalyticsForEmployees(date));
        }
        [Route("api/Analytic/GetEmployeesTasksBetweenDates")]
        [HttpGet]
        public IHttpActionResult GetEmployeesTasksBetweenDates(DateTime fromDate, DateTime toDate)
        {
            if (fromDate <= toDate)
            {
                return Ok(BLL.DataAnalyticsEmployeesBetweenDates(fromDate, toDate));
            }
            return BadRequest("The End date must be greater than the Start date");          
        }
        
       [Route("api/Analytic/EmployeeProjectDataLastWeek")]
        [HttpGet]
        public IHttpActionResult EmployeeProjectDataLastWeek()
        {
            var date = DateTime.Now.AddDays(-7);
           

            return Ok(BLL.DataAnalyticsForEmployees(date));

        }
        [Route("api/Analytic/EmployeeProjectDataLastDay")]
        [HttpGet]
        public IHttpActionResult EmployeeProjectDataLastDay()
        {
            var date = DateTime.Now.AddDays(-1);
            return Ok(BLL.DataAnalyticsForEmployees(date));
        }
        #endregion

    }
}
