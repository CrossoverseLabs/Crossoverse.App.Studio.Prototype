using System;
using Crossoverse.Core.Domain;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Crossoverse.Core.Unity.Behaviour
{
    [DisallowMultipleComponent]
    public class GuidComponent : MonoBehaviour, IGuidComponent
    {
        public Guid Guid => new Guid(_guidString);

        [SerializeField] string _guidString = Guid.Empty.ToString("N");

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateGuid();
        }

        void  UpdateGuid()
        {
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(this);
            if (string.IsNullOrEmpty(path)) return;

            var newGuidString = new Guid(AssetDatabase.AssetPathToGUID(path)).ToString("N");
            if (_guidString != newGuidString)
            {
                _guidString = newGuidString;
                Debug.Log($"[{nameof(GuidComponent)}] Guid is updated. Asset path: {path}");
            }
        }
#endif
    }
}
