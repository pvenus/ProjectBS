using System;
using System.Collections.Generic;
using Character;
using UnityEngine;

namespace Battle.Morpg
{
    internal sealed class MorpgEnvironmentRuntime : IDisposable
    {
        internal static MorpgEnvironmentRuntime Active { get; private set; }
        internal MorpgEnvironmentProfile Profile { get; }
        internal MorpgEnvironmentProfile.Zone Zone => current < 0 ? null : Profile.zones[current];
        private readonly Dictionary<string, Sprite> sprites = new(StringComparer.Ordinal);
        private readonly List<SpriteRenderer> foreground = new();
        private readonly List<CharacterManager> actors = new();
        private readonly GameObject[] structural = new GameObject[3];
        private GameObject root;
        private int current=-1;
        private SpriteRenderer background;
        private float cameraSize;
        private Camera camera;
        private bool disposed;
        private MorpgEnvironmentRuntime(MorpgEnvironmentProfile profile) { Profile=profile; }

        internal static bool TryLoad(BattleMorpgZoneRewardDefinition definition, out MorpgEnvironmentRuntime runtime, out string error)
        {
            runtime=null;error=null;
            try
            {
                var source=Resources.Load<TextAsset>(MorpgEnvironmentGeometry.Resource);
                if(source==null){error="environment profile missing";return false;}
                var profile=JsonUtility.FromJson<MorpgEnvironmentProfile>(source.text);
                var errors=MorpgEnvironmentGeometry.Audit(profile,definition);
                if(errors.Count>0){error=string.Join("; ",errors);return false;}
                var candidate=new MorpgEnvironmentRuntime(profile);
                foreach(var asset in profile.assets)
                {
                    var sprite=Resources.Load<Sprite>(asset.resource);
                    if(sprite==null){error="environment sprite missing: "+asset.id;return false;}
                    candidate.sprites.Add(asset.id,sprite);
                }
                runtime=candidate;return true;
            }
            catch(Exception e){error="environment preflight: "+e.Message;return false;}
        }

