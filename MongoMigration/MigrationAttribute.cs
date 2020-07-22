using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Flexerant.MongoMigration
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MigrationAttribute : Attribute
    {
        public int MigrationNumber { get; private set; }

        public MigrationAttribute(int migrationNumber)
        {
            this.MigrationNumber = migrationNumber;
        }
    }
}
