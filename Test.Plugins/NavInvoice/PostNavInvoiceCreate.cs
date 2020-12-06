using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common;
using Test.Common.Handlers;
using Test.Entities;

namespace Test.Plugins.NavInvoice
{
    public class PostNavInvoiceCreate : MainPlugin
    {
        public override void Execute(IServiceProvider serviceProvider)
        {
            base.Execute(serviceProvider);

            try
            {
                nav_invoice createdInvoice = GetTargetAs<Entity>().ToEntity<nav_invoice>();
                IOrganizationService currentUserService = CreateService();

                NavInvoiceHandler invoiceHandler = new NavInvoiceHandler(currentUserService, TraceService);

                // 5.3
                // 5.5
                invoiceHandler.UpdateRelatedAgreementFactData(createdInvoice);
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
