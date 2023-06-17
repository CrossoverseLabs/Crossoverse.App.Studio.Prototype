using Cysharp.Threading.Tasks;

namespace Crossoverse.Domain.Content
{
    public interface IResourceInfoRepository
    {
        UniTask<ContentResourceInfo> FindByContentIdAsync(string contentId);
    }
}