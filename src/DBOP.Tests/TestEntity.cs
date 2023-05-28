using DBOP.Attributes;
using DBOP.Interfaces;

namespace DBOP.Tests
{
    [TableName("TEST_TABLE")]
    public class TestEntity : IEntityBase
    {
        [PrimaryKey]
        [ColumnName("ID")]
        public int ID { get; set; }

        [ColumnName("NAME")]
        public string? Name { get; set; }

        [ColumnName("VALUE")]
        public int? Value { get; set; }

        [ColumnName("FLAG")]
        public bool? Flag { get; set; }

        [ColumnName("CREATED_DATE")]
        public DateTime? CreatedDate { get; set; }

        [ColumnName("MODIFIED_DATE")]
        public DateTime? ModifiedDate { get; set; }

        [ColumnName("SUB_TEST_TABLE_ID")]
        public int? SubTestTableID { get; set; }

        [Relationship]
        [ForeignKey("SUB_TEST_TABLE_ID")]
        public IEnumerable<SubTestEntity>? SubTableList { get; set; }
    }

    [TableName("SUB_TEST_TABLE")]
    public class SubTestEntity : IEntityBase
    {
        [PrimaryKey]
        [ColumnName("ID")]
        public int ID { get; set; }

        [ColumnName("Value")]
        public string? Value { get; set; }
    }
}