        internal void Activate(Transform parent, Sprite backgroundSprite)
        {
            if(root!=null)throw new InvalidOperationException("environment already installed");
            if(backgroundSprite==null)throw new InvalidOperationException("environment full-map background missing");
            root=new GameObject("MORPG environment");root.transform.SetParent(parent,false);
            background=Child(root.transform,"Full map dressing — collider0").AddComponent<SpriteRenderer>();
            background.sprite=backgroundSprite;background.sortingOrder=-90;
            Vector2 size=background.sprite.bounds.size;
            background.transform.localScale=new Vector3((32f+Profile.overscan*2)/size.x,(18f+Profile.overscan*2)/size.y,1);
            var visuals=Child(root.transform,"Visuals — foliage collider0");
            var collision=Child(root.transform,"Structural colliders");
            for(int i=0;i<3;i++)
            {
                var z=Profile.zones[i];
                structural[i]=Child(collision.transform,z.zoneId);structural[i].SetActive(false);
                foreach(var p in z.props)
                {
                    Show(visuals.transform,p.id,p.asset,p.visualOrigin,p.visualSize,p.visualRotation);
                    var blocker=Child(structural[i].transform,p.colliderId);
                    blocker.transform.position=new Vector3(p.center[0],p.center[1],0);
                    if(p.radius>0)blocker.AddComponent<CircleCollider2D>().radius=p.radius;
                    else blocker.AddComponent<BoxCollider2D>().size=new Vector2(p.box[2]-p.box[0],p.box[3]-p.box[1]);
                }
                foreach(var p in z.decorations)Show(visuals.transform,p.id,p.asset,p.origin,p.size);
                // A closed strip outside each CCW edge, independent of the background and foliage.
                for(int j=0;j<z.walkable.Length;j++)
                {
                    var a=z.walkable[j];var b=z.walkable[(j+1)%z.walkable.Length];
                    var av=new Vector2(a[0],a[1]);var bv=new Vector2(b[0],b[1]);
                    var edge=(bv-av).normalized;var outward=new Vector2(edge.y,-edge.x);
                    var boundary=Child(structural[i].transform,z.boundaryId+".edge."+j);
                    var poly=boundary.AddComponent<PolygonCollider2D>();
                    poly.points=new[]{av-edge,bv+edge,bv+edge+outward,av-edge+outward};
                }
            }
            camera=Camera.main;
            if(camera!=null){cameraSize=camera.orthographicSize;camera.orthographicSize=3;}
            Active=this;
        }
        private static GameObject Child(Transform parent,string name)
        {var go=new GameObject(name);go.transform.SetParent(parent,false);return go;}
        private void Show(Transform parent,string id,string asset,float[] origin,float[] size,float rotation=0)
        {
            var go=Child(parent,id);go.transform.position=new Vector3(origin[0],origin[1],0);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprites[asset];
            var spec=Array.Find(Profile.assets,a=>a.id==asset);
            var rect=spec.opaqueRect;
            bool quarterTurn=Mathf.Abs(rotation)==90;
            float sx=(quarterTurn?size[1]:size[0])/rect[2];
            float sy=(quarterTurn?size[0]:size[1])/rect[3];
            go.transform.localScale=new Vector3(sx,sy,1);
            go.transform.rotation=Quaternion.Euler(0,0,rotation);
            Vector3 localCenter=new Vector3((rect[0]+rect[2]/2-5.12f)*sx,(rect[1]+rect[3]/2-.48f)*sy,0);
            Vector3 rotated=go.transform.rotation*localCenter;
            go.transform.position=new Vector3(origin[0],origin[1]+size[1]/2,0)-rotated;
            renderer.sortingOrder=-50;
            foreground.Add(renderer);
        }
        internal void RegisterActor(CharacterManager actor){if(actor!=null&&!actors.Contains(actor))actors.Add(actor);}
        internal bool TryBeginWarp(int ordinal,out string error)
        {
            error="environment landing blocked";
            if(ordinal<0 || ordinal>=3 || !MorpgEnvironmentGeometry.LandingValid(Profile.zones[ordinal]))return false;
            var z=Profile.zones[ordinal];
            foreach(var actor in actors)
                if(actor!=null && actor.RuntimeData!=null && !actor.RuntimeData.isDead &&
                   actor.GetComponent<MorpgOwnedObject>()!=null &&
                   MorpgEnvironmentGeometry.Distance(new[]{actor.transform.position.x,actor.transform.position.y},z.entry)<z.landingRadius)return false;
            structural[ordinal].SetActive(true); // Destination first; old boundary survives through placement.
            error=null;return true;
        }
        internal void CompleteWarp(int ordinal)
        {current=ordinal;for(int i=0;i<3;i++)if(i!=ordinal)structural[i].SetActive(false);}
        internal void Tick(float delta)
        {
            foreach(var renderer in foreground)
            {
                bool overlap=false;
                foreach(var actor in actors)
                {
                    if(actor==null || actor.RuntimeData==null || actor.RuntimeData.isDead)continue;
                    foreach(var ar in actor.GetComponentsInChildren<SpriteRenderer>())
                        if(ar.enabled && ar.bounds.Intersects(renderer.bounds)){overlap=true;break;}
                    if(overlap)break;
                }
                var color=renderer.color;
                color.a=Mathf.MoveTowards(color.a,overlap?.35f:1f,.65f*delta/(overlap?.12f:.18f));
                renderer.color=color;
            }
        }
        internal Vector2 Sweep(Vector2 from,Vector2 to,float radius)
        {
            if(Zone==null)return to;
            float t=MorpgEnvironmentGeometry.SweepFraction(Zone,from.x,from.y,to.x,to.y,radius);
            return Vector2.Lerp(from,to,t);
        }
        internal Vector2 ClampCamera(Vector2 p)
        {
            if(Zone==null)return p;var r=Zone.cameraClamp;
            return new Vector2(Mathf.Clamp(p.x,r[0],r[2]),Mathf.Clamp(p.y,r[1],r[3]));
        }
        internal static float ActorRadius(Rigidbody2D body)
        {
            float radius=0;
            if(body!=null)foreach(var collider in body.GetComponents<Collider2D>())
                if(collider.enabled&&!collider.isTrigger)radius=Mathf.Max(radius,Mathf.Max(collider.bounds.extents.x,collider.bounds.extents.y));
            return radius;
        }
        public void Dispose()
        {
            if(disposed)return;disposed=true;
            if(Active==this)Active=null;
            actors.Clear();foreground.Clear();current=-1;
            if(root!=null){root.SetActive(false);UnityEngine.Object.Destroy(root);}
            if(camera!=null)camera.orthographicSize=cameraSize;
        }
    }
}
