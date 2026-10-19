// ==================== qcbf@qq.com | 2025-10-31 ====================

using System;
using FLib;
using UnityEngine;

namespace FLib.Unity.DynamicAtlases
{
    public abstract class DynamicAtlasComponent<T> : MonoBehaviour where T : Component
    {
        public bool AwakeConvertToDynamicAtlas;
        public T Target;
        public DynamicAtlas Atlas;
        [SerializeField] protected uint DynamicSpriteId;

        /// <summary>
        /// 
        /// </summary>
        protected virtual DynamicAtlasSprite MakeDynamicSprite(Texture2D uSprite)
        {
            try
            {
                var dSprite = Atlas.AddSprite(uSprite);
                DynamicSpriteId = dSprite.Id;
                return dSprite;
            }
            catch (Exception e)
            {
                throw new Exception(transform.GetTransformPath(), e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void Awake()
        {
            if (DynamicSpriteId > 0) // 主要是clone gameObject 已经转换为动态图集时, 需要正确处理之前转换的数据
            {
                if (!Atlas.DynamicSprites.TryGetValue(DynamicSpriteId, out var dSprite))
                    throw new Exception($"not found dynamic sprite: {transform.GetTransformPath()}");
                dSprite.Use();
            }
            else if (AwakeConvertToDynamicAtlas)
            {
                ConvertToDynamicAtlas();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnDestroy()
        {
            Release();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Release()
        {
            if (Atlas != null && DynamicSpriteId != 0)
                Atlas.RemoveSprite(DynamicSpriteId);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Refresh()
        {
            Release();
            ConvertToDynamicAtlas();
        }


        public abstract void ConvertToDynamicAtlas();

        /// <summary>
        /// 
        /// </summary>
        public virtual Sprite CreateUnitySprite(Sprite originalUnitySprite)
        {
            var dSprite = MakeDynamicSprite(originalUnitySprite.texture);
            var pivot = originalUnitySprite.pivot / new Vector2(dSprite.Size.x, dSprite.Size.y);
            var sprite = Sprite.Create(dSprite.Atlas.AtlasTexture, dSprite.Rect, pivot, originalUnitySprite.pixelsPerUnit, 0, SpriteMeshType.FullRect, originalUnitySprite.border);
#if UNITY_EDITOR
            sprite.name = "[D]" + originalUnitySprite.name;
#endif
            return sprite;
        }

        public static implicit operator T(DynamicAtlasComponent<T> comp) => comp.Target;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            Target ??= GetComponent<T>();
        }
#endif
    }
}