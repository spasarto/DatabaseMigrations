namespace DatabaseMigrations.Database
{
    public struct Migration
    {
        public string Id { get; private set; }
        public string Contents { get; private set; }

        public Migration(string id, string contents)
        {
            Id = id;
            Contents = contents;
        }

        public override bool Equals(object obj) => obj is Migration migration && migration.Id == Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}
