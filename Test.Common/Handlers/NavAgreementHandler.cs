using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Test.Common.Repository;
using Test.Entities;

namespace Test.Common.Handlers
{
    /*
     * Performs operations on nav_agreement Entities.
     */
    public class NavAgreementHandler : EntityHandler
    {
        public NavAgreementHandler(IOrganizationService service, ITracingService tracingService) : base(service, tracingService)
        {
        }

        /*
         * Sets related contact's first agreement date to this agreement's date if there were no previously related agreements for this contact.
         *
         * agreement - created nav_agreement.
         *
         * Returns true if updated, false otherwise.
         */
        public bool UpdateRelatedContactDateOnCreate(nav_agreement agreement)
        {
            BaseRepository<nav_agreement> agreementRepo = new BaseRepository<nav_agreement>(Service, nav_agreement.EntityLogicalName);

            // Checking if there are already existing related agreements.
            QueryExpression query = new QueryExpression
            {
                ColumnSet = new ColumnSet(false),
                TopCount = 1
            };
            query.Criteria.AddCondition(nav_agreement.Fields.nav_contact, ConditionOperator.Equal, agreement.nav_contact.Id);

            if (agreementRepo.GetMultiple(query).Entities.Count < 1)
            {
                // Updating contact's date since this is their first related agreement.
                BaseRepository<Contact> contactRepo = new BaseRepository<Contact>(Service, Contact.EntityLogicalName);
                contactRepo.Update(new Contact
                {
                    Id = agreement.nav_contact.Id,
                    nav_date = agreement.nav_date
                });

                return true;
            }

            return false;
        }

        /*
         * Updates nav_fact based on agreement's nav_summa and nav_factsumma values.
         *
         * agreement - nav_agreement to update;
         * skipDbUpdate - if true, then no data will be written to DB.
         */
        public void UpdateFact(nav_agreement agreement, bool skipDbUpdate = false)
        {
            BaseRepository<nav_agreement> agreementRepo = new BaseRepository<nav_agreement>(Service, nav_agreement.EntityLogicalName);

            // Checking if agreement contains required sum data. If not, obtaining it from CRM.
            if (agreement.nav_summa == null || agreement.nav_factsumma == null)
            {
                agreement = agreementRepo.Get(agreement.Id, new ColumnSet(nav_agreement.Fields.nav_summa, nav_agreement.Fields.nav_factsumma));
            }

            TracingService.Trace($"agreementId={agreement.Id}");

            agreement.nav_fact = Equals(agreement.nav_summa, agreement.nav_factsumma);

            if (skipDbUpdate) return;

            agreementRepo.Update(agreement);
            TracingService.Trace($"agreementId={agreement.Id} updated in DB.");
        }

        /*
         * Creates new related nav_invoice if there are no other related invoices for the agreement.
         *
         * agreement - nav_agreement;
         * emailNotificationSenderId - GUID of the User whose email will be used to send a notification.
         */
        public void CreateFirstRelatedInvoice(nav_agreement agreement, Guid emailNotificationSenderId)
        {
            TracingService.Trace($"agreementId={agreement.Id}");

            if (HasRelatedInvoices(agreement))
            {
                return;
            }

            // Checking if agreement contains required data. If not, obtaining it from CRM.
            if (agreement.nav_name == null || agreement.nav_summa == null || agreement.nav_contact == null)
            {
                BaseRepository<nav_agreement> agreementRepo = new BaseRepository<nav_agreement>(Service, nav_agreement.EntityLogicalName);
                agreement = agreementRepo.Get(agreement.Id, new ColumnSet(nav_agreement.Fields.nav_name, nav_agreement.Fields.nav_summa, nav_agreement.Fields.nav_contact));
            }

            // Creating new invoice.
            nav_invoice newRelatedInvoice = new nav_invoice();
            newRelatedInvoice.nav_name = string.Format("Счет для договора {0}", agreement.nav_name);
            newRelatedInvoice.nav_paydate = newRelatedInvoice.nav_date = DateTime.Now;
            newRelatedInvoice.nav_dogovorid = new EntityReference(nav_agreement.EntityLogicalName, agreement.Id);
            newRelatedInvoice.nav_fact = false;
            newRelatedInvoice.nav_type = true;
            newRelatedInvoice.nav_amount = agreement.nav_summa;

            BaseRepository<nav_invoice> invoiceRepo = new BaseRepository<nav_invoice>(Service, nav_invoice.EntityLogicalName);
            Guid createdInvoiceId = invoiceRepo.Insert(newRelatedInvoice);

            TracingService.Trace($"Created new related nav_invoice with ID={createdInvoiceId} for agreementId={agreement.Id}");

            // Getting agreement's related contact.
            BaseRepository<Contact> contactRepo = new BaseRepository<Contact>(Service, Contact.EntityLogicalName);
            Contact relatedContact = contactRepo.Get(agreement.nav_contact.Id, new ColumnSet(Contact.Fields.FullName, Contact.Fields.EMailAddress1, Contact.Fields.DoNotBulkEMail));

            // Sending email to related contact if they hasn't opted out of emails and their email address is set.
            if (relatedContact.DoNotBulkEMail.HasValue && !relatedContact.DoNotBulkEMail.Value && relatedContact.EMailAddress1 != null)
            {
                // TODO: implement

                TracingService.Trace($"Sent notification email to related contact with ID={relatedContact.Id} at email={relatedContact.EMailAddress1}");
            }
        }

