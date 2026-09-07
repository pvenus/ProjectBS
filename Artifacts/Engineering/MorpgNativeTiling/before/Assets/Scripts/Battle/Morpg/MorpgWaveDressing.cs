using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Morpg
{
    [Serializable] internal sealed class MorpgWaveDressing
    {
        internal const string Resource="battle/morpg/wave-environment-v1/binding";
        public string schemaVersion,backgroundManifestSha256,barrierManifestSha256;
        public Wave[] waves;
        public bool interiorPropsEnabled=true;
        [Serializable] internal sealed class Wave { public string zoneId;public Asset[] assets; }
        [Serializable] internal sealed class Asset
        {
            public string id,kind,resource,guid,sha256;
            public float[] pivot,rect,alphaRect;
            public int sortingOrder;
            public ColliderPatch[] colliderRects;
            [NonSerialized] internal Sprite Sprite;
        }
        [Serializable] internal sealed class ColliderPatch { public float[] rect; }
        internal static bool TryLoad(MorpgEnvironmentProfile environment,out MorpgWaveDressing profile,out string error)
        {
            profile=null;error=null;
            try
            {
                var source=Resources.Load<TextAsset>(Resource);
                if(source==null)throw new InvalidOperationException("wave dressing binding missing");
                if(!StrictCanonicalJson.TryParse(source.text,out var parsed,out error))return false;
                var root=Shape(parsed,"schemaVersion,backgroundManifestSha256,barrierManifestSha256,waves,interiorPropsEnabled");
                foreach(var w in (StrictCanonicalJson.ArrayNode)root["waves"])
                {
                    var wave=Shape(w,"zoneId,assets");
                    foreach(var a in (StrictCanonicalJson.ArrayNode)wave["assets"])
                    {
                        var asset=Shape(a,"id,kind,resource,guid,sha256,pivot,rect,sortingOrder,colliderRects,alphaRect");
                        foreach(var patch in (StrictCanonicalJson.ArrayNode)asset["colliderRects"])Shape(patch,"rect");
                    }
                }
                var candidate=JsonUtility.FromJson<MorpgWaveDressing>(source.text);
                if(candidate.schemaVersion!="morpg-wave-dressing.v1" || candidate.waves?.Length!=3 ||
                   candidate.backgroundManifestSha256?.Length!=64 || candidate.barrierManifestSha256?.Length!=64)
                    throw new InvalidOperationException("wave dressing header");
                var ids=new HashSet<string>();var resources=new HashSet<string>();var guids=new HashSet<string>();
                for(int i=0;i<3;i++)
                {
                    var z=environment.zones[i];var wave=candidate.waves[i];
                    if(wave.zoneId!=z.zoneId || wave.assets?.Length!=3)throw new InvalidOperationException("wave dressing slots");
                    for(int j=0;j<3;j++)
                    {
                        var a=wave.assets[j];string kind=new[]{"background","top","bottom"}[j];
                        if(a.kind!=kind || a.id!=$"wave{i+1}.{kind}" || !ids.Add(a.id) || string.IsNullOrEmpty(a.resource) || !resources.Add(a.resource) ||
                           a.guid?.Length!=32 || !guids.Add(a.guid) || a.sha256?.Length!=64 || !RectValid(a.rect) || !RectValid(a.alphaRect) || a.alphaRect[0]<0 || a.alphaRect[1]<0 || a.alphaRect[2]>1536 || a.alphaRect[3]>512 || a.pivot?.Length!=2 || a.pivot[0]!=.5f ||
                           a.pivot[1]!=.5f || a.sortingOrder!=new[]{-1000,-900,50}[j] || a.colliderRects==null)
                            throw new InvalidOperationException("wave dressing asset: "+a.id);
                        if(j==0)
                        {
                            float hw=environment.cameraOrthographicSize*16f/9f,hh=environment.cameraOrthographicSize;
                            if(a.colliderRects.Length!=0 || a.rect[0]>z.cameraClamp[0]-hw || a.rect[2]<z.cameraClamp[2]+hw ||
                               a.rect[1]>z.cameraClamp[1]-hh || a.rect[3]<z.cameraClamp[3]+hh)
                                throw new InvalidOperationException("background framing/collider");
                        }
                        else
                        {
                            if(a.colliderRects.Length==0)throw new InvalidOperationException("barrier silhouette missing");
                            float edge=z.walkable[0].y;
                            foreach(var p in z.walkable)edge=j==1?Math.Max(edge,p.y):Math.Min(edge,p.y);
                            float sy=(a.rect[3]-a.rect[1])/512f;
                            float visibleInner=a.rect[3]-a.alphaRect[j==1?3:1]*sy;
                            float sx=(a.rect[2]-a.rect[0])/1536f;
                            float left=z.walkable[0].x,right=left;
                            foreach(var point in z.walkable){left=Math.Min(left,point.x);right=Math.Max(right,point.x);}
                            if(Math.Abs(visibleInner-edge)>.001f || Math.Abs(a.rect[0]+a.alphaRect[0]*sx-left)>.001f || Math.Abs(a.rect[0]+a.alphaRect[2]*sx-right)>.001f)
                                throw new InvalidOperationException("barrier alpha edge alignment");
                            foreach(var patch in a.colliderRects)
                            {
                                var box=patch.rect;
                                if(!RectValid(box) || box[0]<a.rect[0] || box[2]>a.rect[2] || box[1]<a.rect[1] || box[3]>a.rect[3] ||
                                   (j==1?box[1]<edge+.05f:box[3]>edge-.05f))
                                    throw new InvalidOperationException("barrier collider invades combat/bounds");
                            }
                        }
                        a.Sprite=Resources.Load<Sprite>(a.resource);
                        if(a.Sprite==null)throw new InvalidOperationException("wave dressing sprite missing: "+a.id);
                        if(Math.Abs(a.Sprite.rect.width-1536)>.01f || Math.Abs(a.Sprite.rect.height-512)>.01f || Math.Abs(a.Sprite.pixelsPerUnit-100)>.001f ||
                           Math.Abs(a.Sprite.pivot.x-768f)>.01f || Math.Abs(a.Sprite.pivot.y-256f)>.01f)
                            throw new InvalidOperationException($"wave dressing imported canvas/pivot mismatch: {a.id}; actual rect={a.Sprite.rect.width}x{a.Sprite.rect.height}, PPU={a.Sprite.pixelsPerUnit}, pivot=({a.Sprite.pivot.x},{a.Sprite.pivot.y}); expected1536x512/100/(768,256). Refresh imports and restart Play.");
                    }
                }
                Debug.Log("[MORPG dressing] Imported canvas1536x512/PPU100/center pivot verified9/9");
                profile=candidate;return true;
            }
            catch(Exception e){error=e.Message;return false;}
        }
        private static bool RectValid(float[] r)
        {
            if(r?.Length!=4)return false;
            foreach(float v in r)if(float.IsNaN(v)||float.IsInfinity(v))return false;
            return r[0]<r[2]&&r[1]<r[3];
        }
        private static StrictCanonicalJson.ObjectNode Shape(object v,string fields)
        {
            if(!(v is StrictCanonicalJson.ObjectNode o))throw new InvalidOperationException("dressing object required");
            var allowed=new HashSet<string>(fields.Split(','));
            foreach(string key in o.Keys)if(!allowed.Remove(key))throw new InvalidOperationException("unknown dressing field "+key);
            if(allowed.Count!=0)throw new InvalidOperationException("missing dressing field");
            return o;
        }
    }
}
