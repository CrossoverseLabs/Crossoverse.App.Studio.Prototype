using System;
using System.Threading.Tasks;
using Crossoverse.Core.Domain.ResourceProvider;
using UnityEngine;

namespace Crossoverse.Core.Context
{
    public sealed class AvatarContext
    {
        public event Action OnLoaded;

        private readonly IAvatarResourceProvider _avatarResourceProvider;
        private readonly RuntimeAnimatorController _runtimeAnimatorController;

        public AvatarContext
        (
            IAvatarResourceProvider avatarResourceProvider,
            RuntimeAnimatorController animatorController
        )
        {
            _avatarResourceProvider = avatarResourceProvider;
            _runtimeAnimatorController = animatorController;
        }

        public async Task LoadAvatarResourceAsync(string path)
        {
            var avatarAnimator = await _avatarResourceProvider.LoadAsync(path);

            avatarAnimator.runtimeAnimatorController = _runtimeAnimatorController;

            avatarAnimator.transform.position = new Vector3(0, 0, 1.5f);
            avatarAnimator.transform.eulerAngles = new Vector3(0, 180, 0);

            OnLoaded?.Invoke();
        }
    }
}