        public void CreateRelatedCreditInvoices(nav_agreement agreement)
        {
            TracingService.Trace($"agreementId={agreement.Id}");

            BaseRepository<nav_agreement> agreementRepo = new BaseRepository<nav_agreement>(Service, nav_agreement.EntityLogicalName);
            BaseRepository<nav_invoice> invoiceRepo = new BaseRepository<nav_invoice>(Service, nav_invoice.EntityLogicalName);

            // Retrieving all related invoices.
            QueryExpression query = new QueryExpression();
            query.ColumnSet = new ColumnSet(nav_invoice.Fields.nav_type, nav_invoice.Fields.nav_fact);
            query.Criteria.AddCondition(nav_invoice.Fields.nav_dogovorid, ConditionOperator.Equal, agreement.Id);

            EntityCollection relatedInvoices = invoiceRepo.GetMultiple(query);

            TracingService.Trace($"[nav_invoices] ec={relatedInvoices}, ec.Entities={relatedInvoices.Entities}, ec.Entities.Count={relatedInvoices.Entities.Count}");

            // Checking if there are any paid or manually created related invoices.
            foreach (Entity entity in relatedInvoices.Entities)
            {
                nav_invoice invoice = (nav_invoice)entity;

                if (invoice.nav_fact == true || invoice.nav_type ==  true)
                {
                    return;
                }
            }

            // Checking if agreement contains required data. If not, obtaining it from CRM.
            if (agreement.nav_creditid == null || agreement.nav_creditperiod == null || agreement.nav_creditamount == null || agreement.nav_name == null)
            {
                agreement = agreementRepo.Get(agreement.Id, new ColumnSet(nav_agreement.Fields.nav_creditid, nav_agreement.Fields.nav_creditperiod, nav_agreement.Fields.nav_creditamount, nav_agreement.Fields.nav_name));

                // Checking if agreement has all required optional fields set.
                if (agreement.nav_creditid == null || agreement.nav_creditperiod == null || agreement.nav_creditamount == null) return;
            }

            // Deleting all existing automatically created related invoices. 
            foreach (Entity entity in relatedInvoices.Entities)
            {
                invoiceRepo.Delete(entity.Id);
            }

            // Creating new related invoices based on the credit information.
            int creditPeriodMonths = agreement.nav_creditperiod.Value * 12;
            Decimal monthlyCreditAmount = agreement.nav_creditamount.Value / creditPeriodMonths;
            DateTime nextInvoiceDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);

            for (int i = 0; i < creditPeriodMonths; ++i)
            {
                nav_invoice newRelatedInvoice = new nav_invoice();
                newRelatedInvoice.nav_name = string.Format("Кредитный счет #{0} для договора {1}", i + 1, agreement.nav_name);
                newRelatedInvoice.nav_date = nextInvoiceDate;
                newRelatedInvoice.nav_paydate = newRelatedInvoice.nav_date;
                newRelatedInvoice.nav_dogovorid = agreement.ToEntityReference();
                newRelatedInvoice.nav_fact = false;
                newRelatedInvoice.nav_type =  true;
                newRelatedInvoice.nav_amount = new Money(monthlyCreditAmount);

                invoiceRepo.Insert(newRelatedInvoice);

                nextInvoiceDate = nextInvoiceDate.AddMonths(1);
            }

            // Updating agreement's payment plan date.
            nav_agreement agreementToUpdate = new nav_agreement();
            agreementToUpdate.Id = agreement.Id;
            agreementToUpdate.nav_paymentplandate = DateTime.Now.AddDays(1);

            agreementRepo.Update(agreementToUpdate);

            TracingService.Trace($"Created {creditPeriodMonths} new nav_invoices related to nav_agreement with ID={agreement.Id}");
        }

        private bool HasRelatedInvoices(nav_agreement agreement)
        {
            TracingService.Trace($"agreementId={agreement.Id}");

            BaseRepository<nav_invoice> invoiceRepo = new BaseRepository<nav_invoice>(Service, nav_invoice.EntityLogicalName);

            // Retrieving the first related nav_invoice.
            QueryExpression query = new QueryExpression();
            query.Criteria.AddCondition(nav_invoice.Fields.nav_dogovorid, ConditionOperator.Equal, agreement.Id);
            query.ColumnSet = new ColumnSet(false);
            query.TopCount = 1;

            EntityCollection ec = invoiceRepo.GetMultiple(query);

            TracingService.Trace($"ec={ec}, ec.Entities={ec.Entities}, ec.Entities.Count={ec.Entities.Count}");

            return ec.Entities.Count > 0;
        }
    }
}