using System;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Common
{
    public abstract class MainPlugin : IPlugin
    {
        protected ITracingService TraceService { get; private set; }
        protected IPluginExecutionContext PluginExecutionContext { get; private set; }
        protected IOrganizationServiceFactory ServiceFactory { get; private set; }

        /*
         * This method should always be called first in the Execute() override in derived classes.
         */
        public virtual void Execute(IServiceProvider serviceProvider)
        {
            TraceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
        }

        /*
         * Creates a new IOrganizationService using current service provider.
         *
         * useCurrentUser - if true, the resulting service will be created with the current user privileges; otherwise a SYSTEM service will be created;
         *
         * Returns a new IOrganizationService.
         */
        protected IOrganizationService CreateService(bool useCurrentUser = true)
        {
            return ServiceFactory.CreateOrganizationService(useCurrentUser ? Guid.Empty : (Guid?)null);
        }

        /*
         * Returns Target Input Parameter cast to the specified type.
         *
         * T - type to cast the parameter to.
         *
         * Returns Target Input Parameter cast to (T).
         */
        public T GetTargetAs<T>() where T : class
        {
            return PluginExecutionContext.InputParameters["Target"] as T;
        }
    }
}
