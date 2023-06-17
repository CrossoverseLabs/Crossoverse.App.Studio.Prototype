namespace Crossoverse.Context
{
    public interface IContentResourceContext
    {
        string CurrentContentName { get; }
        string CurrentContentId { get; }
        string NextContentId { get; }
        void SetNextContentId(string contentId);
    }
}