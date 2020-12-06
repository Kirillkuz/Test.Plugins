using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace Test.Common.Handlers
{
    /*
    * Base class for all CRM Entity handlers.
    */
    public abstract class EntityHandler
    {
        protected IOrganizationService Service { get; }
        protected ITracingService TracingService { get; }

        protected EntityHandler(IOrganizationService service, ITracingService tracingService)
        {
            Service = service;
            TracingService = tracingService;
        }
    }
}
