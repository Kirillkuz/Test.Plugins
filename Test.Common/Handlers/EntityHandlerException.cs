using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Common.Handlers
{
    /*
     * Exception class used by all EntityHandlers.
     */
    public class EntityHandlerException : Exception
    {
        public EntityHandlerException(string message) : base(message)
        {
        }

        public EntityHandlerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
