using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Morpg
{
    [Serializable] internal sealed class MorpgWaveDressing
    {
        internal const string Resource="battle/morpg/wave-environment-v1/binding";
        public string schemaVersion,layeredManifestSha256,barrierManifestSha256;
        internal const string FloorShaderResource="battle/morpg/wave-environment-v3-layered/FloorBlend";
        [NonSerialized] internal Shader FloorShader;
        public Wave[] waves;
        public Layout layout;
        [Serializable] internal sealed class Layout
        {
            public float zoneWidth, zoneGap, overlap, parallaxFactor, cameraY, cameraHalfHeight, bottomInset, topParallaxFactor, bottomParallaxFactor, collisionBandInset, topVisualInnerY, floorHorizonY, floorBlendHeight;
            public int topCount,bottomCount;
            internal float Origin(int ordinal)=>ordinal*(zoneWidth+zoneGap);
        }
        public bool interiorPropsEnabled=true;
        [NonSerialized] private bool projected;
        [Serializable] internal sealed class Wave { public string zoneId;public Asset[] assets; }
        [Serializable] internal sealed class Asset
        {
            public string id,kind,resource,guid,sha256;
            public float[] pivot,rect,alphaRect;
            public float anchorRow,bandOffset;
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
                var root=Shape(parsed,"schemaVersion,layeredManifestSha256,barrierManifestSha256,waves,interiorPropsEnabled,layout");
                Shape(root["layout"],"zoneWidth,zoneGap,overlap,parallaxFactor,cameraY,cameraHalfHeight,bottomInset,topCount,bottomCount,topParallaxFactor,bottomParallaxFactor,collisionBandInset,topVisualInnerY,floorHorizonY,floorBlendHeight");
                foreach(var w in (StrictCanonicalJson.ArrayNode)root["waves"])
                {
                    var wave=Shape(w,"zoneId,assets");
                    foreach(var a in (StrictCanonicalJson.ArrayNode)wave["assets"])
                    {
                        var asset=Shape(a,"id,kind,resource,guid,sha256,pivot,rect,sortingOrder,colliderRects,alphaRect,anchorRow,bandOffset");
                        foreach(var patch in (StrictCanonicalJson.ArrayNode)asset["colliderRects"])Shape(patch,"rect");
                    }
                }
                var candidate=JsonUtility.FromJson<MorpgWaveDressing>(source.text);
                if(candidate.schemaVersion!="morpg-wave-dressing.v2" || candidate.waves?.Length!=3 ||
                   candidate.layeredManifestSha256!="f3ef1e10d277c7359d7cf611dfcd31e365387a803827dc3cc7b1cf2e1627a425" || candidate.barrierManifestSha256?.Length!=64)
                    throw new InvalidOperationException("wave dressing header");
                if(candidate.layout==null || candidate.layout.zoneWidth!=32 || candidate.layout.zoneGap!=20 ||
                   !(candidate.layout.overlap>=.10f&&candidate.layout.overlap<=.15f) || !(candidate.layout.parallaxFactor>=.65f&&candidate.layout.parallaxFactor<=.75f) || candidate.layout.cameraY!=-.5f || candidate.layout.cameraHalfHeight!=5 || candidate.layout.bottomInset!=.65f ||
                   !(candidate.layout.topVisualInnerY>=3.4f&&candidate.layout.topVisualInnerY<=3.65f) || candidate.layout.floorHorizonY!=candidate.layout.topVisualInnerY ||
                   !(candidate.layout.floorBlendHeight>=.3f&&candidate.layout.floorBlendHeight<=.8f) || candidate.layout.topCount!=2 || candidate.layout.bottomCount!=4 || candidate.layout.collisionBandInset!=.05f ||
                   !(candidate.layout.topParallaxFactor>=.35f&&candidate.layout.topParallaxFactor<=.50f) || !(candidate.layout.bottomParallaxFactor>=-.20f&&candidate.layout.bottomParallaxFactor<=-.10f))
                    throw new InvalidOperationException("wave dressing layout unsafe");
                var ids=new HashSet<string>();var resources=new HashSet<string>();var guids=new HashSet<string>();
                for(int i=0;i<3;i++)
                {
                    var z=environment.zones[i];var wave=candidate.waves[i];
                    float min=z.walkable[0].x,max=min;
                    foreach(var point in z.walkable){min=Math.Min(min,point.x);max=Math.Max(max,point.x);}
                    if(Math.Abs(max-min-candidate.layout.zoneWidth)>.001f)throw new InvalidOperationException("zone width/layout mismatch");
                    if(wave.zoneId!=z.zoneId || wave.assets?.Length!=4)throw new InvalidOperationException("wave dressing slots");
                    for(int j=0;j<4;j++)
                    {
                        var a=wave.assets[j];string kind=new[]{"far","floor","top","bottom"}[j];
                        if(a.kind!=kind || a.id!=$"wave{i+1}.{kind}" || !ids.Add(a.id) || string.IsNullOrEmpty(a.resource) || !resources.Add(a.resource) ||
                           a.guid?.Length!=32 || !guids.Add(a.guid) || a.sha256?.Length!=64 || !RectValid(a.rect) || !RectValid(a.alphaRect) || a.alphaRect[0]<0 || a.alphaRect[1]<0 || a.alphaRect[2]>1536 || a.alphaRect[3]>512 || a.pivot?.Length!=2 || a.pivot[0]!=.5f ||
                           a.pivot[1]!=.5f || a.sortingOrder!=new[]{-1100,-1000,-900,50}[j] || a.colliderRects==null)
                            throw new InvalidOperationException("wave dressing asset: "+a.id);
                        if(j<2)
                        {
                            if(a.resource!=$"battle/morpg/wave-environment-v3-layered/wave{i+1}-{kind}" || a.colliderRects.Length!=0 || a.rect[0]>-2 || a.rect[2]<candidate.layout.zoneWidth+2 ||
                               a.rect[1]>candidate.layout.cameraY-candidate.layout.cameraHalfHeight-.1f || a.rect[3]<candidate.layout.cameraY+candidate.layout.cameraHalfHeight+.1f)
                                throw new InvalidOperationException("background framing/collider");
                        }
                        else
                        {
                            if(a.colliderRects.Length==0 || Math.Abs(a.rect[2]-a.rect[0]-15.36f)>.001f || Math.Abs(a.rect[3]-a.rect[1]-5.12f)>.001f ||
                               !(a.anchorRow>=0&&a.anchorRow<=512))throw new InvalidOperationException("native barrier geometry");
                            foreach(var patch in a.colliderRects)
                            {
                                var box=patch.rect;
                                if(!RectValid(box) || box[0]<a.rect[0]-.001f || box[2]>a.rect[2]+.001f || box[1]<a.rect[1]-.001f || box[3]>a.rect[3]+.001f ||
                                   (j==2?box[1]<4+candidate.layout.collisionBandInset-.001f:box[3]>-4-candidate.layout.collisionBandInset+.001f))
                                    throw new InvalidOperationException("barrier collider invades combat core/bounds");
                            }
                        }
                        a.Sprite=Resources.Load<Sprite>(a.resource);
                        if(a.Sprite==null)throw new InvalidOperationException("wave dressing sprite missing: "+a.id);
                        if(Math.Abs(a.Sprite.rect.width-1536)>.01f || Math.Abs(a.Sprite.rect.height-512)>.01f || Math.Abs(a.Sprite.pixelsPerUnit-100)>.001f ||
                           Math.Abs(a.Sprite.pivot.x-768f)>.01f || Math.Abs(a.Sprite.pivot.y-256f)>.01f)
                            throw new InvalidOperationException($"wave dressing imported canvas/pivot mismatch: {a.id}; actual rect={a.Sprite.rect.width}x{a.Sprite.rect.height}, PPU={a.Sprite.pixelsPerUnit}, pivot=({a.Sprite.pivot.x},{a.Sprite.pivot.y}); expected1536x512/100/(768,256). Refresh imports and restart Play.");
                    }
                }
                candidate.FloorShader=Resources.Load<Shader>(FloorShaderResource);
                if(candidate.FloorShader==null || !candidate.FloorShader.isSupported)throw new InvalidOperationException("layered floor shader missing/unsupported");
                Debug.Log("[MORPG dressing] Imported canvas1536x512/PPU100/center pivot verified12/12; layered far/floor6/6");
                profile=candidate;return true;
            }
            catch(Exception e){error=e.Message;return false;}
        }
        // Attempt-local projection. Canonical contract/profile files and persistence remain unchanged.
        internal void Project(MorpgEnvironmentProfile p,BattleMorpgZoneRewardDefinition definition)
        {
            if(projected)throw new InvalidOperationException("dressing already projected");
            projected=true;
            p.cameraOrthographicSize=layout.cameraHalfHeight;
            float oldRight=p.zones[2].walkable[0].x;foreach(var v in p.zones[2].walkable)oldRight=Math.Max(oldRight,v.x);
            float rightMargin=p.mapBounds[2]-oldRight;
            for(int i=0;i<waves.Length;i++)
            {
                var z=p.zones[i];float authored=z.walkable[0].x;
                foreach(var v in z.walkable)authored=Math.Min(authored,v.x);
                float origin=layout.Origin(i),delta=origin-authored;
                foreach(var v in z.walkable)v.x+=delta;
                Shift(z.core,delta);Shift(z.cameraClamp,delta);Shift(z.entry,delta);Shift(z.transitionExit,delta);Shift(z.transitionStaging,delta);
                foreach(var prop in z.props){Shift(prop.center,delta);Shift(prop.box,delta);Shift(prop.visualOrigin,delta);}
                foreach(var decor in z.decorations)Shift(decor.origin,delta);
                foreach(var row in p.reservations)if(row.zoneId==z.zoneId)Shift(row.position,delta);
                var d=definition?.zones[i];
                if(d!=null){Shift(d.bounds,delta);Shift(d.spawnBounds,delta);Shift(d.entry,delta);foreach(var row in d.reservations)Shift(row.position,delta);}
                foreach(var a in waves[i].assets){Shift(a.rect,origin);foreach(var patch in a.colliderRects)Shift(patch.rect,origin);}
            }
            p.mapBounds[2]=layout.Origin(2)+layout.zoneWidth+rightMargin;
        }
        private static void Shift(float[] values,float x)
        {if(values==null || values.Length==0)return;values[0]+=x;if(values.Length==4)values[2]+=x;}
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
