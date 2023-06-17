using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Crossoverse.Domain.Content;

namespace Crossoverse.Infrastructure.Persistence
{
    [CreateAssetMenu(
        menuName = "Crossoverse/LocalRepository/" + nameof(ResourceInfoLocalRepository),
        fileName = nameof(ResourceInfoLocalRepository))]
    public class ResourceInfoLocalRepository : ScriptableObject, IResourceInfoRepository
    {
        [SerializeField] List<ContentResourceInfo> _entities = new List<ContentResourceInfo>();

        public async UniTask<ContentResourceInfo> FindByContentIdAsync(string contentId)
        {
            return await Task.Run(() => 
            {
                foreach (var entity in _entities)
                {
                    if (entity.ContentId == contentId) return entity;
                }
                return null;
            });
        }
    }
}