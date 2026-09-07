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
  Test("superseded-geometry-passes-full-profile-audit",()=>{
   var errors=MorpgEnvironmentGeometry.Audit(p,d);
   File.WriteAllLines(root+"/Artifacts/Engineering/MorpgEnvironmentInstall/revision-02/geometry-errors.txt",errors);
   Check(errors.Count==0,string.Join("; ",errors));
  });
  Test("old-wall-and-house-coordinates-still-rejected",()=>{
   var z=p.zones[2];var wall=z.props.Single(q=>q.id.EndsWith("wall.right"));float original=wall.box[0];wall.box[0]=14;
   Check(MorpgEnvironmentGeometry.Audit(p,d).Any(e=>e.Contains("landing/+X")),"old corridor conflict hidden");wall.box[0]=original;
   var house=z.props.Single(q=>q.id.EndsWith("house.top.right"));var box=house.box;house.box=new[]{10f,6.2f,12.5f,7.25f};
   Check(MorpgEnvironmentGeometry.Audit(p,d).Any(e=>e.Contains("gap below")),"old gap accepted");house.box=box;
  });
  Test("spawn-validator-detects-original-tree-and-house-overlap",()=>{
   Check(!MorpgEnvironmentGeometry.SpawnValid(p.zones[0],new[]{-9f,-7f}),"tree spawn accepted");
   Check(!MorpgEnvironmentGeometry.SpawnValid(p.zones[2],new[]{10.25f,7f}),"house spawn accepted");
  });
  Test("sweep-stops-before-wall-no-tunneling",()=>{
   float t=MorpgEnvironmentGeometry.SweepFraction(p.zones[2],10.25f,0,30,0,.3f);
   float end=10.25f+(30-10.25f)*t;
   Check(end<=13.851f && end>=13.84f,"wall sweep endpoint "+end);
  });
  Test("sweep-respects-sloped-boundary-and-actor-radius",()=>{
   var z=p.zones[0];float t=MorpgEnvironmentGeometry.SweepFraction(z,-10.25f,0,-2,8,.3f);
   Check(t<1 && MorpgEnvironmentGeometry.Free(z,-10.25f+8.25f*t,8*t,.45f),"boundary escaped");
  });
  Test("landing-and-camera-derived-from-same-profile",()=>{
   Check(MorpgEnvironmentGeometry.LandingValid(p.zones[0]) && MorpgEnvironmentGeometry.LandingValid(p.zones[1]) && MorpgEnvironmentGeometry.LandingValid(p.zones[2]),"landings invalid");
   foreach(var z in p.zones)Check(z.cameraClamp[0]-16f/3>=-16 && z.cameraClamp[2]+16f/3<=16 && z.cameraClamp[1]-3>=-9 && z.cameraClamp[3]+3<=9,"camera void");
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
   p.mapBounds=new[]{-100f,-20f,100f,20f};Check(!MorpgEnvironmentGeometry.Audit(p,d).Any(e=>e.Contains("camera void")),"expanded map ignored");
   p.cameraOrthographicSize=camera;p.mapBounds=bounds;
  });
  Test("visual-clustering-cannot-hide-structural-overlap",()=>{
   var a=p.zones[0].props[0];var b=p.zones[0].props[1];var original=b.center;b.center=a.center;
   Check(MorpgEnvironmentGeometry.Audit(p,d).Any(e=>e.Contains("structural overlap")),"overlapping trunks accepted");b.center=original;
  });
  Console.WriteLine("PASS "+count+"/"+count+" production geometry tests; superseded profile valid");
 }
}
