using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace VAV.Web.Controllers
{
    public class HealthCheckController : ApiController
    {
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [AcceptVerbs("HEAD")]
        public HttpResponseMessage Head()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
