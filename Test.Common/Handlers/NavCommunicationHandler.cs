using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common.Repository;
using Test.Entities;

namespace Test.Common.Handlers
{
    /*
     * Performs operations on nav_communication Entities.
     */
    public class NavCommunicationHandler : EntityHandler
    {
        public NavCommunicationHandler(IOrganizationService service, ITracingService tracingService) : base(service, tracingService)
        {
        }

        /*
         * Checks if there are already present nav_communication with nav_main=true and the same nav_type as provided agreement's.
         * Throws an InvalidPluginExecutionException if duplicates were found, returns otherwise.
         *
         * communication - created/updated nav_communication.
         */
        public void CheckMultipleMainCommunications(nav_communication communication)
        {
            BaseRepository<nav_communication> communicationRepo = new BaseRepository<nav_communication>(Service, nav_communication.EntityLogicalName);

            // Checking if all required communication data is present. If not, obtaining it from CRM.
            if (communication.nav_main == null || communication.nav_type == null || communication.nav_contactid == null)
            {
                communication = communicationRepo.Get(communication.Id, new ColumnSet(nav_communication.Fields.nav_main, nav_communication.Fields.nav_type, nav_communication.Fields.nav_contactid));
            }

            TracingService.Trace($"relatedContactId={communication.nav_contactid}, communicationId={communication.Id}, nav_main={communication.nav_main}, nav_type={communication.nav_type}");

            // No need to check non-set objects.
            if (communication.nav_main == null || communication.nav_main == false || communication.nav_type == null || communication.nav_contactid == null)
            {
                return;
            }

            // Getting all other communications related to our contact with their nav_main=true and the same nav_type.
            QueryExpression query = new QueryExpression();
            query.Criteria.AddCondition(nav_communication.Fields.nav_contactid, ConditionOperator.Equal, communication.nav_contactid.Id);
            query.Criteria.AddCondition(nav_communication.Fields.nav_type, ConditionOperator.Equal, communication.nav_type.Value);
            query.Criteria.AddCondition(nav_communication.Fields.nav_main, ConditionOperator.Equal, true);
            query.Criteria.AddCondition(nav_communication.Fields.nav_communicationId, ConditionOperator.NotEqual, communication.Id);
            query.ColumnSet = new ColumnSet(false);

            query.ColumnSet = new ColumnSet(false);

            EntityCollection ec = communicationRepo.GetMultiple(query);

            TracingService.Trace($"Retrieved nav_communications. ec={ec}, ec.Entities={ec.Entities}, ec.Entities.Count={ec.Entities.Count}");

            if (ec.Entities.Count > 0)
            {
                // Another main communication with the same type is already present.
                throw new EntityHandlerException("Основное средство связи с заданным типом уже существует для связанного контакта.");
            }
        }
    }
}
