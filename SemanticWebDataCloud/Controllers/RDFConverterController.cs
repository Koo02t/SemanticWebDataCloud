using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SemanticWebDataCloud.Controllers
{
    public class RDFConverterController : Controller
    {
        // GET: VIrtuoso
        [HttpPost]
        [Route("api/Converter/jobs")]
        public string LBDConverter([FromBody] QueryObject objModel)
        {
        }

    }

}