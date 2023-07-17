using System.Collections.Generic;
using System.Threading.Tasks;
using Crossoverse.Domain.Avatar;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Crossoverse.Infrastructure.Persistence
{
    [CreateAssetMenu(
        menuName = "Crossoverse/LocalRepository/" + nameof(AvatarMetadataLocalRepository),
        fileName = nameof(AvatarMetadataLocalRepository))]
    public class AvatarMetadataLocalRepository : ScriptableObject, IAvatarMetadataRepository
    {
        [SerializeField] List<AvatarMetadata> _entities = new();

        public async UniTask<AvatarMetadata> FindFirstAsync()
        {
            return (_entities.Count > 0) ? _entities[0] : null;
        }

        public async UniTask<AvatarMetadata[]> FindByOwnerIdAsync(string ownerId)
        {
            var entities = new List<AvatarMetadata>();

            await Task.Run(() =>
            {
                foreach (var entity in _entities)
                {
                    if (entity.OwnerId == ownerId)
                    {
                        entities.Add(entity);
                    }
                }
            });

            return entities.ToArray();
        }
    }
}