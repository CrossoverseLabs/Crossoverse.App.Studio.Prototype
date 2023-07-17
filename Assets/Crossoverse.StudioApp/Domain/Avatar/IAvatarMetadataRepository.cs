using Cysharp.Threading.Tasks;

namespace Crossoverse.Domain.Avatar
{
    public interface IAvatarMetadataRepository
    {
        UniTask<AvatarMetadata> FindFirstAsync();
        UniTask<AvatarMetadata[]> FindByOwnerIdAsync(string ownerId);
    }
}