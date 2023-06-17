using Crossoverse.Context;

namespace Crossoverse.StudioApp.Test.Context
{
    public class ContentResourceContextMock : IContentResourceContext
    {
        public string CurrentContentName { get; private set; }
        public string CurrentContentId { get; private set; }
        public string NextContentId { get; private set; }

        public ContentResourceContextMock()
        {
            CurrentContentName = "None";
            CurrentContentId = "None";
        }

        public void SetNextContentId(string contentId)
        {
            UnityEngine.Debug.Log($"[ContentResourceContextMock] SetNextContentId: {contentId}");
            NextContentId = contentId;
        }
    }
}