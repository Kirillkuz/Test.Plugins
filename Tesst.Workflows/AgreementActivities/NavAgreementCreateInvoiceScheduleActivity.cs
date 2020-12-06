using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common.Handlers;
using Test.Common.Workflows;
using Test.Entities;

namespace Tesst.Workflows.AgreementActivities
{
    class NavAgreementCreateInvoiceScheduleActivity : BaseWorkflowActivity
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

                // 6.2
                agreementHandler.CreateRelatedCreditInvoices(agreement);

            }
            catch (Exception e)
            {
                // Can't display any errors since it's a background activity.
                TraceService.Trace(e.ToString());
            }
        }
    }
}