namespace DatabaseMigrations.Database
{
    public class TableJournalOptions
    {
        public string DoesJournalTableExistSql { get; set; } = "";
        public string CreateJournalTableSql { get; set; } = "";
        public string RetrieveEntriesSql { get; set; } = "";
        public string InsertEntrySql { get; set; } = "";
    }
}
