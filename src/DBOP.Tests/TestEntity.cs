using DBOP.Attributes;
using DBOP.Interfaces;

namespace DBOP.Tests
{
    [Table("TEST_TABLE")]
    public class TestEntity : IEntityBase
    {
        [Column("ID"), PrimaryKey]
        public int? ID { get; set; }

        [Column("NAME")]
        public string? Name { get; set; }

        [Column("VALUE")]
        public int? Value { get; set; }

        [Column("FLAG")]
        public bool? Flag { get; set; }

        [Column("CREATED_DATE")]
        public DateTime? CreatedDate { get; set; }

        [Column("MODIFIED_DATE")]
        public DateTime? ModifiedDate { get; set; }

        [Column("SUB_TEST_TABLE_ID")]
        public int? SubTestTableID { get; set; }

        [Relationship, ForeignKey("SUB_TEST_TABLE_ID")]
        public IEnumerable<SubTestEntity>? SubTableList { get; set; }
    }

    [Table("SUB_TEST_TABLE")]
    public class SubTestEntity : IEntityBase
    {
        [Column("ID"), PrimaryKey]
        public int ID { get; set; }

        [Column("VALUE")]
        public string? Value { get; set; }
    }
}