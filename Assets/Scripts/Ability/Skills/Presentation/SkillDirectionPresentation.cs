using System;
using UnityEngine;
namespace Skill
{
    [Serializable]
    public sealed class SkillDirectionalClip
    {
        [SerializeField,Range(0,7)] private int octant;
        [SerializeField] private AnimationClip clip;
        public int Octant=>octant;
        public AnimationClip Clip=>clip;
    }
    [Serializable]
    public sealed class SkillDirectionPresentationProfile
    {
        [SerializeField] private Vector2 canonicalForward=Vector2.right;
        [SerializeField] private bool allowBodyFlip=true;
        [SerializeField] private SkillDirectionalClip[] directionalClips=Array.Empty<SkillDirectionalClip>();
        public Vector2 CanonicalForward=>SkillDirectionMath.Valid(canonicalForward)?canonicalForward.normalized:Vector2.right;
        public AnimationClip ResolveBody(Vector2 direction,AnimationClip fallback,out float angle,out bool flip)
        {
            int octant=SkillDirectionMath.Octant(direction);
            foreach(var entry in directionalClips??Array.Empty<SkillDirectionalClip>())
                if(entry!=null&&entry.Octant==octant&&entry.Clip!=null){angle=0;flip=false;return entry.Clip;}
            int mirrored=(4-octant+8)%8;
            if(allowBodyFlip)foreach(var entry in directionalClips??Array.Empty<SkillDirectionalClip>())
                if(entry!=null&&entry.Octant==mirrored&&entry.Clip!=null){angle=0;flip=true;return entry.Clip;}
            // Upright body art: missing directional clips use horizontal facing only.
            angle=0f;flip=allowBodyFlip&&direction.x<0f;
            return fallback;
        }
    }
    public static class SkillDirectionMath
    {
        public static bool Valid(Vector2 d)=>!float.IsNaN(d.x)&&!float.IsInfinity(d.x)&&!float.IsNaN(d.y)&&!float.IsInfinity(d.y)&&d.sqrMagnitude>.0001f;
        public static int Octant(Vector2 direction)=>((int)Mathf.Round(Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg/45f)+8)%8;
        public static void Solve(Vector2 direction,Vector2 forward,bool allowFlip,out float angle,out bool flip)
        {
            if(!Valid(forward))forward=Vector2.right;
            flip=allowFlip&&direction.x*forward.x<0;
            if(flip)forward.x=-forward.x;
            angle=Mathf.DeltaAngle(Mathf.Atan2(forward.y,forward.x)*Mathf.Rad2Deg,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
        }
    }
    // A renderer-only child. Never rotate/reparent the source, Rigidbody or collider.
    public sealed class SkillDirectionPresentationLease
    {
        private SpriteRenderer source,proxy;
        private bool sourceEnabled,sourceFlipX,sourceFlipY,proxyEnabled,proxyFlipX,proxyFlipY;
        private Sprite proxySprite;
        private Vector3 position,scale;
        private Quaternion rotation;
        private float angle;
        private bool flip;
        private MaterialPropertyBlock block;
        public bool Active {get;private set;}
        public void Begin(SpriteRenderer renderer,float degrees,bool mirror)
        {
            Restore();if(renderer==null)return;
            source=renderer;sourceEnabled=source.enabled;sourceFlipX=source.flipX;sourceFlipY=source.flipY;
            var child=source.transform.Find("__SkillSnapshotDirection");
            if(child==null){child=new GameObject("__SkillSnapshotDirection").transform;child.SetParent(source.transform,false);proxy=child.gameObject.AddComponent<SpriteRenderer>();proxy.enabled=false;}
            else proxy=child.GetComponent<SpriteRenderer>()??child.gameObject.AddComponent<SpriteRenderer>();
            position=child.localPosition;rotation=child.localRotation;scale=child.localScale;
            proxyEnabled=proxy.enabled;proxyFlipX=proxy.flipX;proxyFlipY=proxy.flipY;proxySprite=proxy.sprite;
            child.localPosition=default;child.localScale=new Vector3(1f,1f,1f);
            angle=degrees;flip=mirror;Active=true;Apply();
        }
        public void Apply()
        {
            if(!Active||source==null||proxy==null)return;
            proxy.sprite=source.sprite;proxy.sharedMaterial=source.sharedMaterial;proxy.color=source.color;
            proxy.sortingLayerID=source.sortingLayerID;proxy.sortingOrder=source.sortingOrder;proxy.maskInteraction=source.maskInteraction;
            block??=new MaterialPropertyBlock();source.GetPropertyBlock(block);proxy.SetPropertyBlock(block);
            proxy.flipX=flip;proxy.flipY=sourceFlipY;proxy.transform.rotation=Quaternion.Euler(0,0,angle);
            proxy.enabled=sourceEnabled;source.enabled=false;
        }
        public void Restore()
        {
            if(!Active)return;
            if(source!=null){source.enabled=sourceEnabled;source.flipX=sourceFlipX;source.flipY=sourceFlipY;}
            if(proxy!=null){proxy.enabled=proxyEnabled;proxy.flipX=proxyFlipX;proxy.flipY=proxyFlipY;proxy.sprite=proxySprite;proxy.transform.localPosition=position;proxy.transform.localRotation=rotation;proxy.transform.localScale=scale;}
            Active=false;source=null;
        }
    }
}
