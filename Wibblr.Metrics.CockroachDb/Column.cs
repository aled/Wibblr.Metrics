namespace Wibblr.Metrics.CockroachDb
{
    public class Column
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public string DefaultFunction { get; set; }

        private string DefaultClause
        {
            get => string.IsNullOrEmpty(DefaultFunction) ? "" : $"DEFAULT {DefaultFunction}";
        }

        public override string ToString() =>
            $"{Name} {DataType} {DefaultClause}";
    }
}
