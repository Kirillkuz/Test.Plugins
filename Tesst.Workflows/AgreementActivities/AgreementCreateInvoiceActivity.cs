using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common.Handlers;
using Test.Common.Workflows;
using Test.Entities;

namespace Tesst.Workflows.AgreementActivities
{
    public sealed class AgreementCreateInvoiceActivity : BaseWorkflowActivity
    {

            [ReferenceTarget(nav_agreement.EntityLogicalName)]
            [Input(nav_agreement.EntityLogicalName)]
            [RequiredArgument]
            public InArgument<EntityReference> Agreement { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            Init(context);

            try
            {
                IOrganizationService currentUserService = CreateService();
                NavAgreementHandler agreementHandler = new NavAgreementHandler(currentUserService, TraceService);

                nav_agreement agreement = new nav_agreement
                {
                    Id = Agreement.Get(context).Id
                };

                // 6.1
                agreementHandler.CreateFirstRelatedInvoice(agreement, WorkflowContext.UserId);
            }
            catch (EntityHandlerException e)
            {
                TraceService.Trace(e.ToString());
                throw new InvalidWorkflowException(e.Message);
            }
            catch (Exception e)
            {
                TraceService.Trace(e.ToString());
                throw new InvalidWorkflowException("Возникла ошибка, см. журнал для подробностей.");
            }
        }
    }
    
}
