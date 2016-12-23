using MongoDB.Bson;

namespace Mofichan.DataAccess.Domain
{
    internal class AnalysisArticle
    {
        public ObjectId Id { get; set; }

        public TaggedMessage Article { get; set; }
    }
}