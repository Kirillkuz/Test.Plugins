using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Test.Common.Repository
{
    /*
     * Implementation of the base Repository Pattern interface.
     *
     * T - repository Entity type.
     */
    public class BaseRepository<T> : IRepository<T> where T : Entity
    {
        /*
         * IOrganizationService data provider.
         */
        public IOrganizationService Service { get; }

        /*
         * Repository Entity type logical name.
         */
        public string EntityLogicalName { get; }

        public BaseRepository(IOrganizationService service, string entityLogicalName)
        {
            Service = service ?? throw new ArgumentNullException();
            EntityLogicalName = entityLogicalName ?? throw new ArgumentNullException();
        }

        public T Get(Guid id, ColumnSet columns)
        {
            return (T)Service.Retrieve(EntityLogicalName, id, columns);
        }

        public EntityCollection GetMultiple(QueryExpression query)
        {
            query.EntityName = EntityLogicalName;
            return Service.RetrieveMultiple(query);
        }

        public void Update(T entity)
        {
            Service.Update(entity);
        }

        public void Delete(Guid id)
        {
            Service.Delete(EntityLogicalName, id);
        }

        public Guid Insert(T entity)
        {
            return Service.Create(entity);
        }
    }
}

