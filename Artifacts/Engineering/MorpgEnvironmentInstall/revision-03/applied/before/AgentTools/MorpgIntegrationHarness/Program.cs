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
   MorpgRewardHudMono.EnableForTests=false;
   GameSession.Instance=new GameSession();PartyManager.Instance=new PartyManager();NpcSpawnService.Instance=new NpcSpawnService();Time.timeScale=1;
   Player=new GameObject("player").AddComponent<CharacterManager>();PartyManager.Instance.Members.Add(Player);
   Session=new BattleSession{BattleSO=new Battle.BattleSO{BattleId=BattleMorpgDefinitionValidator.BattleId}};
   Check(BattleMorpgLiveRoute.TryCreate(true,Session,new Resolver(),new GameObject("host").transform,out Route,out var error),error);
   Check(Route.Activate(),"environment activation failed");
  }
  public void Tick(float delta=2){Time.frameCount++;Route.Tick(delta);}
  public void Clear(){Tick();foreach(var c in NpcSpawnService.Instance.Spawned.ToArray())if(!c.RuntimeData.isDead)c.Die();Tick();Tick();}
  public void AdvanceTransition(){for(int i=0;i<500;i++){Tick(.01f);if(Route.WaveRunning)return;}throw new Exception("transition did not release wave");}
  public void Dispose(){Route.Dispose();}
 }
 static void ReachPending(Fixture f){
  MorpgRewardHudMono.EnableForTests=true;f.Clear();f.AdvanceTransition();f.Clear();f.AdvanceTransition();f.Tick();
  foreach(var c in NpcSpawnService.Instance.Spawned.ToArray())if(!c.RuntimeData.isDead)c.Die();
  f.Tick(.01f);f.Tick(.01f);
  Check(f.Route.SettlementState==MorpgFinalSettlementState.FinalSettlementPending && !f.Route.VictoryReady,"pending treated as terminal");
 }
 static void WithDashCorridor(Action test){
  var saved=Resources.Items[MorpgEnvironmentGeometry.Resource];
  var json=((TextAsset)saved).text;Check(StrictCanonicalJson.TryParse(json,out var parsed,out var error),error);
  var root=(StrictCanonicalJson.ObjectNode)parsed;var zones=(StrictCanonicalJson.ArrayNode)root["zones"];
  void Rectangle(int i,double x0,double x1){var z=(StrictCanonicalJson.ObjectNode)zones[i];var points=new StrictCanonicalJson.ArrayNode();foreach(var v in new[]{new[]{x0,-7.35},new[]{x1,-7.35},new[]{x1,7.35},new[]{x0,7.35}})points.Add(new StrictCanonicalJson.ObjectNode{{"x",(decimal)v[0]},{"y",(decimal)v[1]}});z["walkable"]=points;}
  // Test-only envelopes make the approved anchors sweepable; production profile is never rewritten.
  Rectangle(0,-14.5,-3.75);Rectangle(1,-5,6.25);Rectangle(2,5.5,14.5);
  Resources.Items[MorpgEnvironmentGeometry.Resource]=new TextAsset(StrictCanonicalJson.Canonicalize(root));
  try{test();}finally{Resources.Items[MorpgEnvironmentGeometry.Resource]=saved;Camera.main=null;Physics2D.Overlaps=System.Array.Empty<Collider2D>();}
 }
 static void Test(string name,Action test){test();Console.WriteLine("PASS "+name);pass++;}
 static void Main(string[] args){
  string resource="battle/act1/chapter01/battle.act1.chapter01.01.rescue_villagers.morpg-zone-reward";
  foreach(string suffix in new[]{".v1",".p0-addendum.v1"})Resources.Items[resource+suffix]=new TextAsset(File.ReadAllText(Path.Combine(args[0],"Assets/Resources/"+resource+suffix+".json")));
  string environmentJson=File.ReadAllText(Path.Combine(args[0],"Assets/Resources/battle/morpg/environment/environment.v1.json"));
  Resources.Items[MorpgEnvironmentGeometry.Resource]=new TextAsset(environmentJson);
  var environmentProfile=JsonUtility.FromJson<MorpgEnvironmentProfile>(environmentJson);
  Resources.Items["battle/morpg/transitions/transition-dash"]=new AnimationClip();
  foreach(var asset in environmentProfile.assets)Resources.Items[asset.resource]=new Sprite();
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
   f.Tick(.5f);f.Tick(.12f);Check(f.Player.transform.position.x==0,"center warp");Check(NpcSpawnService.Instance.Spawned.Count==19,"early next wave");
   f.AdvanceTransition();Check(!BattleMorpgLiveRoute.IsTransitionLocked(f.Player),"lock leaked");f.Clear();Check(NpcSpawnService.Instance.Spawned.Count==24,"zone2 count");
   f.AdvanceTransition();f.Clear();Check(NpcSpawnService.Instance.Spawned.Count==28,"zone3 count");
   Check(f.Route.VictoryReady,"final victory missing");Check(GameSession.Instance.StageSession.CurrencyRuntimeData.gold==40,"gold conservation");Check(f.Player.xp==10,"raw xp conservation");Check(f.Session.BattleRuntime.rewardExperience==0,"final xp duplicate");
   f.Tick();Check(f.Player.xp==10,"victory retry credited");
  });
  Test("live-pause-transition-and-teardown-unlock",()=>{
   using var f=new Fixture();f.Clear();Time.timeScale=0;f.Tick(100);Check(f.Player.transform.position.x==-10.25f,"paused warp");Time.timeScale=1;f.Route.Dispose();Check(!BattleMorpgLiveRoute.IsTransitionLocked(f.Player),"teardown lock leak");
  });
  Test("live-final-frame-defeat-wins",()=>{
   using var f=new Fixture();f.Clear();f.AdvanceTransition();f.Clear();f.AdvanceTransition();f.Tick();
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
  Test("live-present-death-position-and-arrival-account-HUD-commit",()=>{
   using var f=new Fixture();MorpgRewardHudMono.EnableForTests=true;f.Tick(.1f);
   var enemy=NpcSpawnService.Instance.Spawned[0];enemy.transform.position=new Vector3(2,3,0);enemy.Die();
   Check(GameSession.Instance.StageSession.CurrencyRuntimeData.gold==0 && f.Player.xp==0,"old immediate credit path remained");
   Check(MorpgRewardHudMono.Last.Groups[0].X==2 && MorpgRewardHudMono.Last.Groups[0].Y==3,"death position lost");
   f.Tick(.45f);Check(MorpgRewardHudMono.Last.Receipts==0,"HUD credited before arrival");
   f.Tick(.35f);Check(GameSession.Instance.StageSession.CurrencyRuntimeData.gold==1 && f.Player.xp==.25f && MorpgRewardHudMono.Last.Receipts==1,"arrival did not commit accounts/UI");
   enemy.Die();Check(MorpgRewardHudMono.Last.Receipts==1,"duplicate arrival receipt");
  });
  Test("live-pause-and-disabled-HUD-lossless-failover",()=>{
   using var f=new Fixture();MorpgRewardHudMono.EnableForTests=true;f.Tick(.1f);NpcSpawnService.Instance.Spawned[0].Die();
   Time.timeScale=0;f.Tick(100);Check(GameSession.Instance.StageSession.CurrencyRuntimeData.gold==0,"paused commit");
   MorpgRewardHudMono.Last.LosePresentation();Check(GameSession.Instance.StageSession.CurrencyRuntimeData.gold==1,"disabled view lost reward");
   Time.timeScale=1;f.Tick();Check(GameSession.Instance.StageSession.CurrencyRuntimeData.gold==1,"failover replay");
  });
  Test("live-defeat-and-teardown-settle-pending-confirmed-kills",()=>{
   using(var f=new Fixture()){MorpgRewardHudMono.EnableForTests=true;f.Tick(.1f);NpcSpawnService.Instance.Spawned[0].Die();f.Player.Die();f.Tick(.01f);
    Check(GameSession.Instance.StageSession.CurrencyRuntimeData.gold==1 && f.Player.xp==.25f && !f.Route.VictoryReady,"defeat lost confirmed reward");}
   using(var f=new Fixture()){MorpgRewardHudMono.EnableForTests=true;f.Tick(.1f);NpcSpawnService.Instance.Spawned[0].Die();f.Route.Dispose();f.Route.Dispose();
    Check(GameSession.Instance.StageSession.CurrencyRuntimeData.gold==1 && f.Player.xp==.25f,"teardown lost/duplicated confirmed reward");}
  });
  Test("live-HUD-render-exception-fails-over-without-failing-battle",()=>{
   using var f=new Fixture();MorpgRewardHudMono.EnableForTests=true;f.Tick(.1f);NpcSpawnService.Instance.Spawned[0].Die();
   MorpgRewardHudMono.Last.ThrowTick=true;f.Tick(.1f);Check(GameSession.Instance.StageSession.CurrencyRuntimeData.gold==1,"HUD exception lost reward");
   f.Tick(2f);Check(NpcSpawnService.Instance.Spawned.Count==19,"optional HUD error stopped battle");
  });
  Test("live-presented-28-deaths-settle-before-final-HUD-snapshot",()=>{
   using var f=new Fixture();MorpgRewardHudMono.EnableForTests=true;f.Clear();f.AdvanceTransition();f.Clear();f.AdvanceTransition();f.Clear();
   Check(f.Route.VictoryReady && MorpgRewardHudMono.Last.Receipts==28 && MorpgRewardHudMono.Last.FinalGold==40 && f.Player.xp==10,"final presentation/account barrier");
  });
  Test("final-pending-arrival-and-duplicate-exactly-once",()=>{
   using var f=new Fixture();ReachPending(f);Check(BattleMorpgLiveRoute.IsTransitionLocked(f.Player),"final pending movement unlocked");
   f.Tick(.8f);Check(f.Route.SettlementState==MorpgFinalSettlementState.Completed && f.Route.VictoryReady,"arrival failed victory");
   foreach(var c in NpcSpawnService.Instance.Spawned)c.Die();f.Tick();
   Check(MorpgRewardHudMono.Last.Receipts==28 && f.Player.xp==10 && GameSession.Instance.StageSession.CurrencyRuntimeData.gold==40,"receipt duplicated");
   Check(!BattleMorpgLiveRoute.IsTransitionLocked(f.Player),"final lock leaked");
  });
  Test("final-pending-HUD-loss-flush-before-victory",()=>{
   using var f=new Fixture();ReachPending(f);MorpgRewardHudMono.Last.LosePresentation();f.Tick(.01f);
   Check(f.Route.VictoryReady && MorpgRewardHudMono.Last.Receipts==28,"HUD loss failed final settlement");
  });
  Test("final-pending-scaled-timeout-and-pause",()=>{
   using var f=new Fixture();ReachPending(f);MorpgRewardHudMono.Last.HoldArrivals=true;
   float elapsed=f.Route.FinalSettlementElapsed;Time.timeScale=0;f.Tick(100);
   Check(f.Route.FinalSettlementElapsed==elapsed && !f.Route.VictoryReady,"pause advanced settlement");
   Time.timeScale=1;f.Tick(1.26f);f.Tick();
   Check(f.Route.FinalSettlementTimedOut && f.Route.VictoryReady && MorpgRewardHudMono.Last.Receipts==28 && f.Player.xp==10,"bounded flush failed");
  });
  Test("final-pending-defeat-beats-arrival",()=>{
   using var f=new Fixture();ReachPending(f);f.Player.Die();f.Tick(.8f);
   Check(f.Route.SettlementState==MorpgFinalSettlementState.Defeated && !f.Route.VictoryReady && f.Player.xp==10,"defeat priority or confirmed rewards lost");
  });
  Test("final-pending-teardown-aborts-and-settles-once",()=>{
   using var f=new Fixture();ReachPending(f);f.Route.Dispose();f.Route.Dispose();
   Check(f.Route.SettlementState==MorpgFinalSettlementState.Abandoned && !f.Route.VictoryReady && f.Player.xp==10 && MorpgRewardHudMono.Last.Receipts==28,"abort became victory or replayed rewards");
  });
  Test("final-real-accounting-mismatch-fails",()=>{
   using var f=new Fixture();ReachPending(f);GameSession.Instance.StageSession.CurrencyRuntimeData.gold++;f.Tick(.8f);
   Check(f.Route.SettlementState==MorpgFinalSettlementState.Failed && !f.Route.VictoryReady,"real mismatch accepted");
  });
  Test("environment-real-runtime-roots-and-collider-inventory",()=>{
   using var f=new Fixture();f.Tick(.01f);
   var root=GameObject.All.Last(g=>g.name=="MORPG environment" && g.activeInHierarchy);
   bool Descendant(GameObject g){var t=g.transform.parent;while(t!=null){if(t.gameObject==root)return true;t=t.parent;}return false;}
   var owned=GameObject.All.Where(Descendant).ToArray();
   Check(owned.Count(g=>g.GetComponent<CircleCollider2D>()!=null)==8,"trunk inventory");
   Check(owned.Count(g=>g.GetComponent<BoxCollider2D>()!=null)==9,"fence/house/wall inventory");
   Check(owned.Count(g=>g.GetComponent<PolygonCollider2D>()!=null)==14,"closed boundary inventory");
   Check(owned.Where(g=>g.GetComponent<SpriteRenderer>()!=null).All(g=>g.GetComponent<Collider2D>()==null),"visual collider fringe");
   Check(owned.Count(g=>g.GetComponent<CircleCollider2D>()!=null && g.activeInHierarchy)==8,"initial active zone mismatch");
   f.Route.Dispose();Check(MorpgEnvironmentRuntime.Active==null && !root.activeInHierarchy,"environment teardown leaked");
  });
  Test("environment-destination-enable-before-source-disable",()=>{
   using var f=new Fixture();var runtime=MorpgEnvironmentRuntime.Active;
   var old=GameObject.All.Last(g=>g.name==environmentProfile.zones[0].zoneId);
   var next=GameObject.All.Last(g=>g.name==environmentProfile.zones[1].zoneId);
   Check(old.activeInHierarchy && !next.activeInHierarchy,"initial boundary state");
   Check(runtime.TryBeginWarp(1,out var e),e);Check(old.activeInHierarchy && next.activeInHierarchy,"boundary gap before placement");
   runtime.CompleteWarp(1);Check(!old.activeInHierarchy && next.activeInHierarchy && runtime.Zone.zoneId==next.name,"atomic completion mismatch");
  });
  Test("environment-missing-sprite-preflight-no-scene-mutation",()=>{
   var resourceKey=environmentProfile.assets[0].resource;var saved=Resources.Items[resourceKey];Resources.Items.Remove(resourceKey);
   try{
    var session=new BattleSession{BattleSO=new Battle.BattleSO{BattleId=BattleMorpgDefinitionValidator.BattleId}};
    GameSession.Instance=new GameSession();int count=GameObject.All.Count;
    Check(!BattleMorpgLiveRoute.TryCreate(true,session,new Resolver(),null,out _,out var e) && e.Contains("sprite missing"),"missing asset accepted");
    Check(GameObject.All.Count==count && MorpgEnvironmentRuntime.Active==null,"preflight mutated scene");
   }finally{Resources.Items[resourceKey]=saved;}
  });
  Test("environment-partial-activation-cleans-before-fallback",()=>{
   GameSession.Instance=new GameSession();
   var session=new BattleSession{BattleSO=new Battle.BattleSO{BattleId=BattleMorpgDefinitionValidator.BattleId}};
   Check(BattleMorpgLiveRoute.TryCreate(true,session,new Resolver(),null,out var route,out var e),e);
   GameObject.ThrowOnAddType="PolygonCollider2D";
   Check(!route.Activate(),"injected activation failure ignored");
   Check(MorpgEnvironmentRuntime.Active==null && !GameObject.All.Any(g=>g.name=="MORPG environment" && g.activeInHierarchy),"partial environment left active");
   route.Dispose();
  });
  Test("environment-camera-frame-and-restoration",()=>{
   Camera.main=new GameObject("camera").AddComponent<Camera>();Camera.main.orthographicSize=5;Camera.main.aspect=2;
   using(var f=new Fixture()){
    f.Tick(.01f);Check(Camera.main.orthographicSize==3 && Math.Abs(Camera.main.aspect-16f/9f)<.0001f,"camera frame missing");
    var p=MorpgEnvironmentRuntime.Active.ClampCamera(new Vector2(-100,100));Check(Math.Abs(p.x+10.42f)<.0001f && p.y==5.25f,"zone camera clamp missing");
   }
   Check(Camera.main.orthographicSize==5 && Camera.main.aspect==2,"camera baseline not restored");Camera.main=null;
  });
  Test("sorting-background-props-player-NPC-foreground-telegraph-HUD",()=>{
   using var f=new Fixture();f.Tick(.01f);
   foreach(var actor in new[]{f.Player,NpcSpawnService.Instance.Spawned[0]}){
    var body=actor.gameObject.AddComponent<SpriteRenderer>();body.sprite=new Sprite();
    var hud=new GameObject("health HUD");hud.transform.SetParent(actor.transform,false);hud.AddComponent<Party.UI.CharacterBattleHudUI>();
    var bar=hud.AddComponent<SpriteRenderer>();bar.sortingOrder=200;bar.sprite=new Sprite();
    var sorter=actor.gameObject.AddComponent<Util.SortingOrderMono>();
    typeof(Util.SortingOrderMono).GetMethod("Awake",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(sorter,null);
    var background=GameObject.All.Last(g=>g.name=="Full map dressing — collider0" && g.activeInHierarchy).GetComponent<SpriteRenderer>();
    var prop=GameObject.All.Last(g=>g.name.StartsWith("prop.") && g.activeInHierarchy).GetComponent<SpriteRenderer>();
    foreach(float y in new[]{-7.35f,-5f,0f,5f,7.35f}){
     actor.transform.position=new Vector3(0,y,0);sorter.UpdateSortingOrder();
     Check(background.sortingLayerID==body.sortingLayerID && prop.sortingLayerID==body.sortingLayerID,"layer mismatch");
     Check(background.sortingOrder<prop.sortingOrder && prop.sortingOrder<body.sortingOrder && body.sortingOrder<Battle.BattlePresentationSortingPolicy.MorpgForegroundProps && Battle.BattlePresentationSortingPolicy.MorpgForegroundProps<Battle.BattlePresentationSortingPolicy.MorpgGroundTelegraph && Battle.BattlePresentationSortingPolicy.MorpgGroundTelegraph<bar.sortingOrder,"body hidden or HUD behind body at y="+y);
     Check(body.color.a==1 && bar.sortingOrder==200,"body alpha or HUD mutated");
    }
   }
   Check(GameObject.All.Last(g=>g.name=="MORPG environment" && g.activeInHierarchy).transform.parent==null,"ancestor sorting group inherited");
  });
  Test("sorting-foreground-fades-prop-only-and-restores",()=>{
   using var f=new Fixture();f.Tick(.01f);var runtime=MorpgEnvironmentRuntime.Active;
   var body=f.Player.gameObject.AddComponent<SpriteRenderer>();body.sprite=new Sprite();body.TestBounds=new Bounds{center=new Vector3(-10.25f,0,0),size=new Vector3(1,1,0)};
   var props=GameObject.All.Where(g=>g.activeInHierarchy && (g.name.StartsWith("prop.")||g.name.StartsWith("dressing."))).Select(g=>g.GetComponent<SpriteRenderer>()).ToArray();
   foreach(var r in props)r.TestBounds=new Bounds{center=new Vector3(1000,1000,0),size=new Vector3(1,1,0)};
   var foreground=props[0];foreground.TestBounds=body.TestBounds;runtime.Tick(.12f);
   Check(Math.Abs(foreground.color.a-.35f)<.0001f && foreground.sortingOrder==Battle.BattlePresentationSortingPolicy.MorpgForegroundProps,"foreground fade missing");
   Check(body.color.a==1,"occlusion altered body alpha");
   body.TestBounds=new Bounds{center=new Vector3(-1000,-1000,0),size=new Vector3(1,1,0)};runtime.Tick(.18f);
   Check(foreground.color.a==1 && foreground.sortingOrder==Battle.BattlePresentationSortingPolicy.MorpgBackProps && body.color.a==1,"prop restore failed");
  });
  Test("sorting-pool-disable-dispose-and-foreign-actor-preserve-legacy",()=>{
   using var f=new Fixture();f.Tick(.01f);
   var body=f.Player.gameObject.AddComponent<SpriteRenderer>();f.Player.transform.position=new Vector3(0,6,0);
   var sorter=f.Player.gameObject.AddComponent<Util.SortingOrderMono>();
   var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
   typeof(Util.SortingOrderMono).GetMethod("Awake",flags).Invoke(sorter,null);sorter.UpdateSortingOrder();Check(body.sortingOrder==-700,"MORPG body band");
   typeof(Util.SortingOrderMono).GetMethod("OnDisable",flags).Invoke(sorter,null);Check(body.sortingOrder==-600,"pool baseline lost");
   sorter.UpdateSortingOrder();Check(body.sortingOrder==-700,"reuse failed");
   var foreign=new GameObject("foreign actor");foreign.AddComponent<CharacterManager>();foreign.transform.position=new Vector3(0,6,0);
   var fs=foreign.AddComponent<Util.SortingOrderMono>();typeof(Util.SortingOrderMono).GetMethod("Awake",flags).Invoke(fs,null);Check(fs.CalculateSortingOrder()==-600,"foreign actor changed");
   f.Route.Dispose();Check(body.sortingOrder==-600 && body.color.a==1,"dispose sorting/alpha restoration failed");
  });
  Test("dash-normal-body-exit-hidden-cut-entry-settle-wave",()=>WithDashCorridor(()=>{
   Camera.main=new GameObject("dash camera").AddComponent<Camera>();var follow=Camera.main.gameObject.AddComponent<Battle.BattlePlayerCameraFollowMono>();
   using var f=new Fixture();var animation=f.Player.gameObject.AddComponent<AnimationMono>();var animator=f.Player.gameObject.AddComponent<Animator>();animator.applyRootMotion=true;
   var body=f.Player.gameObject.AddComponent<SpriteRenderer>();body.sprite=new Sprite();body.flipX=true;f.Player.gameObject.AddComponent<Rigidbody2D>();
   f.Clear();var view=f.Route.TransitionView;Check(view!=null && !view.Clock.Fallback,"valid corridor selected fallback: "+view?.FallbackReason);
   float cameraX=Camera.main.transform.position.x;f.Tick(.5f);f.Tick(.08f);f.Tick(.1f);
   Check(f.Player.transform.position.x>-10.25f && Camera.main.transform.position.x==cameraX && !follow.enabled,"exit/camera ownership");
   float x=f.Player.transform.position.x,alpha=body.color.a;Time.timeScale=0;f.Tick(100);Check(f.Player.transform.position.x==x&&body.color.a==alpha,"pause advanced dash");Time.timeScale=1;
   for(int i=0;i<100&&view.Clock.Phase!=MorpgDashPhase.Invisible;i++)f.Tick(.01f);
   Check(body.color.a==0 && f.Player.transform.position.x==-3 && Camera.main.transform.position.x==0 && NpcSpawnService.Instance.Spawned.Count==19,"cut not hidden/atomic");
   f.Tick(.01f);Check(body.color.a==0,"no invisible rendered checkpoint");f.Tick(.18f);
   Check(f.Player.transform.position.x>-3&&f.Player.transform.position.x<0&&Camera.main.transform.position.x==0,"entry dash/camera");
   f.Tick(.181f);Check(f.Player.transform.position.x==0&&body.color.a==1&&BattleMorpgLiveRoute.IsTransitionLocked(f.Player),"landing idle gate");
   f.Tick(.161f);Check(!BattleMorpgLiveRoute.IsTransitionLocked(f.Player)&&!follow.enabled&&animator.applyRootMotion,"unlock restoration");
   f.Tick(.199f);Check(NpcSpawnService.Instance.Spawned.Count==19,"early wave");f.Tick(.002f);
   Check(NpcSpawnService.Instance.Spawned.Count==20&&!follow.enabled,"wave receipt or held camera");f.Tick(.01f);Check(follow.enabled&&animation.BodyPlays==2&&body.color.a==1&&body.flipX,"camera/body exact release");
  }));
  Test("dash-current-authored-invalid-corridor-safe-fallback",()=>{
   using var f=new Fixture();f.Player.gameObject.AddComponent<AnimationMono>();f.Player.gameObject.AddComponent<SpriteRenderer>();f.Clear();
   Check(f.Route.TransitionView.Clock.Fallback&&f.Route.TransitionView.FallbackReason=="authored dash corridor blocked","unsafe authored exit swept through wall");
   f.Tick(.5f);f.Tick(.12f);Check(f.Route.TransitionView.Clock.ScreenOpacity==1&&f.Player.transform.position.x==0,"fallback cut exposed");f.AdvanceTransition();
  });
  Test("dash-dynamic-exit-obstruction-enters-obscured-fallback",()=>WithDashCorridor(()=>{
   using var f=new Fixture();f.Player.gameObject.AddComponent<AnimationMono>();var sr=f.Player.gameObject.AddComponent<SpriteRenderer>();var rb=f.Player.gameObject.AddComponent<Rigidbody2D>();
   f.Clear();f.Tick(.5f);f.Tick(.08f);rb.BlockCast=new GameObject("dynamic obstruction").AddComponent<BoxCollider2D>();
   float x=f.Player.transform.position.x;f.Tick(.1f);Check(f.Route.TransitionView.Clock.Fallback&&f.Player.transform.position.x==x,"dynamic blocker penetrated");
   rb.BlockCast=null;f.Tick(.12f);Check(sr.color.a==0&&f.Route.TransitionView.Clock.ScreenOpacity==1,"dynamic fallback exposed cut");f.AdvanceTransition();
  }));
  Test("dash-entry-obstruction-falls-back-without-second-cut",()=>WithDashCorridor(()=>{
   using var f=new Fixture();f.Player.gameObject.AddComponent<AnimationMono>();var sr=f.Player.gameObject.AddComponent<SpriteRenderer>();var rb=f.Player.gameObject.AddComponent<Rigidbody2D>();f.Clear();
   var view=f.Route.TransitionView;for(int i=0;i<200&&view.Clock.Phase!=MorpgDashPhase.Entry;i++)f.Tick(.01f);
   f.Tick(.1f);float x=f.Player.transform.position.x,alpha=sr.color.a;rb.BlockCast=new GameObject("entry blocker").AddComponent<BoxCollider2D>();
   f.Tick(.01f);Check(view.Clock.Fallback&&f.Player.transform.position.x==x&&sr.color.a<=alpha,"entry fallback flashed or penetrated");
   rb.BlockCast=null;f.Tick(.12f);Check(sr.color.a==0&&view.Clock.ScreenOpacity==1&&f.Player.transform.position.x==0&&f.Route.ActiveZoneIndex==1,"hidden fallback landing");
   f.AdvanceTransition();Check(f.Route.ActiveZoneIndex==1&&NpcSpawnService.Instance.Spawned.Count==20,"fallback repeated cut or wave");
  }));
  Test("dash-abort-after-cut-restores-body-camera-input-once",()=>WithDashCorridor(()=>{
   Camera.main=new GameObject("abort camera").AddComponent<Camera>();var follow=Camera.main.gameObject.AddComponent<Battle.BattlePlayerCameraFollowMono>();
   using var f=new Fixture();f.Player.gameObject.AddComponent<AnimationMono>();var sr=f.Player.gameObject.AddComponent<SpriteRenderer>();sr.color=new Color(1,1,1,.7f);sr.flipX=true;
   var animator=f.Player.gameObject.AddComponent<Animator>();animator.applyRootMotion=true;f.Clear();
   for(int i=0;i<150&&f.Route.ActiveZoneIndex==0;i++)f.Tick(.01f);Check(f.Route.ActiveZoneIndex==1,"cut missing");
   f.Route.AbortActiveTransition();f.Route.AbortActiveTransition();
   Check(f.Player.transform.position.x==-10.25f&&Camera.main.transform.position.x==-10.25f&&follow.enabled&&sr.color.a==.7f&&sr.flipX&&animator.applyRootMotion&&!BattleMorpgLiveRoute.IsTransitionLocked(f.Player)&&!f.Route.VictoryReady,"abort lease leak");
  }));
  Test("dash-reduced-motion-and-missing-clip-never-play-body",()=>WithDashCorridor(()=>{
   using(var f=new Fixture()){var a=f.Player.gameObject.AddComponent<AnimationMono>();f.Player.gameObject.AddComponent<SpriteRenderer>();f.Route.ReducedMotionTransitions=true;f.Clear();
    Check(f.Route.TransitionView.FallbackReason=="reduced motion","reduced motion ignored");f.AdvanceTransition();Check(a.BodyPlays==0,"reduced motion played body");}
   var key="battle/morpg/transitions/transition-dash";var saved=Resources.Items[key];Resources.Items.Remove(key);
   try{using var f=new Fixture();var a=f.Player.gameObject.AddComponent<AnimationMono>();f.Player.gameObject.AddComponent<SpriteRenderer>();f.Clear();Check(f.Route.TransitionView.Clock.Fallback,"missing clip blocked route");f.AdvanceTransition();Check(a.BodyPlays==0,"missing clip played");}finally{Resources.Items[key]=saved;}
  }));
  Test("dash-blocked-landing-fails-before-cut-and-restores",()=>WithDashCorridor(()=>{
   using var f=new Fixture();f.Player.gameObject.AddComponent<AnimationMono>();var sr=f.Player.gameObject.AddComponent<SpriteRenderer>();f.Clear();
   Physics2D.Overlaps=new[]{new GameObject("landing blocked").AddComponent<BoxCollider2D>()};
   for(int i=0;i<200 && f.Route.SettlementState!=MorpgFinalSettlementState.Failed;i++)f.Tick(.01f);
   Check(f.Route.SettlementState==MorpgFinalSettlementState.Failed&&f.Player.transform.position.x==-10.25f&&sr.color.a==1&&!BattleMorpgLiveRoute.IsTransitionLocked(f.Player)&&NpcSpawnService.Instance.Spawned.Count==19,"invalid landing mutated next wave");
  }));
  Console.WriteLine("PASS "+pass+"/"+pass+" live glue simulations (Unity engine stub)");
 }
}
