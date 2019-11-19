using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EmployeesTimesheet.API.Controllers
{
    public class AnalyticController : ApiController
    {
        // GET: api/Analytic
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Analytic/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Analytic
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Analytic/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Analytic/5
        public void Delete(int id)
        {
        }
    }
}
