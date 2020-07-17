using System;
using System.Collections.Generic;
using System.Text;

namespace MongoMigration
{
    public class MigrationException : Exception
    {
        public MigrationException(string message) : base(message) { }
        public MigrationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
