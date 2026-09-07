using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Skill;
namespace UnityEngine {
 public class SerializeField:Attribute{} public class RangeAttribute:Attribute{public RangeAttribute(int a,int b){}}
 public struct Vector2{public float x,y;public Vector2(float a,float b){x=a;y=b;}public static Vector2 right=>new Vector2(1,0);public float sqrMagnitude=>x*x+y*y;public Vector2 normalized=>new Vector2(x/(float)Math.Sqrt(sqrMagnitude),y/(float)Math.Sqrt(sqrMagnitude));}
 public struct Vector3{public float x,y,z;public Vector3(float a,float b,float c){x=a;y=b;z=c;}}
 public struct Quaternion{public float angle;public static Quaternion Euler(float a,float b,float c)=>new Quaternion{angle=c};}
 public static class Mathf{public const float Rad2Deg=57.295779513f;public static float Round(float a)=>(float)Math.Round(a);public static float Atan2(float a,float b)=>(float)Math.Atan2(a,b);public static float DeltaAngle(float a,float b){float d=(b-a)%360;if(d>180)d-=360;if(d< -180)d+=360;return d;}}
 public class Component{public GameObject gameObject;public Transform transform=>gameObject.transform;public T GetComponent<T>() where T:Component=>gameObject.GetComponent<T>();}
 public class Transform:Component{public Transform parent;public string name;public Vector3 localPosition,localScale=new Vector3(1,1,1);public Quaternion localRotation;public List<Transform> children=new List<Transform>();public Quaternion rotation{get=>Quaternion.Euler(0,0,localRotation.angle+(parent?.rotation.angle??0));set=>localRotation=Quaternion.Euler(0,0,value.angle-(parent?.rotation.angle??0));}public void SetParent(Transform p,bool w){parent=p;p.children.Add(this);}public Transform Find(string n)=>children.Find(c=>c.name==n);}
 public class GameObject{public Transform transform;Dictionary<Type,Component> items=new Dictionary<Type,Component>();public GameObject(string n){transform=new Transform{gameObject=this,name=n};}public T AddComponent<T>() where T:Component,new(){var t=new T{gameObject=this};items[typeof(T)]=t;return t;}public T GetComponent<T>() where T:Component=>items.TryGetValue(typeof(T),out var v)?(T)v:null;}
 public class Sprite{} public class AnimationClip{} public class MaterialPropertyBlock{}
 public class SpriteRenderer:Component{public bool enabled=true,flipX,flipY;public Sprite sprite;public object sharedMaterial,color,maskInteraction;public int sortingLayerID,sortingOrder;public void GetPropertyBlock(MaterialPropertyBlock b){}public void SetPropertyBlock(MaterialPropertyBlock b){}}
}
class ActualFacingProbe {
 public enum DiagonalDirection {UpLeft,DownLeft,UpRight,DownRight}
 public object _facingLockOwner=new object();public SpriteRenderer targetSpriteRenderer,_comboPresentationRenderer;
 public bool _comboPresentationActive=true;public DiagonalDirection _facingLockDirection;
 public void Tick()=>ApplyFacingLockAfterAnimationSample();
 /* ACTUAL FACING */
}
class Tests{
 static int count;static void Check(bool b,string n){if(!b)throw new Exception(n);count++;}
 static bool Near(float a,float b)=>Math.Abs(Mathf.DeltaAngle(a,b))<.001;
 static void Set(object o,string n,object v)=>o.GetType().GetField(n,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(o,v);
 static void Main(){var profile=new SkillDirectionPresentationProfile();var fallback=new AnimationClip();
 for(int i=0;i<8;i++){float deg=i*45;var d=new Vector2((float)Math.Cos(deg/Mathf.Rad2Deg),(float)Math.Sin(deg/Mathf.Rad2Deg));var clip=profile.ResolveBody(d,fallback,out var a,out var f);Check(clip==fallback&&a==0&&f==(d.x<0),"body fallback octant "+i);SkillDirectionMath.Solve(d,Vector2.right,false,out a,out f);Check(!f&&Near(a,deg),"VFX octant "+i);Check(SkillDirectionMath.Octant(d)==i,"octant "+i);}
 var entries=new SkillDirectionalClip[8];for(int i=0;i<8;i++){entries[i]=new SkillDirectionalClip();Set(entries[i],"octant",i);Set(entries[i],"clip",new AnimationClip());}Set(profile,"directionalClips",entries);
 for(int i=0;i<8;i++){var d=new Vector2((float)Math.Cos(i*Math.PI/4),(float)Math.Sin(i*Math.PI/4));Check(profile.ResolveBody(d,fallback,out var a,out var f)==entries[i].Clip&&a==0&&!f,"native clip priority "+i);}
 Set(profile,"directionalClips",new[]{entries[0]});Check(profile.ResolveBody(new Vector2(-1,0),fallback,out var angle,out var flip)==entries[0].Clip&&angle==0&&flip,"mirrored clip priority");
 Set(profile,"canonicalForward",new Vector2(0,1));Set(profile,"directionalClips",Array.Empty<SkillDirectionalClip>());profile.ResolveBody(Vector2.right,fallback,out angle,out flip);Check(angle==0&&!flip,"body ignores canonical angle and stays upright");
 Check(!SkillDirectionMath.Valid(new Vector2(float.NaN,0))&&!SkillDirectionMath.Valid(new Vector2(0,0)),"invalid direction");
 foreach(bool nested in new[]{false,true}){var root=new GameObject("physics");root.transform.localRotation=Quaternion.Euler(0,0,23);var go=nested?new GameObject("renderer"):root;if(nested)go.transform.SetParent(root.transform,false);var source=go.AddComponent<SpriteRenderer>();source.flipX=true;source.flipY=true;var lease=new SkillDirectionPresentationLease();
 for(int cycle=0;cycle<8;cycle++){lease.Begin(source,cycle*45,cycle%2==0);var child=source.transform.Find("__SkillSnapshotDirection");var proxy=child.GetComponent<SpriteRenderer>();source.flipX=false;lease.Apply();Check(!source.enabled&&proxy.enabled&&proxy.flipX==(cycle%2==0)&&Near(child.rotation.angle,cycle*45),"sample overwrite and immutable angle");Check(Near(root.transform.rotation.angle,23),"physics rotation unchanged");lease.Restore();Check(source.enabled&&source.flipX&&source.flipY&&!proxy.enabled&&Near(child.localRotation.angle,0),"pool baseline restored");lease.Restore();Check(!lease.Active,"idempotent cancellation");}
 var old=source.transform.Find("__SkillSnapshotDirection");old.localPosition=new Vector3(2,3,4);old.localScale=new Vector3(3,2,1);old.localRotation=Quaternion.Euler(0,0,17);lease.Begin(source,90,false);lease.Restore();Check(old.localPosition.x==2&&old.localScale.x==3&&Near(old.localRotation.angle,17),"existing local transform exact restore");Check(source.enabled,"scripted transition original renderer restored");}
 var empty=new SkillDirectionPresentationLease();empty.Begin(null,90,true);empty.Apply();empty.Restore();Check(!empty.Active,"missing visual safe");
 for(int i=0;i<8;i++){
  var root=new GameObject("character");var body=root.AddComponent<SpriteRenderer>();var visual=new GameObject("VFX").AddComponent<SpriteRenderer>();
  var dir=new Vector2((float)Math.Cos(i*Math.PI/4),(float)Math.Sin(i*Math.PI/4));var p=new SkillDirectionPresentationProfile();p.ResolveBody(dir,fallback,out var a,out var f);
  var bodyLease=new SkillDirectionPresentationLease();bodyLease.Begin(body,a,f);var rendered=body.transform.Find("__SkillSnapshotDirection");
  Check(Near(root.transform.rotation.angle,0)&&Near(body.transform.rotation.angle,0)&&Near(rendered.rotation.angle,0),"body root renderer Z zero octant "+i);
  SkillDirectionMath.Solve(dir,Vector2.right,false,out a,out f);var vfxLease=new SkillDirectionPresentationLease();vfxLease.Begin(visual,a,f);
  Check(Near(visual.transform.Find("__SkillSnapshotDirection").rotation.angle,i*45)&&Near(visual.transform.rotation.angle,0),"only VFX child rotates octant "+i);
  bodyLease.Restore();vfxLease.Restore();
 }
 foreach(bool mirrored in new[]{false,true}){
  var body=new GameObject("body").AddComponent<SpriteRenderer>();var proxy=new GameObject("combo").AddComponent<SpriteRenderer>();var probe=new ActualFacingProbe{targetSpriteRenderer=body,_comboPresentationRenderer=proxy,_facingLockDirection=mirrored?ActualFacingProbe.DiagonalDirection.UpLeft:ActualFacingProbe.DiagonalDirection.DownRight};
  body.flipX=!mirrored;proxy.flipX=!mirrored;probe.Tick();Check(body.flipX==mirrored&&proxy.flipX==mirrored,"actual post-sample lock repairs original and noncanonical proxy flip");
  probe._facingLockOwner=null;body.flipX=!mirrored;proxy.flipX=!mirrored;probe.Tick();Check(body.flipX==!mirrored&&proxy.flipX==!mirrored,"released lock leaves scripted/NPC renderer alone");
 }
 Console.WriteLine("PASS "+count+" visual direction assertions");
 }
}
