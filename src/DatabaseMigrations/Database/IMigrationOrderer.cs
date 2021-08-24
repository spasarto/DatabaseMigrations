using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseMigrations.Database
{
    public interface IMigrationOrderer
    {
        /// <summary>
        /// Returns the migrations in the order you wish to execute. 
        /// </summary>
        IEnumerable<Migration> OrderMigrations(IEnumerable<Migration> migrations);
    }

    public class FuncMigrationOrderer : IMigrationOrderer
    {
        private readonly Func<IEnumerable<Migration>, IEnumerable<Migration>> orderFunction;

        public FuncMigrationOrderer(Func<IEnumerable<Migration>, IEnumerable<Migration>> orderFunction)
        {
            this.orderFunction = orderFunction;
        }

        public IEnumerable<Migration> OrderMigrations(IEnumerable<Migration> migrations) => orderFunction(migrations);
    }
}
