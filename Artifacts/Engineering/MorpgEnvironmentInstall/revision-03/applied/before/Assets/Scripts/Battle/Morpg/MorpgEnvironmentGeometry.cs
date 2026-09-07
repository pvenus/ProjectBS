using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Morpg
{
    // This profile is the only coordinate source for art, collision, spawn, movement and camera.
    [Serializable] internal sealed class MorpgEnvironmentProfile
    {
        public string schemaVersion, contractSha256, sourceManifestSha256;
        public float[] mapBounds;
        public float overscan, minCorridorWidth;
        public float cameraOrthographicSize = 3f;
        public Asset[] assets;
        public Zone[] zones;
        public Reservation[] reservations;
        [Serializable] internal sealed class Asset { public string id, resource, guid, sha256; public float[] opaqueRect; }
        [Serializable] internal sealed class Point { public float x,y; public float this[int i] => i==0?x:y; }
        [Serializable] internal sealed class Zone
        {
            public string zoneId, waveId, anchorId, boundaryId, propManifestId, colliderManifestId, backgroundBindingId, reservationAuditRef;
            public Point[] walkable;
            public float[] core, entry, egressAxis, cameraClamp;
            public float[] transitionExit, transitionStaging;
            public float spawnInset, actorInset, landingRadius, egressLength, egressRadius;
            public Prop[] props;
            public Decoration[] decorations;
        }
        [Serializable] internal sealed class Prop
        {
            public string id, colliderId, asset;
            public float[] center, box, visualOrigin, visualSize;
            public float radius, visualRotation;
            public bool flipX;
        }
        [Serializable] internal sealed class Decoration { public string id, asset; public float[] origin, size; public bool flipX; public float visualRotation; }
        [Serializable] internal sealed class Reservation { public string reservationId, zoneId, role; public float[] position; public bool spawnValid; }
    }

    internal static class MorpgEnvironmentGeometry
    {
        internal const string Resource = "battle/morpg/environment/environment.v1";
        internal static bool ValidateJson(string json, out string error)
        {
            error=null;
            try
            {
                if(!StrictCanonicalJson.TryParse(json,out var value,out error))return false;
                var root=Shape(value,"schemaVersion,contractSha256,sourceManifestSha256,mapBounds,overscan,minCorridorWidth,assets,zones,reservations","cameraOrthographicSize");
                foreach(var a in Array(root,"assets"))Shape(a,"id,sourcePath,path,resource,sha256,guid,opaqueRect");
                foreach(var row in Array(root,"reservations"))Shape(row,"reservationId,zoneId,role,original,position,spawnValid,relocated");
                foreach(var item in Array(root,"zones"))
                {
                    var z=Shape(item,"zoneId,waveId,anchorId,entry,walkable,core,spawnInset,actorInset,landingRadius,egressLength,egressRadius,egressAxis,cameraClamp,boundaryId,propManifestId,colliderManifestId,backgroundBindingId,reservationAuditRef,props,decorations","transitionExit,transitionStaging");
                    foreach(var point in Array(z,"walkable"))Shape(point,"x,y");
                    foreach(var prop in Array(z,"props"))Shape(prop,"id,colliderId,asset,center,radius,box,visualSize,visualOrigin,visualRotation","flipX");
                    foreach(var decoration in Array(z,"decorations"))Shape(decoration,"id,asset,origin,size","flipX,visualRotation");
                }
                return true;
            }
            catch(Exception e){error="environment shape: "+e.Message;return false;}
        }
        private static StrictCanonicalJson.ObjectNode Shape(object value,string keys,string optionalKeys=null)
        {
            if(!(value is StrictCanonicalJson.ObjectNode node))throw new InvalidOperationException("object required");
            var expected=keys.Split(',');
            var allowed=new HashSet<string>(expected,StringComparer.Ordinal);
            if(optionalKeys!=null)foreach(string key in optionalKeys.Split(','))allowed.Add(key);
            foreach(string key in node.Keys)if(!allowed.Contains(key))throw new InvalidOperationException("unknown field "+key);
            foreach(string key in expected)if(!node.ContainsKey(key)||node[key]==null)throw new InvalidOperationException("missing "+key);
            return node;
        }
        private static StrictCanonicalJson.ArrayNode Array(StrictCanonicalJson.ObjectNode node,string key)
        {
            if(!(node[key] is StrictCanonicalJson.ArrayNode array))throw new InvalidOperationException(key+" array required");
            return array;
        }

        internal static double Distance(float[] a, float[] b) => Math.Sqrt((a[0]-b[0])*(a[0]-b[0])+(a[1]-b[1])*(a[1]-b[1]));
        internal static bool Inside(MorpgEnvironmentProfile.Zone z, float x, float y, float inset)
        {
            for (int i=0; i<z.walkable.Length; i++)
            {
                var a=z.walkable[i]; var b=z.walkable[(i+1)%z.walkable.Length];
                double dx=b[0]-a[0], dy=b[1]-a[1], length=Math.Sqrt(dx*dx+dy*dy);
                if (length<=0 || (dx*(y-a[1])-dy*(x-a[0]))/length < inset-.00001) return false;
            }
            return true;
        }
        internal static double PropDistance(MorpgEnvironmentProfile.Prop p, float x, float y)
        {
            if (p.radius>0) return Distance(p.center,new[]{x,y})-p.radius;
            double dx=Math.Max(Math.Max(p.box[0]-x,0),x-p.box[2]);
            double dy=Math.Max(Math.Max(p.box[1]-y,0),y-p.box[3]);
            return Math.Sqrt(dx*dx+dy*dy);
        }
        internal static double StructuralGap(MorpgEnvironmentProfile.Prop a, MorpgEnvironmentProfile.Prop b)
        {
            if(a.radius>0 && b.radius>0)return Distance(a.center,b.center)-a.radius-b.radius;
            if(a.radius>0)return PropDistance(b,a.center[0],a.center[1])-a.radius;
            if(b.radius>0)return PropDistance(a,b.center[0],b.center[1])-b.radius;
            double dx=Math.Max(a.box[0]-b.box[2],b.box[0]-a.box[2]);
            double dy=Math.Max(a.box[1]-b.box[3],b.box[1]-a.box[3]);
            if(dx<0 && dy<0)return Math.Max(dx,dy);
            return Math.Sqrt(Math.Max(0,dx)*Math.Max(0,dx)+Math.Max(0,dy)*Math.Max(0,dy));
        }

        internal static bool Free(MorpgEnvironmentProfile.Zone z, float x, float y, float inset)
        {
            if (!Inside(z,x,y,inset)) return false;
            foreach(var p in z.props) if(PropDistance(p,x,y)<inset-.00001) return false;
            return true;
        }
        internal static bool SpawnValid(MorpgEnvironmentProfile.Zone z, float[] point)
        {
            if(!Inside(z,point[0],point[1],z.spawnInset) || Distance(point,z.entry)<3.25) return false;
            foreach(var p in z.props) if(PropDistance(p,point[0],point[1])<.55-.00001) return false;
            float cx=Math.Max(z.entry[0],Math.Min(z.entry[0]+z.egressLength,point[0]));
            return Distance(point,new[]{cx,z.entry[1]})>=z.egressRadius+.55-.00001;
        }
        internal static bool LandingValid(MorpgEnvironmentProfile.Zone z)
        {
            // Sample the full swept capsule, not only its anchor or endpoint.
            for(int i=0;i<=50;i++)
                if(!Free(z,z.entry[0]+z.egressLength*i/50f,z.entry[1],z.egressRadius+z.actorInset))return false;
            return Free(z,z.entry[0],z.entry[1],z.landingRadius);
        }
        internal static float SweepFraction(MorpgEnvironmentProfile.Zone z, float x, float y, float endX, float endY, float radius)
        {
            float inset=z.actorInset+Math.Max(0,radius);
            if(!Free(z,x,y,inset)) return 0f;
            double length=Distance(new[]{x,y},new[]{endX,endY});
            // Every interval is shorter than the minimum structural footprint (.24).
            int steps=Math.Max(1,(int)Math.Ceiling(length/.025));
            float last=0;
            for(int i=1;i<=steps;i++)
            {
                float t=(float)i/steps;
                if(!Free(z,x+(endX-x)*t,y+(endY-y)*t,inset))
                {
                    float hi=t;
                    for(int n=0;n<16;n++) {float mid=(last+hi)*.5f;if(Free(z,x+(endX-x)*mid,y+(endY-y)*mid,inset))last=mid;else hi=mid;}
                    return last;
                }
                last=t;
            }
            return 1f;
        }
        internal static List<string> Audit(MorpgEnvironmentProfile p, BattleMorpgZoneRewardDefinition d)
        {
            var errors=new List<string>();
            if(p==null || p.schemaVersion!="morpg-environment.v1" || p.zones?.Length!=3 || p.assets?.Length!=15 || p.reservations?.Length!=28)
            {errors.Add("environment schema/exact inventory missing");return errors;}
            if(p.mapBounds?.Length!=4 || p.mapBounds[0]>=p.mapBounds[2] || p.mapBounds[1]>=p.mapBounds[3] ||
               p.cameraOrthographicSize<=0 || float.IsNaN(p.cameraOrthographicSize) || float.IsInfinity(p.cameraOrthographicSize))
            {errors.Add("map/camera dimensions invalid");return errors;}
            var ids=new HashSet<string>(StringComparer.Ordinal);
            foreach(var a in p.assets)if(string.IsNullOrEmpty(a.resource)||a.guid?.Length!=32||!ids.Add(a.id))errors.Add("asset reference invalid");
            for(int i=0;i<3;i++)
            {
                var z=p.zones[i];var wave=d.zones[i];
                if(z.zoneId!=wave.id || z.waveId!=wave.waveId || z.anchorId!=wave.entryWarpAnchorId || z.walkable?.Length<3 ||
                   z.entry?.Length!=2 || z.core?.Length!=4 || z.cameraClamp?.Length!=4 || z.props?.Length!=new[]{8,4,5}[i] ||
                   string.IsNullOrEmpty(z.boundaryId)||string.IsNullOrEmpty(z.propManifestId)||string.IsNullOrEmpty(z.colliderManifestId)||
                   string.IsNullOrEmpty(z.backgroundBindingId)||string.IsNullOrEmpty(z.reservationAuditRef))
                {errors.Add("zone references invalid: "+i);continue;}
                if(!LandingValid(z))errors.Add("landing/+X corridor blocked: "+z.zoneId);
                foreach(var prop in z.props)
                {
                    if(!ids.Contains(prop.asset) || prop.colliderId!="collider."+prop.id)errors.Add("prop reference invalid: "+prop.id);
                    float cx=Math.Max(z.core[0],Math.Min(z.core[2],prop.center[0]));
                    float cy=Math.Max(z.core[1],Math.Min(z.core[3],prop.center[1]));
                    if(PropDistance(prop,cx,cy)<z.actorInset-.00001)errors.Add("core overlap: "+prop.id);
                }
                for(int a=0;a<z.props.Length;a++)for(int b=a+1;b<z.props.Length;b++)
                {
                    var left=z.props[a];var right=z.props[b];
                    if(StructuralGap(left,right)<-.00001)errors.Add("structural overlap: "+left.id+" / "+right.id);
                    if(left.radius>0||right.radius>0)continue;
                    if(left.box[1]==right.box[1] && left.box[3]==right.box[3])
                    {
                        float gap=Math.Max(left.box[0],right.box[0])-Math.Min(left.box[2],right.box[2]);
                        if(gap>0 && gap<p.minCorridorWidth)errors.Add("prop gap below minimum: "+left.id+" / "+right.id);
                    }
                }
                for(int j=0;j<wave.reservations.Length;j++)
                {
                    var r=wave.reservations[j];
                    if(!SpawnValid(z,r.position))errors.Add("unsafe spawn: "+r.reservationId);
                    int matches=0;
                    foreach(var row in p.reservations)if(row.reservationId==r.reservationId && row.zoneId==z.zoneId && row.role==r.unitKey && row.spawnValid && Distance(row.position,r.position)<.00001)matches++;
                    if(matches!=1)errors.Add("reservation audit mismatch: "+r.reservationId);
                    for(int k=0;k<j;k++)if(Distance(r.position,wave.reservations[k].position)<.65-.00001)errors.Add("reservation separation: "+r.reservationId);
                }
                // The authored camera and map envelope share one coordinate source.
                float hw=p.cameraOrthographicSize*16f/9f+p.overscan,hh=p.cameraOrthographicSize+p.overscan;
                if(z.cameraClamp[0]-hw<p.mapBounds[0]-p.overscan || z.cameraClamp[2]+hw>p.mapBounds[2]+p.overscan || z.cameraClamp[1]-hh<p.mapBounds[1]-p.overscan || z.cameraClamp[3]+hh>p.mapBounds[3]+p.overscan)errors.Add("camera void: "+z.zoneId);
            }
            return errors;
        }
    }
}
