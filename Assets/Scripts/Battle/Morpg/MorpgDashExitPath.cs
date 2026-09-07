using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Morpg
{
    // Revision03 has a clear center lane. Project with swept movement, never an exposed teleport.
    internal sealed class MorpgDashExitPath
    {
        private readonly List<Vector3> points;
        internal float Length { get; }
        internal Vector3 End => points[points.Count-1];
        private MorpgDashExitPath(List<Vector3> points,float length){this.points=points;Length=length;}
        internal Vector3 Sample(float progress)
        {
            float remaining=Mathf.Clamp01(progress)*Length;
            for(int i=1;i<points.Count;i++)
            {
                float length=Vector2.Distance(points[i-1],points[i]);
                if(remaining<=length)return Vector3.Lerp(points[i-1],points[i],length>0?remaining/length:1);
                remaining-=length;
            }
            return End;
        }
        internal static bool SegmentFree(MorpgEnvironmentProfile.Zone zone,Vector3 a,Vector3 b,float inset)
        {
            int n=Math.Max(1,(int)Math.Ceiling(Vector2.Distance(a,b)/.025f));
            for(int i=0;i<=n;i++)
            {var p=Vector3.Lerp(a,b,(float)i/n);if(!MorpgEnvironmentGeometry.Free(zone,p.x,p.y,inset))return false;}
            return true;
        }
        internal static MorpgDashExitPath Plan(MorpgEnvironmentProfile.Zone zone,Vector3 source,float actorRadius)
        {
            if(zone.transitionExit?.Length!=2)return null;
            float inset=actorRadius+zone.actorInset;
            var lane=new Vector3(Mathf.Clamp(source.x,zone.entry[0],zone.transitionExit[0]),zone.entry[1],source.z);
            // Keep the fade in the captured combat frame; do not dash across the entire32wu zone.
            var end=new Vector3(Mathf.Clamp(source.x+2.5f,zone.entry[0],zone.transitionExit[0]),zone.entry[1],source.z);
            var shoulder=new Vector3(source.x,Mathf.Clamp(source.y,-2f+zone.entry[1],2f+zone.entry[1]),source.z);
            var candidates=new[]{new[]{source,end},new[]{source,lane,end},new[]{source,shoulder,lane,end}};
            MorpgDashExitPath best=null;
            foreach(var route in candidates)
            {
                var points=new List<Vector3>{source};float length=0;bool valid=true;
                for(int i=1;i<route.Length;i++)
                {
                    if(!SegmentFree(zone,route[i-1],route[i],inset)){valid=false;break;}
                    float step=Vector2.Distance(route[i-1],route[i]);
                    if(step>.00001f){points.Add(route[i]);length+=step;}
                }
                if(valid && (best==null||length<best.Length)){if(points.Count==1)points.Add(source);best=new MorpgDashExitPath(points,length);}
            }
            return best;
        }
    }
}
