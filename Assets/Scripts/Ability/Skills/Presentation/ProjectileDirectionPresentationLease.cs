using UnityEngine;
namespace Skill
{
    // The live prefab is ProjectileEntity -> Scaler (collider) -> Renderer (Animator).
    // Insert below Scaler and above Animator; never reparent the physics hierarchy.
    public sealed class ProjectileDirectionPresentationLease
    {
        private Transform animated, originalParent, wrapper;
        private int sibling;
        private Vector3 position, scale, wrapperPosition, wrapperScale;
        private Quaternion rotation, wrapperRotation;
        private SpriteRenderer renderer;
        private bool flipX, flipY;
        private float angle;
        public bool Active {get;private set;}
        public bool Begin(Transform animatedRoot,Transform gameplayRoot,SpriteRenderer source,Vector2 direction,bool applyDirectionRotation,float rotationOffset)
        {
            Restore();
            if(animatedRoot==null||gameplayRoot==null||animatedRoot==gameplayRoot||
                !animatedRoot.IsChildOf(gameplayRoot)||animatedRoot.GetComponentInChildren<Collider2D>(true)!=null||
                animatedRoot.GetComponentInChildren<Rigidbody2D>(true)!=null)return false;
            animated=animatedRoot;originalParent=animated.parent;sibling=animated.GetSiblingIndex();
            position=animated.localPosition;rotation=animated.localRotation;scale=animated.localScale;
            renderer=source;if(renderer!=null){flipX=renderer.flipX;flipY=renderer.flipY;}
            wrapper=originalParent.Find("__ProjectileDirectionRotation");
            if(wrapper==null){wrapper=new GameObject("__ProjectileDirectionRotation").transform;wrapper.SetParent(originalParent,false);}
            wrapperPosition=wrapper.localPosition;wrapperRotation=wrapper.localRotation;wrapperScale=wrapper.localScale;
            wrapper.localPosition=Vector3.zero;wrapper.localScale=Vector3.one;
            angle=applyDirectionRotation&&SkillDirectionMath.Valid(direction)
                ?Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg+rotationOffset:0f;
            animated.SetParent(wrapper,false);
            Active=true;Apply();return true;
        }
        public void Apply()
        {
            if(Active&&wrapper!=null)wrapper.rotation=Quaternion.Euler(0f,0f,angle);
        }
        public void Restore()
        {
            if(!Active)return;
            if(animated!=null)
            {
                animated.SetParent(originalParent,false);animated.SetSiblingIndex(sibling);
                animated.localPosition=position;animated.localRotation=rotation;animated.localScale=scale;
            }
            if(renderer!=null){renderer.flipX=flipX;renderer.flipY=flipY;}
            if(wrapper!=null){wrapper.localPosition=wrapperPosition;wrapper.localRotation=wrapperRotation;wrapper.localScale=wrapperScale;}
            Active=false;animated=null;renderer=null;
        }
    }
}
