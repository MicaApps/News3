using SQLite;

namespace freeRSS.Schema
{
    public class FeedInfo
    {
        [PrimaryKey, AutoIncrement]
        public int? Id { get; set; }

        [MaxLength(40), NotNull]
        public string Name { get; set; }

        [NotNull]
        public string Source { get; set; }

        public string IconSrc { get; set; }

        [NotNull]
        public string LastBuildedTime { get; set; }

        [MaxLength(96)]
        public string Description { get; set; }
    }
}
