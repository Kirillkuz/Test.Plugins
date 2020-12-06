using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common;
using Test.Common.Handlers;
using Test.Entities;

namespace Test.Plugins.NavAgreement
{
    public class PreNavAgreementCreate : MainPlugin
    {
        public override void Execute(IServiceProvider serviceProvider)
        {
            base.Execute(serviceProvider);

            try
            {
                nav_agreement createdAgreement = GetTargetAs<Entity>().ToEntity<nav_agreement>();
                IOrganizationService currentUserService = CreateService();

                NavAgreementHandler agreementHandler = new NavAgreementHandler(currentUserService, TraceService);

                // 5.2
                agreementHandler.UpdateRelatedContactDateOnCreate(createdAgreement);
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

