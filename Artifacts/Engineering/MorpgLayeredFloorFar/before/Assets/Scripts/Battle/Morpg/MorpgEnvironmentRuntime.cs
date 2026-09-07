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
        internal Rect MapBounds => Rect.MinMaxRect(Profile.mapBounds[0],Profile.mapBounds[1],Profile.mapBounds[2],Profile.mapBounds[3]);
        internal MorpgEnvironmentProfile.Zone Zone => current < 0 ? null : runtimeZones[current];
        private readonly Dictionary<string, Sprite> sprites = new(StringComparer.Ordinal);
        private readonly List<SpriteRenderer> foreground = new();
        private readonly List<CharacterManager> actors = new();
        private readonly GameObject[] visualRoots=new GameObject[3];
        private readonly Transform[] backgrounds=new Transform[3];
        private readonly Vector3[] backgroundBase=new Vector3[3];
        private readonly List<Sprite> croppedSprites=new();
        private sealed class ParallaxVisual { internal Transform Transform;internal Vector3 Base;internal float Factor;internal int Zone; }
        private readonly List<ParallaxVisual> barrierVisuals=new();
        private Vector3 parallaxOrigin;
        private readonly GameObject[] structural = new GameObject[3];
        private GameObject root;
        private int current=-1;
        private MorpgEnvironmentProfile.Zone[] runtimeZones;
        internal MorpgEnvironmentProfile.Zone RuntimeZone(int ordinal)=>runtimeZones[ordinal];
        internal MorpgWaveDressing Dressing { get; private set; }
        private float cameraSize, cameraAspect;
        private bool cameraOrthographic;
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
                if(!MorpgEnvironmentGeometry.ValidateJson(source.text,out error))return false;
                var profile=JsonUtility.FromJson<MorpgEnvironmentProfile>(source.text);
                var errors=MorpgEnvironmentGeometry.Audit(profile,definition);
                if(errors.Count>0){error=string.Join("; ",errors);return false;}
                var candidate=new MorpgEnvironmentRuntime(profile);
                foreach(var asset in profile.assets)
                {
                    var sprite=Resources.Load<Sprite>(asset.resource);
                    if(sprite==null){error="environment sprite missing: "+asset.id;return false;}
                    if(asset.opaqueRect?.Length!=4 || asset.opaqueRect[2]<=0 || asset.opaqueRect[3]<=0)
                    {error="environment opaque geometry missing: "+asset.id;return false;}
                    candidate.sprites.Add(asset.id,sprite);
                }
                if(!MorpgWaveDressing.TryLoad(profile,out var dressing,out error))return false;
                dressing.Project(profile,definition);
                errors=MorpgEnvironmentGeometry.Audit(profile,definition);
                if(errors.Count>0){error=string.Join("; ",errors);return false;}
                candidate.Dressing=dressing;candidate.runtimeZones=Array.ConvertAll(profile.zones,z=>dressing.interiorPropsEnabled?z:z.WithoutInteriorProps());runtime=candidate;return true;
            }
            catch(Exception e){error="environment preflight: "+e.Message;return false;}
        }

        internal void Activate(Transform parent, Sprite backgroundSprite)
        {
            if(root!=null || Active!=null)throw new InvalidOperationException("environment already installed");
            root=new GameObject("MORPG environment");// Keep independent from any ancestor SortingGroup; ownership is this disposable runtime.
            root.transform.SetParent(null,true);
            var visuals=Child(root.transform,"Visuals — foliage collider0");
            var collision=Child(root.transform,"Structural colliders");
            for(int i=0;i<3;i++)
            {
                var z=Profile.zones[i];
                visualRoots[i]=Child(visuals.transform,"wave.visual."+i);visualRoots[i].SetActive(false);
                structural[i]=Child(collision.transform,z.zoneId);structural[i].SetActive(false);
                foreach(var a in Dressing.waves[i].assets)
                    if(a.kind=="background")backgrounds[i]=ShowBackground(visualRoots[i].transform,a);
                    else ShowTiles(visualRoots[i].transform,structural[i].transform,a,i);
                backgroundBase[i]=backgrounds[i].position;
                if(Dressing.interiorPropsEnabled)foreach(var p in z.props)
                {
                    Show(visualRoots[i].transform,p.id,p.asset,p.visualOrigin,p.visualSize,p.visualRotation,p.flipX);
                    var blocker=Child(structural[i].transform,p.colliderId);
                    blocker.transform.position=new Vector3(p.center[0],p.center[1],0);
                    if(p.radius>0)blocker.AddComponent<CircleCollider2D>().radius=p.radius;
                    else blocker.AddComponent<BoxCollider2D>().size=new Vector2(p.box[2]-p.box[0],p.box[3]-p.box[1]);
                }
                if(Dressing.interiorPropsEnabled)foreach(var p in z.decorations)Show(visualRoots[i].transform,p.id,p.asset,p.origin,p.size,p.visualRotation,p.flipX);
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
            if(camera!=null){cameraSize=camera.orthographicSize;cameraAspect=camera.aspect;cameraOrthographic=camera.orthographic;camera.orthographic=true;camera.orthographicSize=Profile.cameraOrthographicSize;camera.aspect=16f/9f;}
            current=0;structural[0].SetActive(true);visualRoots[0].SetActive(true);
            root.AddComponent<MorpgEnvironmentLateView>().Owner=this;
            ResetParallax(0);
            Active=this;
        }
        private static GameObject Child(Transform parent,string name)
        {var go=new GameObject(name);go.transform.SetParent(parent,false);return go;}
        private static Transform ShowBackground(Transform parent,MorpgWaveDressing.Asset asset)
        {
            var go=Child(parent,asset.id);var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=asset.Sprite;
            float width=asset.rect[2]-asset.rect[0],height=asset.rect[3]-asset.rect[1];
            go.transform.position=new Vector3(asset.rect[0]+width*.5f,asset.rect[1]+height*.5f,0);
            go.transform.localScale=new Vector3(width/15.36f,height/5.12f,1);
            renderer.sortingLayerID=BattlePresentationSortingPolicy.MorpgSortingLayerId;renderer.sortingOrder=asset.sortingOrder;
            return go.transform;
        }
        internal List<float> TileStarts(int ordinal,string kind)
        {
            float width=15.36f;var layout=Dressing.layout;
            int count=kind=="top"?layout.topCount:layout.bottomCount;
            float step=kind=="top"?width:width*(1-layout.overlap);
            float first=(layout.zoneWidth-(width+(count-1)*step))*.5f;
            var starts=new List<float>();
            // IDs .tile.0..N are stable left-to-right; all zones use the same authored local layout.
            for(int i=0;i<count;i++)starts.Add(layout.Origin(ordinal)+first+i*step);
            return starts;
        }
        private void ShowTiles(Transform parent,Transform collisionParent,MorpgWaveDressing.Asset asset,int ordinal)
        {
            var group=Child(parent,asset.id);float origin=Dressing.layout.Origin(ordinal),end=origin+Dressing.layout.zoneWidth;
            int index=0;
            foreach(float start in TileStarts(ordinal,asset.kind))
            {
                float left=Math.Max(origin,start),right=Math.Min(end,start+15.36f);
                var go=Child(group.transform,asset.id+".tile."+index++);
                var renderer=go.AddComponent<SpriteRenderer>();
                // Keep native width and overscan. Only inward alpha fringe is clipped to the non-combat band.
                float bottom=asset.rect[1],top=asset.rect[3],band=Dressing.layout.collisionBandInset;
                if(asset.kind=="top")bottom=Math.Max(bottom,4+band);else top=Math.Min(top,-4-band);
                float cropY=(bottom-asset.rect[1])*100,height=(top-bottom)*100;
                height=Math.Min(height,asset.Sprite.rect.height-cropY);
                renderer.sprite=Sprite.Create(asset.Sprite.texture,new Rect(asset.Sprite.rect.x,asset.Sprite.rect.y+cropY,1536,height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
                croppedSprites.Add(renderer.sprite);
                go.transform.position=new Vector3(start+15.36f*.5f,(bottom+top)*.5f,0);
                go.transform.localScale=Vector3.one;
                renderer.sortingLayerID=BattlePresentationSortingPolicy.MorpgSortingLayerId;renderer.sortingOrder=asset.sortingOrder;
                barrierVisuals.Add(new ParallaxVisual{Transform=go.transform,Base=go.transform.position,Zone=ordinal,Factor=asset.kind=="top"?Dressing.layout.topParallaxFactor:Dressing.layout.bottomParallaxFactor});
                var paths=new List<Vector2[]>();
                foreach(var patch in asset.colliderRects)
                {
                    var r=patch.rect;float l=Math.Max(left,r[0]+start-origin),rr=Math.Min(right,r[2]+start-origin);
                    if(rr<=l)continue;
                    float cx=go.transform.position.x,cy=go.transform.position.y;
                    paths.Add(new[]{new Vector2(l-cx,r[1]-cy),new Vector2(rr-cx,r[1]-cy),new Vector2(rr-cx,r[3]-cy),new Vector2(l-cx,r[3]-cy)});
                }
                var blocker=Child(collisionParent,"silhouette."+go.name);blocker.transform.position=go.transform.position;
                var collider=blocker.AddComponent<PolygonCollider2D>();collider.pathCount=paths.Count;
                for(int j=0;j<paths.Count;j++)collider.SetPath(j,paths[j]);
            }
        }
        private void ResetParallax(int ordinal)
        {
            // A canonical per-zone camera origin prevents accumulated drift across cut/activation.
            parallaxOrigin=new Vector3(Dressing.layout.Origin(ordinal)+Dressing.layout.zoneWidth*.5f,Dressing.layout.cameraY,0);
            ApplyParallax();
        }
        internal void ApplyParallax()
        {
            if(current<0 || camera==null || backgrounds[current]==null)return;
            var limits=CameraLimits(current);
            float dx=Mathf.Clamp(camera.transform.position.x,limits.xMin,limits.xMax)-parallaxOrigin.x;
            // World-follow coefficients. Never move physics, props, actors, or the owning wave root.
            backgrounds[current].position=backgroundBase[current]+new Vector3(dx*Dressing.layout.parallaxFactor,0,0);
            foreach(var visual in barrierVisuals)if(visual.Zone==current)
                visual.Transform.position=visual.Base+new Vector3(dx*visual.Factor,0,0);

        }
        private void Show(Transform parent,string id,string asset,float[] origin,float[] size,float rotation=0,bool flipX=false)
        {
            var go=Child(parent,id);go.transform.position=new Vector3(origin[0],origin[1],0);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprites[asset];renderer.flipX=flipX;
            var spec=Array.Find(Profile.assets,a=>a.id==asset);
            var rect=spec.opaqueRect;
            bool quarterTurn=Mathf.Abs(rotation)==90;
            float sx=(quarterTurn?size[1]:size[0])/rect[2];
            float sy=(quarterTurn?size[0]:size[1])/rect[3];
            go.transform.localScale=new Vector3(sx,sy,1);
            go.transform.rotation=Quaternion.Euler(0,0,rotation);
            Vector3 localCenter=new Vector3((rect[0]+rect[2]/2-5.12f)*sx,(rect[1]+rect[3]/2-.48f)*sy,0);
            if(flipX)localCenter.x=-localCenter.x;
            Vector3 rotated=go.transform.rotation*localCenter;
            go.transform.position=new Vector3(origin[0],origin[1]+size[1]/2,0)-rotated;
            renderer.sortingLayerID=BattlePresentationSortingPolicy.MorpgSortingLayerId;
            renderer.sortingOrder=BattlePresentationSortingPolicy.MorpgBackProps;
            foreground.Add(renderer);
        }
        internal bool OwnsTransitionBlocker(Collider2D collider)
        {
            if(collider==null || !collider.enabled || collider.isTrigger)return false;
            foreach(var zone in structural)
                if(zone!=null && zone.activeInHierarchy && collider.transform.IsChildOf(zone.transform))return true;
            return false;
        }
        internal static bool OwnsActor(CharacterManager actor) => Active != null && actor != null && Active.actors.Contains(actor);
        internal void RegisterActor(CharacterManager actor)
        {
            if(actor==null || actors.Contains(actor))return;
            actors.Add(actor);
            foreach(var sorter in actor.GetComponentsInChildren<Util.SortingOrderMono>(true))sorter.UpdateSortingOrder();
        }
        internal bool TryBeginWarp(int ordinal,out string error,bool invisibleCut=false)
        {
            error="environment landing blocked";
            if(ordinal<0 || ordinal>=3 || !MorpgEnvironmentGeometry.LandingValid(runtimeZones[ordinal]))return false;
            var z=runtimeZones[ordinal];
            foreach(var actor in actors)
                if(actor!=null && actor.RuntimeData!=null && !actor.RuntimeData.isDead &&
                   actor.GetComponent<MorpgOwnedObject>()!=null &&
                   MorpgEnvironmentGeometry.Distance(new[]{actor.transform.position.x,actor.transform.position.y},z.entry)<z.landingRadius)return false;
            if(invisibleCut)
            {
                for(int i=0;i<3;i++){structural[i].SetActive(false);visualRoots[i].SetActive(false);}
                current=ordinal; // Same simulation tick: old off, placement/camera, new on.
            }
            // Destination visuals/colliders are enabled only by CompleteWarp after camera placement.
            error=null;return true;
        }
        internal void CompleteWarp(int ordinal)
        {
            current=ordinal;ResetParallax(ordinal);
            for(int i=0;i<3;i++){bool active=i==ordinal;structural[i].SetActive(active);visualRoots[i].SetActive(active);}
        }
        internal void Tick(float delta)
        {
            foreach(var renderer in foreground)
            {
                if(!renderer.gameObject.activeInHierarchy)continue;
                bool overlap=false;
                foreach(var actor in actors)
                {
                    if(actor==null || actor.RuntimeData==null || actor.RuntimeData.isDead)continue;
                    foreach(var ar in actor.GetComponentsInChildren<SpriteRenderer>())
                        if(ar.enabled && ar.bounds.Intersects(renderer.bounds)){overlap=true;break;}
                    if(overlap)break;
                }
                // Only the environment-owned prop is promoted/faded. Bodies and their MPBs are never written.
                renderer.sortingOrder=overlap ? BattlePresentationSortingPolicy.MorpgForegroundProps
                    : BattlePresentationSortingPolicy.MorpgBackProps;
                var color=renderer.color;
                color.a=Mathf.MoveTowards(color.a,overlap?.35f:1f,.65f*delta/(overlap?.12f:.18f));
                renderer.color=color;
            }
        }
        internal Vector2 Sweep(Vector2 from,Vector2 to,float radius)
        {
            if(Zone==null)return to;
            to=ClampActorCenter(to,radius);
            float t=MorpgEnvironmentGeometry.SweepFraction(Zone,from.x,from.y,to.x,to.y,radius);
            return Vector2.Lerp(from,to,t);
        }
        internal Rect CameraLimits(int ordinal)
        {
            var z=runtimeZones[ordinal];var bg=Dressing.waves[ordinal].assets[0].rect;
            float hh=camera!=null?camera.orthographicSize:Profile.cameraOrthographicSize;
            float hw=hh*(camera!=null?camera.aspect:16f/9f);
            float left=z.walkable[0].x,right=left,bottom=z.walkable[0].y,top=bottom;
            foreach(var point in z.walkable){left=Math.Min(left,point.x);right=Math.Max(right,point.x);bottom=Math.Min(bottom,point.y);top=Math.Max(top,point.y);}
            float minX=Math.Max(left+hw,bg[0]+hw+.1f),maxX=Math.Min(right-hw,bg[2]-hw-.1f);
            float minY=Dressing.layout.cameraY,maxY=minY;
            if(minX>maxX)minX=maxX=(left+right)/2;
            if(minY>maxY)minY=maxY=(bottom+top)/2;
            return Rect.MinMaxRect(minX,minY,maxX,maxY);
        }
        internal Vector2 FrameAim(Vector2 p)=>new Vector2(p.x,p.y+Dressing.layout.cameraY);
        internal Vector2 CameraTarget(int ordinal,Vector2 p) => ClampCameraAt(ordinal,FrameAim(p));
        private Vector2 ClampCameraAt(int ordinal,Vector2 p)
        {var r=CameraLimits(ordinal);return new Vector2(Mathf.Clamp(p.x,r.xMin,r.xMax),Mathf.Clamp(p.y,r.yMin,r.yMax));}
        internal Vector2 ClampCamera(Vector2 p)=>Zone==null?p:ClampCameraAt(current,p);
        internal Vector2 ClampActorCenter(Vector2 p,float radius=0)
        {
            if(Zone==null)return p;
            var cameraRange=CameraLimits(current);float hh=camera!=null?camera.orthographicSize:Profile.cameraOrthographicSize;
            float hw=hh*(camera!=null?camera.aspect:16f/9f),left=Zone.walkable[0].x,right=left,bottom=Zone.walkable[0].y,top=bottom;
            foreach(var v in Zone.walkable){left=Math.Min(left,v.x);right=Math.Max(right,v.x);bottom=Math.Min(bottom,v.y);top=Math.Max(top,v.y);}
            float inset=Math.Max(0,radius)+Zone.actorInset;
            // Camera-center limits are expanded by viewport half-width before applying actor clearance.
            left=Math.Max(left,cameraRange.xMin-hw);right=Math.Min(right,cameraRange.xMax+hw);
            return new Vector2(Mathf.Clamp(p.x,left+inset,right-inset),Mathf.Clamp(p.y,bottom+inset,top-inset));
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
            foreach(var actor in actors)
                if(actor!=null)foreach(var sorter in actor.GetComponentsInChildren<Util.SortingOrderMono>(true))sorter.UpdateSortingOrder();
            actors.Clear();foreground.Clear();barrierVisuals.Clear();current=-1;
            foreach(var sprite in croppedSprites)UnityEngine.Object.Destroy(sprite);croppedSprites.Clear();
            if(root!=null){root.SetActive(false);UnityEngine.Object.Destroy(root);}
            if(camera!=null){camera.orthographicSize=cameraSize;camera.aspect=cameraAspect;camera.orthographic=cameraOrthographic;}
        }
    }
    [DefaultExecutionOrder(32700)]
    internal sealed class MorpgEnvironmentLateView:MonoBehaviour
    {
        internal MorpgEnvironmentRuntime Owner;
        private void LateUpdate()=>Owner?.ApplyParallax();
    }
}
