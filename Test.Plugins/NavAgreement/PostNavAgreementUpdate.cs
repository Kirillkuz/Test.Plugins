using System;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common;
using Test.Entities;
using Test.Common.Handlers;

namespace Test.Plugins.NavAgreement
{
    public class PostNavAgreementUpdate : MainPlugin
    {
        public override void Execute(IServiceProvider serviceProvider)
        {
            base.Execute(serviceProvider);

            if (PluginExecutionContext.Depth > 1)
            {
                // Recursive update.
                return;
            }

            try
            {
                nav_agreement updatedAgreement = GetTargetAs<Entity>().ToEntity<nav_agreement>();
                IOrganizationService currentUserService = CreateService();

                NavAgreementHandler agreementHandler = new NavAgreementHandler(currentUserService, TraceService);

                // 5.5 
                agreementHandler.UpdateFact(updatedAgreement);
            }
            catch (EntityHandlerException e)
            {
                TraceService.Trace(e.ToString());
                throw new InvalidPluginExecutionException(e.Message);
            }
            catch (Exception e)
            {
                TraceService.Trace(e.ToString());
                throw new InvalidPluginExecutionException("Возникла ошибка, см. журнал для подробностей.");
            }
        }
    }
}
