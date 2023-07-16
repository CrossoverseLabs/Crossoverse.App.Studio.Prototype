using Cysharp.Threading.Tasks;
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
            UnityEngine.Debug.Log($"<color=orange>[ContentResourceContextMock] SetNextContentId: {contentId}</color>");
            NextContentId = contentId;
        }

        public async UniTask<bool> LoadContentResourceInfoAsync(string contentId)
        {
            UnityEngine.Debug.Log($"<color=orange>[ContentResourceContextMock] LoadContentResourceInfoAsync: {contentId}</color>");
            return true;
        }

        public async UniTask LoadSkyboxAsync()
        {
            UnityEngine.Debug.Log($"<color=orange>[ContentResourceContextMock] LoadSkyboxAsync</color>");
        }
    }
}