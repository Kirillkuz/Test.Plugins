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
    public class PreNavInvoiceDelete : MainPlugin
    {
        public override void Execute(IServiceProvider serviceProvider)
        {
            base.Execute(serviceProvider);

            try
            {
                EntityReference deletedInvoiceRef = GetTargetAs<EntityReference>();
                nav_invoice deletedInvoice = new nav_invoice
                {
                    Id = deletedInvoiceRef.Id
                };
                IOrganizationService currentUserService = CreateService();

                NavInvoiceHandler invoiceHandler = new NavInvoiceHandler(currentUserService, TraceService);

                // 5.3
                // 5.5
                invoiceHandler.UpdateRelatedAgreementFactData(deletedInvoice, deletedInvoice.Id);
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
