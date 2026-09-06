using System;
using System.IO;
using System.Linq;
using Battle.Morpg;
using Character;
using UnityEngine;
using Session;
using Party;
class Program {
 static int pass;
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 sealed class Resolver:ISpawnUnitResolver {public readonly CharacterSO black=new(){name="black"},chain=new(){name="chain"};public CharacterSO Resolve(SpawnUnitRequest r)=>r.Key=="black"?black:chain;}
 sealed class Fixture:IDisposable {
  public BattleMorpgLiveRoute Route;public CharacterManager Player;public BattleSession Session;
  public Fixture(){
   GameSession.Instance=new GameSession();PartyManager.Instance=new PartyManager();NpcSpawnService.Instance=new NpcSpawnService();Time.timeScale=1;
   Player=new GameObject("player").AddComponent<CharacterManager>();PartyManager.Instance.Members.Add(Player);
   Session=new BattleSession{BattleSO=new Battle.BattleSO{BattleId=BattleMorpgDefinitionValidator.BattleId}};
   Check(BattleMorpgLiveRoute.TryCreate(true,Session,new Resolver(),new GameObject("host").transform,out Route,out var error),error);
   Route.Activate();
  }
  public void Tick(float delta=2){Time.frameCount++;Route.Tick(delta);}
  public void Clear(){Tick();foreach(var c in NpcSpawnService.Instance.Spawned.ToArray())if(!c.RuntimeData.isDead)c.Die();Tick();Tick();}
  public void Dispose(){Route.Dispose();}
 }
 static void Test(string name,Action test){test();Console.WriteLine("PASS "+name);pass++;}
 static void Main(string[] args){
  string resource="battle/act1/chapter01/battle.act1.chapter01.01.rescue_villagers.morpg-zone-reward";
  foreach(string suffix in new[]{".v1",".p0-addendum.v1"})Resources.Items[resource+suffix]=new TextAsset(File.ReadAllText(Path.Combine(args[0],"Assets/Resources/"+resource+suffix+".json")));
  Test("live-exact-gate-preflight-no-mutation",()=>{
   var s=new BattleSession{BattleSO=new Battle.BattleSO{BattleId="other"}};
   Check(!BattleMorpgLiveRoute.TryCreate(true,s,new Resolver(),null,out _,out _),"foreign battle accepted");
   s.BattleSO.BattleId=BattleMorpgDefinitionValidator.BattleId;
   Check(!BattleMorpgLiveRoute.TryCreate(false,s,new Resolver(),null,out _,out _),"disabled accepted");
   var saved=Resources.Items[resource+".v1"];Resources.Items.Remove(resource+".v1");
   Check(!BattleMorpgLiveRoute.TryCreate(true,s,new Resolver(),null,out _,out _),"missing definition accepted");Resources.Items[resource+".v1"]=saved;
  });
  Test("live-19-5-4-warp-reward-dedup-final",()=>{
   using var f=new Fixture();f.Clear();Check(NpcSpawnService.Instance.Spawned.Count==19,"zone1 count");
   Check(BattleMorpgLiveRoute.IsTransitionLocked(f.Player),"transition unlocked early");
   var dead=NpcSpawnService.Instance.Spawned[0];int gold=GameSession.Instance.StageSession.CurrencyRuntimeData.gold;dead.Die();Check(GameSession.Instance.StageSession.CurrencyRuntimeData.gold==gold,"duplicate reward");
   f.Tick(.7f);Check(f.Player.transform.position.x==0,"center warp");Check(NpcSpawnService.Instance.Spawned.Count==19,"early next wave");
   f.Tick(.6f);Check(!BattleMorpgLiveRoute.IsTransitionLocked(f.Player),"lock leaked");f.Clear();Check(NpcSpawnService.Instance.Spawned.Count==24,"zone2 count");
   f.Tick(1.3f);f.Clear();Check(NpcSpawnService.Instance.Spawned.Count==28,"zone3 count");
   Check(f.Route.VictoryReady,"final victory missing");Check(GameSession.Instance.StageSession.CurrencyRuntimeData.gold==40,"gold conservation");Check(f.Player.xp==10,"raw xp conservation");Check(f.Session.BattleRuntime.rewardExperience==0,"final xp duplicate");
   f.Tick();Check(f.Player.xp==10,"victory retry credited");
  });
  Test("live-pause-transition-and-teardown-unlock",()=>{
   using var f=new Fixture();f.Clear();Time.timeScale=0;f.Tick(100);Check(f.Player.transform.position.x==-10.25f,"paused warp");Time.timeScale=1;f.Route.Dispose();Check(!BattleMorpgLiveRoute.IsTransitionLocked(f.Player),"teardown lock leak");
  });
  Test("live-final-frame-defeat-wins",()=>{
   using var f=new Fixture();f.Clear();f.Tick(1.3f);f.Clear();f.Tick(1.3f);f.Tick();
   foreach(var c in NpcSpawnService.Instance.Spawned.ToArray())if(!c.RuntimeData.isDead)c.Die();f.Player.Die();f.Tick();Check(!f.Route.VictoryReady,"defeat became victory");
  });
  Test("live-spawn-fault-no-next-wave",()=>{
   using var f=new Fixture();NpcSpawnService.Instance.FailNext=true;f.Tick();f.Tick(100);Check(NpcSpawnService.Instance.Spawned.Count==0&&!f.Route.VictoryReady,"failed producer replayed");
  });
  Test("live-xp-notification-fault-rolls-back-gold-and-xp",()=>{
   using var f=new Fixture();f.Tick();f.Player.ThrowNextStat=true;NpcSpawnService.Instance.Spawned[0].Die();f.Tick();
   Check(GameSession.Instance.StageSession.CurrencyRuntimeData.gold==0 && f.Player.xp==0,"partial reward leaked");Check(!f.Route.VictoryReady,"failed reward won");
  });
  Test("live-owned-projectile-cleanup-preserves-foreign",()=>{
   using var f=new Fixture();f.Tick();var owner=NpcSpawnService.Instance.Spawned[0].GetComponent<MorpgOwnedObject>();var projectile=new GameObject("owned");var foreign=new GameObject("foreign");
   Check(owner.RegisterProjectile(projectile),"child registration");f.Clear();Check(projectile.destroyed&&!foreign.destroyed,"cleanup crossed ownership");Check(!owner.RegisterProjectile(new GameObject("late")),"late producer admitted");
  });
  Console.WriteLine("PASS "+pass+"/"+pass+" live glue simulations (Unity engine stub)");
 }
}
