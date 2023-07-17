using System;
using Cysharp.Threading.Tasks;
using Crossoverse.Domain.Avatar;

namespace Crossoverse.Core.Context
{
    public sealed class PlayerContext
    {
        private readonly IAvatarMetadataRepository _avatarMetadataRepository;
        private readonly AvatarContext _avatarContext;

        private IDisposable _disposable;

        public PlayerContext
        (
            IAvatarMetadataRepository avatarMetadataRepository,
            AvatarContext avatarContext
        )
        {
            _avatarMetadataRepository = avatarMetadataRepository;
            _avatarContext = avatarContext;
        }

        public async UniTask InitializeAsync()
        {
            var avatar = await _avatarMetadataRepository.FindFirstAsync();
            if (avatar != null)
            {
                var resourcePath = avatar.ResourcePath.Replace("[StreamingAssetsPath]", UnityEngine.Application.streamingAssetsPath);
                await _avatarContext.LoadAvatarResourceAsync(resourcePath);
            }

            UnityEngine.Debug.Log($"[{nameof(PlayerContext)}] Initialized"); 
        }
    }
}