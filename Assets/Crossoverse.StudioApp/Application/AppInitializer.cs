using MessagePack.Resolvers;
using UnityEngine;

namespace Crossoverse.StudioApp.Application
{
    class AppInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            RegisterResolvers();
            DevelopmentOnlyLogger.Log($"<color=lime>[{nameof(AppInitializer)}] Resolvers have been registered</color>");
            Crossoverse.Toolkit.Serialization.MessagePackMessageSerializer.DefaultOptions = MessagePack.MessagePackSerializer.DefaultOptions;
        }

        static void RegisterResolvers()
        {
            // NOTE: Currently, CompositeResolver doesn't work on Unity IL2CPP build. Use StaticCompositeResolver instead of it.
            StaticCompositeResolver.Instance.Register(
                GeneratedResolver.Instance,
                BuiltinResolver.Instance,
                PrimitiveObjectResolver.Instance
            );

            MessagePack.MessagePackSerializer.DefaultOptions = MessagePack.MessagePackSerializer.DefaultOptions.WithResolver(StaticCompositeResolver.Instance);
        }
    }
}
