using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Test.Common.Repository
{
    /*
     * Base Repository Pattern interface with CRUD operations.
     *
     * T - repository Entity type.
     */
    public interface IRepository<T> where T : Entity
    {
        /*
         * Retrieves Entity with the specified ID.
         *
         * id - GUID of the Entity to retrieve;
         * columns - Entity columns to retrieve.
         *
         * Returns entity with the specified id.
         */
        T Get(Guid id, ColumnSet columns);

        /*
         * Retrieves multiple Entities based on the specified QueryExpression.
         *
         * query - QueryExpression to apply as a filter; if null, all existing Entities are retrieved.
         *
         * Retuns EntityCollection filtered by query.
         */
        EntityCollection GetMultiple(QueryExpression query = null);

        /*
         * Updates Entity data.
         *
         * entity - Entity to update.
         */
        void Update(T entity);

        /*
         * Deletes Entity with the specified ID.
         *
         * id - GUID of the Entity to delete.
         */
        void Delete(Guid id);

        /*
         * Creates new Entity.
         *
         * entity - Entity to create.
         *
         * Returns GUID of newly created Entity.
         */
        Guid Insert(T entity);
    }
}
