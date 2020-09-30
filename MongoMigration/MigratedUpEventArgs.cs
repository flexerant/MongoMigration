using System;
using System.Collections.Generic;
using System.Text;

namespace Flexerant.MongoMigration
{
    public class MigratedUpEventArgs : EventArgs
    {
        public MigratedItem Migration { get; private set; }

        public MigratedUpEventArgs(MigratedItem migration)
        {
            this.Migration = migration;
        }
    }
}
