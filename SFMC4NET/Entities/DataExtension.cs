using System.Collections.Generic;

namespace SFMC4NET.Entities
{
    public enum DataType { None, Text, Number, Date, EmailAddress, Boolean, Decimal };

    public class DataExtension
    {
        public string Name { get; set; }
        public string CustomerKey { get; set; }
        public long FolderId { get; set; } = 0;
        public List<DataExtensionColumn> Columns { get; set; }
    }
        
    public class DataExtensionColumn
    {
        public string Name { get; set; }
        public int MaxLength { get; set; } = 0;
        public DataType InferedType { get; set; } = DataType.None;
        public bool IsNullable { get; set; } = false;
    }
}
