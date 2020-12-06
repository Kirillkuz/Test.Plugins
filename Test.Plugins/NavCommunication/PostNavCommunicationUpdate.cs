using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common;
using Test.Common.Handlers;
using Test.Entities;

namespace Test.Plugins.NavCommunication
{
    public class PostNavCommunicationUpdate : MainPlugin
    {
        public override void Execute(IServiceProvider serviceProvider)
        {
            base.Execute(serviceProvider);

            try
            {
                nav_communication createdCommunication = GetTargetAs<Entity>().ToEntity<nav_communication>();
                IOrganizationService currentUserService = CreateService();

                NavCommunicationHandler communicationHandler = new NavCommunicationHandler(currentUserService, TraceService);

                // 5.6
                communicationHandler.CheckMultipleMainCommunications(createdCommunication);
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
