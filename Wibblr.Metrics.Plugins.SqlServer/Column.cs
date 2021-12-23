using System;

namespace Wibblr.Metrics.Plugins.SqlServer
{
    public class Column
    {
        public string Name { get; set; }
        
        public string DataType { get; set; }

        public Type Type { get; set; }

        public bool Identity { get; set; }

        private string IdentityClause
        {
            get => Identity ? " IDENTITY" : $"";
        }

        public Column(string name, string dataType, bool identity = false)
        {
            Name = name;
            DataType = dataType;
            Identity = identity;
        }

        public Column(string name, string dataType, Type type)
        {
            Name = name;
            DataType = dataType;
            Type = type;
        }


        public override string ToString() =>
            $"{Name} {DataType}{IdentityClause} NOT NULL";
    }
}
