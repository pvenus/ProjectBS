using System;
using System.IO;
using System.Linq;
using Battle.Morpg;
using UnityEngine;
class GeometryTests
{
 static int count;
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 static void Test(string n,Action a){a();Console.WriteLine("PASS "+n);count++;}
 static void Main(string[] args)
 {
  string root=args[0];
  var p=JsonUtility.FromJson<MorpgEnvironmentProfile>(File.ReadAllText(root+"/Assets/Resources/battle/morpg/environment/environment.v1.json"));
  var d=JsonUtility.FromJson<BattleMorpgZoneRewardDefinition>(File.ReadAllText(root+"/Assets/Resources/battle/act1/chapter01/battle.act1.chapter01.01.rescue_villagers.morpg-zone-reward.v1.json"));
  Test("exact-15-assets-17-blockers-3-zones",()=>Check(p.assets.Length==15 && p.zones.Select(z=>z.props.Length).SequenceEqual(new[]{8,4,5}),"inventory"));
  Test("all-28-reservations-safe-count-role-id-preserved",()=>{
   Check(BattleMorpgDefinitionValidator.TryValidate(d,out var e),e);
   foreach(var z in d.zones)foreach(var r in z.reservations)Check(MorpgEnvironmentGeometry.SpawnValid(p.zones[z.index],r.position),r.reservationId);
  });
  Test("horizontal-geometry-passes-full-profile-audit",()=>{
   var errors=MorpgEnvironmentGeometry.Audit(p,d);
   File.WriteAllLines(root+"/Artifacts/Engineering/MorpgEnvironmentInstall/revision-03/applied/geometry-errors.txt",errors);
   Check(errors.Count==0,string.Join("; ",errors));
  });
  Test("center-obstacle-and-narrow-box-gap-rejected",()=>{
   var z=p.zones[2];var wall=z.props.Single(q=>q.id.EndsWith("wall.right"));var original=wall.box;var center=wall.center;
   wall.box=new[]{z.entry[0],-1f,z.entry[0]+.2f,1f};wall.center=z.entry;
   Check(MorpgEnvironmentGeometry.Audit(p,d).Any(e=>e.Contains("landing/+X")),"corridor conflict hidden");wall.box=original;wall.center=center;
   var left=z.props.Single(q=>q.id.EndsWith("house.top.left"));var right=z.props.Single(q=>q.id.EndsWith("house.top.right"));var box=right.box;
   right.box=new[]{left.box[2]+1,left.box[1],left.box[2]+3,left.box[3]};
   Check(MorpgEnvironmentGeometry.Audit(p,d).Any(e=>e.Contains("gap below")),"narrow gap accepted");right.box=box;
  });
  Test("spawn-validator-detects-tree-and-house-overlap",()=>{
   Check(!MorpgEnvironmentGeometry.SpawnValid(p.zones[0],p.zones[0].props[0].center),"tree spawn accepted");
   Check(!MorpgEnvironmentGeometry.SpawnValid(p.zones[2],p.zones[2].props[0].center),"house spawn accepted");
  });
  Test("sweep-stops-before-wall-no-tunneling",()=>{
   var z=p.zones[2];float x=z.entry[0];float target=110;
   float t=MorpgEnvironmentGeometry.SweepFraction(z,x,0,target,0,.3f);float end=x+(target-x)*t;
   Check(Math.Abs(end-(z.props.Single(q=>q.id.EndsWith("wall.right")).box[0]-.45f))<.002f,"wall sweep endpoint "+end);
  });
  Test("sweep-respects-boundary-and-actor-radius",()=>{
   var z=p.zones[0];float x=z.entry[0];float t=MorpgEnvironmentGeometry.SweepFraction(z,x,0,x,8,.3f);
   Check(t<1 && MorpgEnvironmentGeometry.Free(z,x,8*t,.45f),"boundary escaped");
  });
  Test("landing-and-camera-derived-from-same-profile",()=>{
   foreach(var z in p.zones){Check(MorpgEnvironmentGeometry.LandingValid(z),"landing invalid");
    float hw=p.cameraOrthographicSize*16/9;Check(z.cameraClamp[0]-hw>=p.mapBounds[0]&&z.cameraClamp[2]+hw<=p.mapBounds[2]&&z.cameraClamp[1]-p.cameraOrthographicSize>=p.mapBounds[1]&&z.cameraClamp[3]+p.cameraOrthographicSize<=p.mapBounds[3],"camera void");}
  });
  Test("installed-exit-staging-entry-capsules-pass",()=>{
   foreach(var z in p.zones)Check(MorpgEnvironmentGeometry.DashCorridorsValid(z),"dash capsule blocked "+z.zoneId);
   var prior=p.zones[1].transitionStaging;p.zones[1].transitionStaging=new[]{36.1f,0};
   Check(!MorpgEnvironmentGeometry.DashCorridorsValid(p.zones[1])&&MorpgEnvironmentGeometry.Audit(p,d).Any(e=>e.Contains("authored dash corridor")),"unsafe staging accepted");p.zones[1].transitionStaging=prior;
  });
  Test("horizontal-aspect-progression-and-original-identities",()=>{
   var old=JsonUtility.FromJson<BattleMorpgZoneRewardDefinition>(File.ReadAllText(root+"/Artifacts/Engineering/MorpgEnvironmentInstall/revision-03/applied/before/Assets/Resources/battle/act1/chapter01/battle.act1.chapter01.01.rescue_villagers.morpg-zone-reward.v1.json"));
   for(int i=0;i<3;i++){
    var z=p.zones[i];Check(Math.Abs((z.walkable.Max(v=>v.x)-z.walkable.Min(v=>v.x))/(z.walkable.Max(v=>v.y)-z.walkable.Min(v=>v.y))-4)<.001,"aspect");
    for(int j=0;j<d.zones[i].reservations.Length;j++){var a=d.zones[i].reservations[j];var b=old.zones[i].reservations[j];
     Check(a.reservationId==b.reservationId&&a.unitKey==b.unitKey&&a.localDueTime==b.localDueTime&&a.positionCandidateKey==b.positionCandidateKey&&a.sourceZoneId==b.sourceZoneId,"identity/time drift");
     Check(Math.Abs(a.position[1])>=z.egressRadius+z.actorInset+.55f-.0001f,"reservation in dash lane");
     if(j>0)Check(a.position[0]>d.zones[i].reservations[j-1].position[0],"reverse progression");
    }
   }
  });
  Test("missing-GUID-and-reservation-audit-rejected",()=>{
   string guid=p.assets[0].guid;p.assets[0].guid=null;Check(MorpgEnvironmentGeometry.Audit(p,d).Any(e=>e.Contains("asset reference")),"GUID accepted");p.assets[0].guid=guid;
   p.reservations[0].spawnValid=false;Check(MorpgEnvironmentGeometry.Audit(p,d).Any(e=>e.Contains("audit mismatch")),"audit mismatch accepted");p.reservations[0].spawnValid=true;
  });
  Test("strict-JSON-rejects-unknown-and-missing-fields",()=>{
   string json=File.ReadAllText(root+"/Assets/Resources/battle/morpg/environment/environment.v1.json");
   Check(MorpgEnvironmentGeometry.ValidateJson(json,out var e),e);
   Check(!MorpgEnvironmentGeometry.ValidateJson(json.Replace("\"schemaVersion\"","\"unknownSchema\""),out _),"unknown field ignored");
  });
  Test("authored-camera-envelope-rejects-void-and-accepts-expanded-map",()=>{
   float camera=p.cameraOrthographicSize;var bounds=p.mapBounds;
   p.cameraOrthographicSize=10;Check(MorpgEnvironmentGeometry.Audit(p,d).Any(e=>e.Contains("camera void")),"camera dimensions hardcoded");
   p.mapBounds=new[]{-200f,-20f,200f,20f};Check(!MorpgEnvironmentGeometry.Audit(p,d).Any(e=>e.Contains("camera void")),"expanded map ignored");
   p.cameraOrthographicSize=camera;p.mapBounds=bounds;
  });
  Test("visual-clustering-cannot-hide-structural-overlap",()=>{
   var a=p.zones[0].props[0];var b=p.zones[0].props[1];var original=b.center;b.center=a.center;
   Check(MorpgEnvironmentGeometry.Audit(p,d).Any(e=>e.Contains("structural overlap")),"overlapping trunks accepted");b.center=original;
  });
  Console.WriteLine("PASS "+count+"/"+count+" production geometry tests; horizontal profile valid");
 }
}
