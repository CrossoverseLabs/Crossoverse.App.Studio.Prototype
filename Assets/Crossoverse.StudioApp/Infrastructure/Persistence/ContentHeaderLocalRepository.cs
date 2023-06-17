using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Crossoverse.Domain.Content;

namespace Crossoverse.Infrastructure.Persistence
{
    [CreateAssetMenu(
        menuName = "Crossoverse/LocalRepository/" + nameof(ContentHeaderLocalRepository),
        fileName = nameof(ContentHeaderLocalRepository))]
    public class ContentHeaderLocalRepository : ScriptableObject, IContentHeaderRepository
    {
        [SerializeField] List<ContentHeaderEntity> _entities = new List<ContentHeaderEntity>();

        public async UniTask<ContentHeaderEntity> FindByIdAsync(string id)
        {
            return await Task.Run(() => 
            {
                foreach (var entity in _entities)
                {
                    if (entity.Id == id) return entity;
                }
                return null;
            });
        }

        public List<ContentHeaderEntity> FindByCategory(string category)
        {
            var entities = new List<ContentHeaderEntity>();

            foreach (var entity in _entities)
            {
                if (entity.Category == category)
                {
                    entities.Add(entity);
                }
            }

            return entities;
        }
    }
}