using System;
using Cysharp.Threading.Tasks;

namespace Crossoverse.Context
{
    public sealed class ContentStageContext
    {
        private readonly IContentResourceContext _resourceContext;

        public ContentStageContext
        (
            IContentResourceContext resourceContext
        )
        {
            _resourceContext = resourceContext;
        }

        public async UniTask InitializeAsync()
        {
            var contentId = _resourceContext.NextContentId;
            await _resourceContext.LoadContentResourceInfoAsync(contentId);
            await _resourceContext.LoadSkyboxAsync();
        }
    }
}