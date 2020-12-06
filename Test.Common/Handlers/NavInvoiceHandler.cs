using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Test.Common.Repository;
using Test.Entities;

namespace Test.Common.Handlers
{
    /*
     * Performs operations on nav_invoice Entities.
     */
    public class NavInvoiceHandler : EntityHandler
    {
        public NavInvoiceHandler(IOrganizationService service, ITracingService tracingService) : base(service, tracingService)
        {
        }

        /*
         * Updates nav_fact and nav_factsum fields of invoice's related agreement based on all it's related paid invoices.
         *
         * invoice - nav_invoice;
         * invoicesToExclude - IDs of nav_invoices to exclude from the related invoices list.
         *
         * Returns true if updated, false otherwise.
         */
        public bool UpdateRelatedAgreementFactData(nav_invoice invoice, params Guid[] invoicesToExclude)
        {
            Guid relatedAgreementId;
            BaseRepository<nav_invoice> invoiceRepo = new BaseRepository<nav_invoice>(Service, nav_invoice.EntityLogicalName);
            BaseRepository<nav_agreement> agreementRepo = new BaseRepository<nav_agreement>(Service, nav_agreement.EntityLogicalName);

            // Checking if invoice contains related agreement ID. If not, obtaining it from CRM.)
            if (invoice.nav_dogovorid != null)
            {
                relatedAgreementId = invoice.nav_dogovorid.Id;
            }
            else
            {
                relatedAgreementId = invoiceRepo.Get(invoice.Id, new ColumnSet(nav_invoice.Fields.nav_dogovorid)).nav_dogovorid.Id;
            }

            TracingService.Trace($"relatedAgreementId={relatedAgreementId}");

            // Obtaining related agreement sum from CRM.
            nav_agreement relatedAgreement = agreementRepo.Get(relatedAgreementId, new ColumnSet(nav_agreement.Fields.nav_summa));

            // Getting total paid invoices sum.
            Decimal totalPaidInvoiceSum = GetRelatedNavInvoicesSum(relatedAgreementId, invoicesToExclude);

            relatedAgreement.nav_factsumma = new Money(totalPaidInvoiceSum);
            relatedAgreement.nav_fact = relatedAgreement.nav_summa.Value == totalPaidInvoiceSum;

            // Updating related agreement.
            agreementRepo.Update(relatedAgreement);

            TracingService.Trace($"Updated agreement with ID={relatedAgreementId}, totalPaidInvoiceSum={totalPaidInvoiceSum}, nav_fact={relatedAgreement.nav_fact}.");

            return true;
        }

        /*
         * Validates sum of all related agreement's paid invoices and updates invoice's nav_paydate on success.
         *
         * invoice - nav_invoice;
         * skipDbUpdate - if true, then no data will be written to DB.
         */
        public void UpdateInvoiceDate(nav_invoice invoice, bool skipDbUpdate = false)
        {
            if (!ValidateRelatedAgreementInvoicesSum(invoice))
            {
                throw new EntityHandlerException("Сумма созданных счетов превышает сумму связанного договора.");
            }

            TracingService.Trace("invoices sum validated, setting nav_paydate to current DateTime.");

            invoice.nav_paydate = DateTime.Now;

            if (skipDbUpdate) return;

            BaseRepository<nav_invoice> invoiceRepo = new BaseRepository<nav_invoice>(Service, nav_invoice.EntityLogicalName);
            invoiceRepo.Update(invoice);

            TracingService.Trace($"Updated invoice with ID={invoice.Id} in DB.");
        }

        /*
         * Checks if sum of all related agreement's paid invoices is bigger than it's own sum.
         *
         * invoice - nav_invoice.
         *
         * Returns true if sum of all related agreement's paid invoices <= agreement sum; false otherwise.
         */
        private bool ValidateRelatedAgreementInvoicesSum(nav_invoice invoice)
        {
            nav_agreement relatedAgreement = new nav_agreement();
            BaseRepository<nav_invoice> invoiceRepo = new BaseRepository<nav_invoice>(Service, nav_invoice.EntityLogicalName);

            // Checking if invoice contains related agreement ID. If not, obtaining it from CRM.
            if (invoice.nav_dogovorid != null)
            {
                relatedAgreement.Id = invoice.nav_dogovorid.Id;
            }
            else
            {
                relatedAgreement.Id = invoiceRepo.Get(invoice.Id, new ColumnSet(nav_invoice.Fields.nav_dogovorid)).nav_dogovorid.Id;
            }

            // Getting related agreement sum.
            BaseRepository<nav_agreement> agreementRepo = new BaseRepository<nav_agreement>(Service, nav_agreement.EntityLogicalName);
            relatedAgreement = agreementRepo.Get(relatedAgreement.Id, new ColumnSet(nav_agreement.Fields.nav_summa));

            TracingService.Trace($"relatedAgreementId={relatedAgreement.Id}, invoiceId={invoice.Id}");

            // Getting related paid invoices sum.
            Decimal totalPaidInvoiceSum = GetRelatedNavInvoicesSum(relatedAgreement.Id);

            return totalPaidInvoiceSum <= relatedAgreement.nav_summa.Value;
        }

        /*
         * Retrieves sum of all agreement's related paid invoices.
         *
         * agreementId - GUID of nav_agreementl;
         * invoicesToExclude - IDs of nav_invoices to exclude from the related invoices list.
         *
         * returns sum of all agreement's related paid invoices nav_amounts.
         */
        private Decimal GetRelatedNavInvoicesSum(Guid agreementId, params Guid[] invoicesToExclude)
        {
            QueryExpression query = new QueryExpression();
            query.EntityName = nav_invoice.EntityLogicalName;
            query.Criteria.AddCondition(nav_invoice.Fields.nav_dogovorid, ConditionOperator.Equal, agreementId);
            query.Criteria.AddCondition(nav_invoice.Fields.nav_fact, ConditionOperator.Equal, true);

            foreach (Guid guid in invoicesToExclude)
            {
                query.Criteria.AddCondition(nav_invoice.Fields.nav_invoiceId, ConditionOperator.NotEqual, guid);
            }

            query.ColumnSet = new ColumnSet(nav_invoice.Fields.nav_amount);

            BaseRepository<Entity> entitiesRepo = new BaseRepository<Entity>(Service, nav_invoice.EntityLogicalName);
            EntityCollection ec = entitiesRepo.GetMultiple(query);

            TracingService.Trace($"Retrieved nav_invoices. ec={ec}, ec.Entities={ec.Entities}, ec.Entities.Count={ec.Entities.Count}");

            decimal totalInvoiceAmount = 0M;

            foreach (Entity invoice in ec.Entities)
            {
                totalInvoiceAmount += invoice.GetAttributeValue<Money>(nav_invoice.Fields.nav_amount).Value;
            }

            return totalInvoiceAmount;
        }
    }
}
