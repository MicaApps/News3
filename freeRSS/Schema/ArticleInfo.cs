using SQLite;

namespace freeRSS.Schema
{
    public class ArticleInfo
    {
        [PrimaryKey, AutoIncrement]
        public int? Id { get; set; }

        [NotNull]
        public int FeedId { get; set; }

        [NotNull]
        public string Title { get; set; }

        public string PubDate { get; set; }
        [NotNull]
        public string Source { get; set; }
        [NotNull]
        public string Description { get; set; }
        public string Rtf { get; set; }
        public string OldRtf { get; set; }

        [NotNull]
        public bool Unread { get; set; }
        [NotNull]
        public bool Isstarred { get; set; }
    }
}
