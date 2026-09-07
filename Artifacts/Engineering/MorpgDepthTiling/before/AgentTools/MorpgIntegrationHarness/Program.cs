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
  public Fixture(Action<BattleSession> beforeActivate=null){
   MorpgRewardHudMono.EnableForTests=false;
   GameSession.Instance=new GameSession();PartyManager.Instance=new PartyManager();NpcSpawnService.Instance=new NpcSpawnService();Time.timeScale=1;
   Player=new GameObject("player").AddComponent<CharacterManager>();PartyManager.Instance.Members.Add(Player);
   Session=new BattleSession{BattleSO=new Battle.BattleSO{BattleId=BattleMorpgDefinitionValidator.BattleId}};
   beforeActivate?.Invoke(Session);
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
 static void WithInstalledDashProfile(Action test){
  // No geometry mutation: normal Dash is exercised against the installed profile.
  try{test();}finally{Camera.main=null;Physics2D.Overlaps=System.Array.Empty<Collider2D>();}
 }
 static Collider2D StructuralBlocker(string zoneId){var go=new GameObject("owned structural obstruction");go.transform.SetParent(GameObject.All.Last(g=>g.name==zoneId&&g.activeInHierarchy).transform,false);return go.AddComponent<BoxCollider2D>();}
 static void WithInteriors(Action test){
  var saved=Resources.Items[MorpgWaveDressing.Resource];
  Resources.Items[MorpgWaveDressing.Resource]=new TextAsset(((TextAsset)saved).text.Replace("\"interiorPropsEnabled\": false","\"interiorPropsEnabled\": true"));
  try{test();}finally{Resources.Items[MorpgWaveDressing.Resource]=saved;}
 }
 static void WithoutInteriors(Action test){
  var saved=Resources.Items[MorpgWaveDressing.Resource];
  Resources.Items[MorpgWaveDressing.Resource]=new TextAsset(((TextAsset)saved).text.Replace("\"interiorPropsEnabled\": true","\"interiorPropsEnabled\": false"));
  try{test();}finally{Resources.Items[MorpgWaveDressing.Resource]=saved;}
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
  var dressingJson=File.ReadAllText(Path.Combine(args[0],"Assets/Resources/"+MorpgWaveDressing.Resource+".json"));
  Resources.Items[MorpgWaveDressing.Resource]=new TextAsset(dressingJson);
  var dressing=JsonUtility.FromJson<MorpgWaveDressing>(dressingJson);
  foreach(var wave in dressing.waves)foreach(var asset in wave.assets)Resources.Items[asset.resource]=new Sprite{pivot=new Vector2(asset.pivot[0]*1536,asset.pivot[1]*512),bounds=new Bounds{size=new Vector3(15.36f,5.12f,0)}};
  JsonUtility.FromJson<MorpgWaveDressing>(dressingJson).Project(environmentProfile,null);
  float startX=environmentProfile.zones[0].entry[0],entryX=environmentProfile.zones[1].entry[0],stagingX=environmentProfile.zones[1].transitionStaging[0];
  float sourceCameraX=environmentProfile.zones[0].walkable.Min(v=>v.x)+environmentProfile.cameraOrthographicSize*16f/9f,targetCameraX=environmentProfile.zones[1].walkable.Min(v=>v.x)+environmentProfile.cameraOrthographicSize*16f/9f;
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
   f.Tick(.5f);f.Tick(.12f);Check(f.Player.transform.position.x==entryX,"center warp");Check(NpcSpawnService.Instance.Spawned.Count==19,"early next wave");
   f.AdvanceTransition();Check(!BattleMorpgLiveRoute.IsTransitionLocked(f.Player),"lock leaked");f.Clear();Check(NpcSpawnService.Instance.Spawned.Count==24,"zone2 count");
   f.AdvanceTransition();f.Clear();Check(NpcSpawnService.Instance.Spawned.Count==28,"zone3 count");
   Check(f.Route.VictoryReady,"final victory missing");Check(GameSession.Instance.StageSession.CurrencyRuntimeData.gold==40,"gold conservation");Check(f.Player.xp==10,"raw xp conservation");Check(f.Session.BattleRuntime.rewardExperience==0,"final xp duplicate");
   f.Tick();Check(f.Player.xp==10,"victory retry credited");
  });
  Test("live-pause-transition-and-teardown-unlock",()=>{
   using var f=new Fixture();f.Clear();Time.timeScale=0;f.Tick(100);Check(f.Player.transform.position.x==startX,"paused warp");Time.timeScale=1;f.Route.Dispose();Check(!BattleMorpgLiveRoute.IsTransitionLocked(f.Player),"teardown lock leak");
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
  Test("environment-real-runtime-roots-and-collider-inventory",()=>WithInteriors(()=>{
   using var f=new Fixture();f.Tick(.01f);
   var root=GameObject.All.Last(g=>g.name=="MORPG environment" && g.activeInHierarchy);
   bool Descendant(GameObject g){var t=g.transform.parent;while(t!=null){if(t.gameObject==root)return true;t=t.parent;}return false;}
   var owned=GameObject.All.Where(Descendant).ToArray();
   Check(owned.Count(g=>g.GetComponent<CircleCollider2D>()!=null)==8,"trunk inventory");
   Check(owned.Count(g=>g.GetComponent<BoxCollider2D>()!=null)==9,"fence/house/wall inventory");
   Check(owned.Count(g=>g.GetComponent<PolygonCollider2D>()!=null)==environmentProfile.zones.Sum(z=>z.walkable.Length)+18,"closed boundary + silhouette inventory");
   Check(owned.Where(g=>g.GetComponent<SpriteRenderer>()!=null).All(g=>g.GetComponent<Collider2D>()==null),"visual collider fringe");
   Check(owned.Count(g=>g.GetComponent<CircleCollider2D>()!=null && g.activeInHierarchy)==8,"initial active zone mismatch");
   f.Route.Dispose();Check(MorpgEnvironmentRuntime.Active==null && !root.activeInHierarchy,"environment teardown leaked");
  }));
  Test("environment-destination-isolated-until-atomic-placement",()=>{
   using var f=new Fixture();var runtime=MorpgEnvironmentRuntime.Active;
   var old=GameObject.All.Last(g=>g.name==environmentProfile.zones[0].zoneId);
   var next=GameObject.All.Last(g=>g.name==environmentProfile.zones[1].zoneId);
   Check(old.activeInHierarchy && !next.activeInHierarchy,"initial boundary state");
   Check(runtime.TryBeginWarp(1,out var e),e);Check(old.activeInHierarchy && !next.activeInHierarchy,"destination enabled before placement");
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
    f.Tick(.01f);Check(Camera.main.orthographicSize==environmentProfile.cameraOrthographicSize && Math.Abs(Camera.main.aspect-16f/9f)<.0001f,"camera frame missing");
    var p=MorpgEnvironmentRuntime.Active.ClampCamera(new Vector2(-100,100));Check(Math.Abs(p.x-sourceCameraX)<.0001f && p.y==dressing.layout.cameraY,"zone camera clamp missing");
   }
   Check(Camera.main.orthographicSize==5 && Camera.main.aspect==2,"camera baseline not restored");Camera.main=null;
  });
  Test("sorting-background-props-player-NPC-foreground-telegraph-HUD",()=>WithInteriors(()=>{
   using var f=new Fixture();f.Tick(.01f);
   foreach(var actor in new[]{f.Player,NpcSpawnService.Instance.Spawned[0]}){
    var body=actor.gameObject.AddComponent<SpriteRenderer>();body.sprite=new Sprite();
    var hud=new GameObject("health HUD");hud.transform.SetParent(actor.transform,false);hud.AddComponent<Party.UI.CharacterBattleHudUI>();
    var bar=hud.AddComponent<SpriteRenderer>();bar.sortingOrder=200;bar.sprite=new Sprite();
    var sorter=actor.gameObject.AddComponent<Util.SortingOrderMono>();
    typeof(Util.SortingOrderMono).GetMethod("Awake",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(sorter,null);
    var background=GameObject.All.Last(g=>g.name=="wave1.background" && g.activeInHierarchy).GetComponent<SpriteRenderer>();
    var prop=GameObject.All.Last(g=>g.name.StartsWith("prop.") && g.activeInHierarchy).GetComponent<SpriteRenderer>();
    foreach(float y in new[]{-7.35f,-5f,0f,5f,7.35f}){
     actor.transform.position=new Vector3(0,y,0);sorter.UpdateSortingOrder();
     Check(background.sortingLayerID==body.sortingLayerID && prop.sortingLayerID==body.sortingLayerID,"layer mismatch");
     Check(background.sortingOrder<prop.sortingOrder && prop.sortingOrder<body.sortingOrder && body.sortingOrder<Battle.BattlePresentationSortingPolicy.MorpgForegroundProps && Battle.BattlePresentationSortingPolicy.MorpgForegroundProps<Battle.BattlePresentationSortingPolicy.MorpgGroundTelegraph && Battle.BattlePresentationSortingPolicy.MorpgGroundTelegraph<bar.sortingOrder,"body hidden or HUD behind body at y="+y);
     Check(body.color.a==1 && bar.sortingOrder==200,"body alpha or HUD mutated");
    }
   }
   Check(GameObject.All.Last(g=>g.name=="MORPG environment" && g.activeInHierarchy).transform.parent==null,"ancestor sorting group inherited");
  }));
  Test("sorting-foreground-fades-prop-only-and-restores",()=>WithInteriors(()=>{
   using var f=new Fixture();f.Tick(.01f);var runtime=MorpgEnvironmentRuntime.Active;
   var body=f.Player.gameObject.AddComponent<SpriteRenderer>();body.sprite=new Sprite();body.TestBounds=new Bounds{center=new Vector3(startX,0,0),size=new Vector3(1,1,0)};
   var props=GameObject.All.Where(g=>g.activeInHierarchy && (g.name.StartsWith("prop.")||g.name.StartsWith("dressing."))).Select(g=>g.GetComponent<SpriteRenderer>()).ToArray();
   foreach(var r in props)r.TestBounds=new Bounds{center=new Vector3(1000,1000,0),size=new Vector3(1,1,0)};
   var foreground=props[0];foreground.TestBounds=body.TestBounds;runtime.Tick(.12f);
   Check(Math.Abs(foreground.color.a-.35f)<.0001f && foreground.sortingOrder==Battle.BattlePresentationSortingPolicy.MorpgForegroundProps,"foreground fade missing");
   Check(body.color.a==1,"occlusion altered body alpha");
   body.TestBounds=new Bounds{center=new Vector3(-1000,-1000,0),size=new Vector3(1,1,0)};runtime.Tick(.18f);
   Check(foreground.color.a==1 && foreground.sortingOrder==Battle.BattlePresentationSortingPolicy.MorpgBackProps && body.color.a==1,"prop restore failed");
  }));
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
  Test("dash-normal-body-exit-hidden-cut-entry-settle-wave",()=>WithInstalledDashProfile(()=>{
   Camera.main=new GameObject("dash camera").AddComponent<Camera>();var follow=Camera.main.gameObject.AddComponent<Battle.BattlePlayerCameraFollowMono>();
   using var f=new Fixture();var animation=f.Player.gameObject.AddComponent<AnimationMono>();var animator=f.Player.gameObject.AddComponent<Animator>();animator.applyRootMotion=true;
   var body=f.Player.gameObject.AddComponent<SpriteRenderer>();body.sprite=new Sprite();body.flipX=true;f.Player.gameObject.AddComponent<Rigidbody2D>();
   f.Clear();var view=f.Route.TransitionView;Check(view!=null && !view.Clock.Fallback,"valid corridor selected fallback: "+view?.FallbackReason);
   float cameraX=Camera.main.transform.position.x;f.Tick(.5f);f.Tick(.08f);f.Tick(.1f);
   Check(f.Player.transform.position.x>startX && Camera.main.transform.position.x==cameraX && !follow.enabled,"exit/camera ownership");
   float x=f.Player.transform.position.x,alpha=body.color.a;Time.timeScale=0;f.Tick(100);Check(f.Player.transform.position.x==x&&body.color.a==alpha,"pause advanced dash");Time.timeScale=1;
   for(int i=0;i<100&&view.Clock.Phase!=MorpgDashPhase.Invisible;i++)f.Tick(.01f);
   Check(body.color.a==0 && f.Player.transform.position.x==stagingX && Math.Abs(Camera.main.transform.position.x-targetCameraX)<.001f && NpcSpawnService.Instance.Spawned.Count==19,"cut not hidden/atomic");
   f.Tick(.01f);Check(body.color.a==0,"no invisible rendered checkpoint");f.Tick(.18f);
   Check(f.Player.transform.position.x>stagingX&&f.Player.transform.position.x<entryX&&Math.Abs(Camera.main.transform.position.x-targetCameraX)<.001f,"entry dash/camera");
   f.Tick(.181f);Check(f.Player.transform.position.x==entryX&&body.color.a==1&&BattleMorpgLiveRoute.IsTransitionLocked(f.Player),"landing idle gate");
   f.Tick(.161f);Check(!BattleMorpgLiveRoute.IsTransitionLocked(f.Player)&&!follow.enabled&&animator.applyRootMotion,"unlock restoration");
   f.Tick(.199f);Check(NpcSpawnService.Instance.Spawned.Count==19,"early wave");f.Tick(.002f);
   Check(NpcSpawnService.Instance.Spawned.Count==20&&!follow.enabled,"wave receipt or held camera");f.Tick(.01f);Check(follow.enabled&&animation.BodyPlays==0&&((AnimationClip)Resources.Items["battle/morpg/transitions/transition-dash"]).Samples>0&&body.color.a==1&&body.flipX,"camera/body exact release");
  }));
  Test("installed-both-dash-transitions-camera-and-wave-order",()=>WithInstalledDashProfile(()=>{
   Camera.main=new GameObject("two leg camera").AddComponent<Camera>();var follow=Camera.main.gameObject.AddComponent<Battle.BattlePlayerCameraFollowMono>();
   using var f=new Fixture();f.Player.gameObject.AddComponent<AnimationMono>();var sr=f.Player.gameObject.AddComponent<SpriteRenderer>();f.Player.gameObject.AddComponent<Rigidbody2D>();
   for(int leg=0;leg<2;leg++){
    f.Clear();var view=f.Route.TransitionView;Check(!view.Clock.Fallback,"installed leg selected fallback "+leg);
    int count=NpcSpawnService.Instance.Spawned.Count;bool hidden=false;
    for(int n=0;n<250&&!f.Route.WaveRunning;n++){
     f.Tick(.01f);var env=MorpgEnvironmentRuntime.Active;env.ApplyParallax();
     int activeOrdinal=env.Zone==env.RuntimeZone(leg)?leg:leg+1;
     var activeBackground=GameObject.All.Last(g=>g.name=="wave"+(activeOrdinal+1)+".background"&&g.activeInHierarchy);
     float bgCenter=env.Dressing.layout.Origin(activeOrdinal)+env.Dressing.layout.zoneWidth/2;
     Check(Math.Abs(activeBackground.transform.position.x-bgCenter-(Camera.main.transform.position.x-bgCenter)*env.Dressing.layout.parallaxFactor)<.001f,"transition parallax discontinuity");
     if(view.Clock.Phase==MorpgDashPhase.Invisible){hidden=true;Check(sr.color.a==0,"cut visible");}
     if(view.Clock.Phase!=MorpgDashPhase.Ready)Check(NpcSpawnService.Instance.Spawned.Count==count,"early reservation");
    }
    Check(hidden&&f.Route.WaveRunning&&f.Player.transform.position.x==environmentProfile.zones[leg+1].entry[0],"installed leg failed");
    Check(!follow.enabled,"camera released at receipt");f.Tick(.01f);Check(follow.enabled,"camera release missing");
   }
  }));
  Test("dash-missing-runtime-anchor-safe-fallback",()=>{
   using var f=new Fixture();f.Player.gameObject.AddComponent<AnimationMono>();f.Player.gameObject.AddComponent<SpriteRenderer>();MorpgEnvironmentRuntime.Active.RuntimeZone(0).transitionExit=null;f.Clear();
   Check(f.Route.TransitionView.Clock.Fallback&&f.Route.TransitionView.FallbackReason=="authored dash corridor blocked","unsafe authored exit swept through wall");
   f.Tick(.5f);f.Tick(.08f);f.Tick(.32f);Check(f.Route.TransitionView.Clock.ScreenOpacity==1&&f.Player.transform.position.x==entryX,"fallback cut exposed");f.AdvanceTransition();
  });
  Test("dash-dynamic-exit-obstruction-enters-obscured-fallback",()=>WithInstalledDashProfile(()=>{
   using var f=new Fixture();f.Player.gameObject.AddComponent<AnimationMono>();var sr=f.Player.gameObject.AddComponent<SpriteRenderer>();var rb=f.Player.gameObject.AddComponent<Rigidbody2D>();
   f.Clear();f.Tick(.5f);f.Tick(.08f);rb.BlockCast=StructuralBlocker(environmentProfile.zones[0].zoneId);
   float x=f.Player.transform.position.x;f.Tick(.1f);Check(f.Route.TransitionView.Clock.Fallback&&f.Player.transform.position.x==x,"dynamic blocker penetrated");
   rb.BlockCast=null;f.Tick(.32f);Check(sr.color.a==0&&f.Route.TransitionView.Clock.ScreenOpacity==1,"dynamic fallback exposed cut");f.AdvanceTransition();
  }));
  Test("dash-entry-obstruction-falls-back-without-second-cut",()=>WithInstalledDashProfile(()=>{
   using var f=new Fixture();f.Player.gameObject.AddComponent<AnimationMono>();var sr=f.Player.gameObject.AddComponent<SpriteRenderer>();var rb=f.Player.gameObject.AddComponent<Rigidbody2D>();f.Clear();
   var view=f.Route.TransitionView;for(int i=0;i<200&&view.Clock.Phase!=MorpgDashPhase.Entry;i++)f.Tick(.01f);
   f.Tick(.1f);float x=f.Player.transform.position.x,alpha=sr.color.a;rb.BlockCast=StructuralBlocker(environmentProfile.zones[1].zoneId);
   f.Tick(.01f);Check(view.Clock.Fallback&&f.Player.transform.position.x==x&&sr.color.a<=alpha,"entry fallback flashed or penetrated");
   rb.BlockCast=null;f.Tick(.32f);Check(sr.color.a==0&&view.Clock.ScreenOpacity==1&&f.Player.transform.position.x==entryX&&f.Route.ActiveZoneIndex==1,"hidden fallback landing");
   f.AdvanceTransition();Check(f.Route.ActiveZoneIndex==1&&NpcSpawnService.Instance.Spawned.Count==20,"fallback repeated cut or wave");
  }));
  Test("dash-abort-after-cut-restores-body-camera-input-once",()=>WithInstalledDashProfile(()=>{
   Camera.main=new GameObject("abort camera").AddComponent<Camera>();var follow=Camera.main.gameObject.AddComponent<Battle.BattlePlayerCameraFollowMono>();
   using var f=new Fixture();f.Player.gameObject.AddComponent<AnimationMono>();var sr=f.Player.gameObject.AddComponent<SpriteRenderer>();sr.color=new Color(1,1,1,.7f);sr.flipX=true;
   var animator=f.Player.gameObject.AddComponent<Animator>();animator.applyRootMotion=true;f.Clear();
   for(int i=0;i<150&&f.Route.ActiveZoneIndex==0;i++)f.Tick(.01f);Check(f.Route.ActiveZoneIndex==1,"cut missing");
   f.Route.AbortActiveTransition();f.Route.AbortActiveTransition();
   Check(f.Player.transform.position.x==startX&&Math.Abs(Camera.main.transform.position.x-sourceCameraX)<.001f&&follow.enabled&&sr.color.a==.7f&&sr.flipX&&animator.applyRootMotion&&!BattleMorpgLiveRoute.IsTransitionLocked(f.Player)&&!f.Route.VictoryReady,"abort lease leak");
  }));
  Test("dash-reduced-motion-and-missing-clip-never-play-body",()=>WithInstalledDashProfile(()=>{
   int priorSamples=((AnimationClip)Resources.Items["battle/morpg/transitions/transition-dash"]).Samples;
   using(var f=new Fixture()){var a=f.Player.gameObject.AddComponent<AnimationMono>();f.Player.gameObject.AddComponent<SpriteRenderer>();f.Route.ReducedMotionTransitions=true;f.Clear();
    Check(f.Route.TransitionView.FallbackReason=="reduced motion","reduced motion ignored");f.AdvanceTransition();Check(a.BodyPlays==0&&((AnimationClip)Resources.Items["battle/morpg/transitions/transition-dash"]).Samples==priorSamples,"reduced motion played body");}
   var key="battle/morpg/transitions/transition-dash";var saved=Resources.Items[key];Resources.Items.Remove(key);
   try{using var f=new Fixture();var a=f.Player.gameObject.AddComponent<AnimationMono>();f.Player.gameObject.AddComponent<SpriteRenderer>();f.Clear();Check(f.Route.TransitionView.Clock.Fallback,"missing clip blocked route");f.AdvanceTransition();Check(a.BodyPlays==0,"missing clip played");}finally{Resources.Items[key]=saved;}
  }));
  Test("dash-blocked-landing-fails-before-cut-and-restores",()=>WithInstalledDashProfile(()=>{
   using var f=new Fixture();f.Player.gameObject.AddComponent<AnimationMono>();var sr=f.Player.gameObject.AddComponent<SpriteRenderer>();f.Clear();
   Physics2D.Overlaps=new[]{StructuralBlocker(environmentProfile.zones[0].zoneId)};
   for(int i=0;i<200 && f.Route.SettlementState!=MorpgFinalSettlementState.Failed;i++)f.Tick(.01f);
   Check(f.Route.SettlementState==MorpgFinalSettlementState.Failed&&f.Player.transform.position.x==startX&&sr.color.a==1&&!BattleMorpgLiveRoute.IsTransitionLocked(f.Player)&&NpcSpawnService.Instance.Spawned.Count==19,"invalid landing mutated next wave");
  }));
  Test("dash-planner-all-legal-half-unit-grid-starts",()=>{
   int starts=0;
   foreach(var z in environmentProfile.zones.Take(2))
    for(float x=z.walkable.Min(v=>v.x)+.5f;x<=z.walkable.Max(v=>v.x)-.5f;x+=.5f)
     for(float y=-3.5f;y<=3.5f;y+=.5f){
      if(!MorpgEnvironmentGeometry.Free(z,x,y,.5f))continue;
      var path=MorpgDashExitPath.Plan(z,new Vector3(x,y,0),.35f);
      Check(path!=null,"legal clear path missing: "+x+","+y);Check(path.Length<=7,"offscreen long dash");
      for(int i=0;i<=20;i++){var point=path.Sample(i/20f);Check(MorpgEnvironmentGeometry.Free(z,point.x,point.y,.5f),"projection penetrates prop");}
      starts++;
     }
   Check(starts>1500,"insufficient clear-position coverage");Console.WriteLine("AUDIT "+starts+" legal clear positions swept");
  });
  Test("dash-top-bottom-center-right-edge-normal-visible-fade",()=>WithInstalledDashProfile(()=>{
   foreach(var point in new[]{new Vector3(5,3,0),new Vector3(15,-3,0),new Vector3(27,2.5f,0),new Vector3(30,0,0),new Vector3(31,2.5f,0),new Vector3(13,0,0)}){
    Camera.main=new GameObject("arbitrary clear camera").AddComponent<Camera>();Camera.main.gameObject.AddComponent<Battle.BattlePlayerCameraFollowMono>();
    using var f=new Fixture();var sr=f.Player.gameObject.AddComponent<SpriteRenderer>();f.Player.gameObject.AddComponent<AnimationMono>();f.Tick();
    f.Player.transform.position=point;var cameraPoint=MorpgEnvironmentRuntime.Active.ClampCamera(point);Camera.main.transform.position=new Vector3(cameraPoint.x,cameraPoint.y,-10);
    int logs=Debug.Messages.Count(m=>m.StartsWith("[MORPG transition]"));f.Clear();var view=f.Route.TransitionView;
    Check(view.Clock.Mode==MorpgDashMode.NormalDash,"arbitrary clear fallback "+point.x+","+point.y);
    f.Tick(.5f);f.Tick(.08f);f.Tick(view.Clock.ExitDuration*.6f);
    Check(sr.color.a>.3f&&sr.color.a<.8f&&view.Clock.ScreenOpacity==0,"normal fade not visible");
    Check(Math.Abs(f.Player.transform.position.x-cameraPoint.x)<Camera.main.orthographicSize*Camera.main.aspect,"fade outside camera");
    f.AdvanceTransition();Check(Debug.Messages.Count(m=>m.StartsWith("[MORPG transition]"))==logs+1,"start diagnostic duplicated");
   }
  }));
  Test("dash-cleared-NPC-and-projectile-colliders-do-not-obstruct",()=>WithInstalledDashProfile(()=>{
   foreach(string kind in new[]{"cleared NPC","cleared projectile"}){
    using var f=new Fixture();f.Player.gameObject.AddComponent<AnimationMono>();f.Player.gameObject.AddComponent<SpriteRenderer>();var rb=f.Player.gameObject.AddComponent<Rigidbody2D>();
    f.Clear();var unrelated=new GameObject(kind).AddComponent<BoxCollider2D>();rb.BlockCast=unrelated;Physics2D.Overlaps=new[]{unrelated};
    var view=f.Route.TransitionView;f.AdvanceTransition();Check(view.Clock.Mode==MorpgDashMode.NormalDash,"hostile corpse selected fallback");
    Physics2D.Overlaps=System.Array.Empty<Collider2D>();
   }
  }));
  Test("dash-CC-and-body-owner-retain-assisted-pose-without-state-mutation",()=>WithInstalledDashProfile(()=>{
   using var f=new Fixture();var animation=f.Player.gameObject.AddComponent<AnimationMono>();animation.BodyAvailable=false;animation.CurrentDirection=AnimationMono.DiagonalDirection.UpLeft;
   var sr=f.Player.gameObject.AddComponent<SpriteRenderer>();sr.flipX=true;f.Player.CanMove=false;var clip=(AnimationClip)Resources.Items["battle/morpg/transitions/transition-dash"];int samples=clip.Samples;
   f.Clear();f.Tick(.5f);var view=f.Route.TransitionView;
   Check(view.Clock.Mode==MorpgDashMode.AssistedDash&&clip.Samples>samples&&!sr.flipX,"CC removed Dash pose");
   Check(!f.Player.CanMove&&animation.CurrentDirection==AnimationMono.DiagonalDirection.UpLeft&&animation.BodyPlays==0,"status/facing owner mutated");
   f.Tick(.08f);f.Tick(.11f);Check(sr.color.a>0&&sr.color.a<1&&view.Clock.ScreenOpacity==0,"assisted fade hidden");
   f.AdvanceTransition();Check(!f.Player.CanMove&&sr.flipX,"status cleared or flip not restored");
  }));
  Test("dash-structural-assistance-one-way-keeps-pose-and-diagnostic",()=>WithInstalledDashProfile(()=>{
   using var f=new Fixture();var sr=f.Player.gameObject.AddComponent<SpriteRenderer>();f.Player.gameObject.AddComponent<AnimationMono>();var rb=f.Player.gameObject.AddComponent<Rigidbody2D>();var clip=(AnimationClip)Resources.Items["battle/morpg/transitions/transition-dash"];
   int logs=Debug.Messages.Count(m=>m.StartsWith("[MORPG transition assistance]"));f.Clear();f.Tick(.5f);f.Tick(.08f);
   rb.BlockCast=StructuralBlocker(environmentProfile.zones[0].zoneId);f.Tick(.05f);int samples=clip.Samples;var view=f.Route.TransitionView;
   f.Tick(.11f);Check(view.Clock.Mode==MorpgDashMode.AssistedDash&&clip.Samples>samples&&sr.color.a>0&&sr.color.a<1&&view.Clock.ScreenOpacity==0,"assistance dropped body");
   rb.BlockCast=null;f.AdvanceTransition();Check(view.Clock.Mode==MorpgDashMode.AssistedDash&&Debug.Messages.Count(m=>m.StartsWith("[MORPG transition assistance]"))==logs+1,"mode switched/repeated assistance");
  }));
  Test("dash-renderer-not-animation-manager-defines-visual-availability",()=>WithInstalledDashProfile(()=>{
   using(var f=new Fixture()){f.Player.gameObject.AddComponent<SpriteRenderer>();f.Clear();var view=f.Route.TransitionView;Check(view.Clock.Mode==MorpgDashMode.NormalDash,"missing manager disabled available clip");f.AdvanceTransition();}
   using(var f=new Fixture()){f.Player.gameObject.AddComponent<AnimationMono>();f.Clear();Check(f.Route.TransitionView.Clock.Mode==MorpgDashMode.NonDash&&f.Route.TransitionView.FallbackReason=="body clip/renderer unavailable","missing body renderer sampled HUD");f.AdvanceTransition();}
  }));
  Test("native-tile-scale-clipping-collider-and-wave-root-isolation",()=>{
   using var f=new Fixture();var env=MorpgEnvironmentRuntime.Active;
   var root=GameObject.All.Last(g=>g.name=="MORPG environment"&&g.activeInHierarchy);
   var all=GameObject.All.Where(g=>g.transform.IsChildOf(root.transform)).ToArray();
   for(int i=0;i<3;i++){
    env.CompleteWarp(i);float origin=env.Dressing.layout.Origin(i);
    for(int j=0;j<3;j++){
     var vr=all.Single(g=>g.name=="wave.visual."+j);Check(vr.activeInHierarchy==(i==j),"foreign wave visible");
     var cr=all.Single(g=>g.name==env.Profile.zones[j].zoneId);Check(cr.activeInHierarchy==(i==j),"foreign collider active");
    }
    var tiles=all.Where(g=>g.activeInHierarchy&&g.name.Contains(".tile.")&&g.GetComponent<SpriteRenderer>()!=null).ToArray();Check(tiles.Length==6,"native tile inventory");
    foreach(var go in tiles){
     var sr=go.GetComponent<SpriteRenderer>();Check(go.transform.localScale.x==1&&go.transform.localScale.y==1&&go.transform.localScale.z==1,"tile stretched");
     float hw=sr.sprite.rect.width/200;Check(go.transform.position.x-hw>=origin-.001f&&go.transform.position.x+hw<=origin+env.Dressing.layout.zoneWidth+.001f,"zone crop leaked");
     Check(sr.sortingOrder==(go.name.Contains(".top.")?-900:50),"barrier sorting drift");
     var collider=all.Single(g=>g.name=="silhouette."+go.name).GetComponent<PolygonCollider2D>();
     Check(collider.pathCount>0,"silhouette empty");foreach(var path in collider.Paths.Values)foreach(var point in path){float x=point.x+collider.transform.position.x;Check(x>=origin-.001f&&x<=origin+env.Dressing.layout.zoneWidth+.001f,"collider crop leaked");}
    }
   }
  });
  Test("wave-dressing-any-of9-missing-fails-before-scene-mutation",()=>{
   foreach(var wave in dressing.waves)foreach(var asset in wave.assets){
    var saved=Resources.Items[asset.resource];Resources.Items.Remove(asset.resource);int count=GameObject.All.Count;
    try{Check(!MorpgWaveDressing.TryLoad(environmentProfile,out _,out var error)&&error.Contains("sprite missing"),"partial dressing accepted");Check(GameObject.All.Count==count,"preflight scene mutation");}
    finally{Resources.Items[asset.resource]=saved;}
   }
  });
  Test("wave-dressing-unknown-field-sorting-and-invasive-collider-rejected",()=>{
   var saved=Resources.Items[MorpgWaveDressing.Resource];
   try{
    foreach(var bad in new[]{dressingJson.Replace("\"schemaVersion\"","\"unknownSchema\""),dressingJson.Replace("\"sortingOrder\": 50","\"sortingOrder\": -900")}){
     Resources.Items[MorpgWaveDressing.Resource]=new TextAsset(bad);Check(!MorpgWaveDressing.TryLoad(environmentProfile,out _,out _),"invalid binding accepted");}
    Check(StrictCanonicalJson.TryParse(dressingJson,out var parsed,out _),"parse");var node=(StrictCanonicalJson.ObjectNode)parsed;
    var wave=(StrictCanonicalJson.ObjectNode)((StrictCanonicalJson.ArrayNode)node["waves"])[0];var top=(StrictCanonicalJson.ObjectNode)((StrictCanonicalJson.ArrayNode)wave["assets"])[1];var patch=(StrictCanonicalJson.ObjectNode)((StrictCanonicalJson.ArrayNode)top["colliderRects"])[0];
    patch["rect"]=new StrictCanonicalJson.ArrayNode{(decimal)1,(decimal)2,(decimal)2,(decimal)3};Resources.Items[MorpgWaveDressing.Resource]=new TextAsset(StrictCanonicalJson.Canonicalize(node));
    Check(!MorpgWaveDressing.TryLoad(environmentProfile,out _,out _),"combat-invading silhouette accepted");
   }finally{Resources.Items[MorpgWaveDressing.Resource]=saved;}
  });
  Test("viewport-debug-off-gate-removes-interior-renderers-colliders-and-virtual-blocks",()=>WithoutInteriors(()=>{
   using var f=new Fixture();var env=MorpgEnvironmentRuntime.Active;
   Check(!env.Dressing.interiorPropsEnabled&&env.Profile.zones[0].props.Length==8&&env.RuntimeZone(0).props.Length==0,"definition/gate mismatch");
   var root=GameObject.All.Last(g=>g.name=="MORPG environment"&&g.activeInHierarchy);
   var objects=GameObject.All.Where(g=>g.transform.IsChildOf(root.transform)).ToArray();
   Check(objects.Count(g=>g.GetComponent<SpriteRenderer>()!=null)==21,"interior art still created");
   Check(objects.Count(g=>g.GetComponent<CircleCollider2D>()!=null||g.GetComponent<BoxCollider2D>()!=null)==0,"interior collider still created");
   var trunk=env.Profile.zones[0].props[0];Check(MorpgEnvironmentGeometry.Free(env.RuntimeZone(0),trunk.center[0],trunk.center[1],.15f),"invisible trunk still blocks");
   Check(MorpgDashExitPath.Plan(env.RuntimeZone(0),new Vector3(28,3,0),.35f)!=null,"Dash reads disabled prop definitions");
  }));
  Test("viewport-production-default-restores17-props54-decor-and-colliders",()=>{
   using var f=new Fixture();var env=MorpgEnvironmentRuntime.Active;
   Check(env.RuntimeZone(0).props.Length==8&&env.Dressing.interiorPropsEnabled,"reactivation lost definitions");
   var root=GameObject.All.Last(g=>g.name=="MORPG environment"&&g.activeInHierarchy);var objects=GameObject.All.Where(g=>g.transform.IsChildOf(root.transform)).ToArray();
   Check(objects.Count(g=>g.GetComponent<SpriteRenderer>()!=null)==92,"expected21 dressing+17 prop+54 decor renderers");
   Check(objects.Count(g=>g.GetComponent<CircleCollider2D>()!=null)==8&&objects.Count(g=>g.GetComponent<BoxCollider2D>()!=null)==9,"original17 structural colliders absent");
   Check(objects.Count(g=>g.name.StartsWith("dressing.")&&g.GetComponent<SpriteRenderer>()!=null)==54,"decoration count drift");
   for(int i=0;i<3;i++){
    var z=env.Profile.zones[i];var structural=GameObject.All.Last(g=>g.name==z.zoneId);var owned=objects.Where(g=>g.transform.IsChildOf(structural.transform)).ToArray();
    Check(owned.Count(g=>g.GetComponent<CircleCollider2D>()!=null||g.GetComponent<BoxCollider2D>()!=null)==new[]{8,4,5}[i],"per-zone structural collider count");
    Check(env.RuntimeZone(i).props.Length==new[]{8,4,5}[i],"runtime zone lost restored collision definitions");
    foreach(var prop in z.props)Check(!MorpgEnvironmentGeometry.Free(env.RuntimeZone(i),prop.center[0],prop.center[1],.15f),"restored prop center is traversable");
   }
   Console.WriteLine("AUDIT production true: props17/decor54, structural colliders8/4/5; all17 prop centers blocked");
   var trunk=env.Profile.zones[0].props[0];Check(!MorpgEnvironmentGeometry.Free(env.RuntimeZone(0),trunk.center[0],trunk.center[1],.15f),"reactivated collision absent");
  });
  Test("viewport-extents-side-limits-and-no-background-void-all-zones",()=>WithInstalledDashProfile(()=>{
   Camera.main=new GameObject("viewport geometry camera").AddComponent<Camera>();using var f=new Fixture();var env=MorpgEnvironmentRuntime.Active;
   foreach(float aspect in new[]{16f/9f,2f}){
    Camera.main.aspect=aspect;float hh=Camera.main.orthographicSize,hw=hh*aspect;
    for(int i=0;i<3;i++){
     env.CompleteWarp(i);var limits=env.CameraLimits(i);var bg=env.Dressing.waves[i].assets[0].rect;float left=env.RuntimeZone(i).walkable.Min(v=>v.x),right=left+32;
     Check(Math.Abs(limits.xMin-hw-left)<.001f&&Math.Abs(limits.xMax+hw-right)<.001f,"side edge not at viewport edge");
     foreach(float x in new[]{limits.xMin,limits.xMax})foreach(float y in new[]{limits.yMin,limits.yMax})Check(x-hw>=bg[0]&&x+hw<=bg[2]&&y-hh>=bg[1]&&y+hh<=bg[3],"viewport gray gap");
     var a=env.ClampActorCenter(new Vector2(-100,-100),.35f);var b=env.ClampActorCenter(new Vector2(200,100),.35f);
     Check(Math.Abs(a.x-left-.5f)<.001f&&Math.Abs(b.x-right+.5f)<.001f&&a.y==-3.5f&&b.y==3.5f,"player confused with camera-center limits");
     var settled=env.ClampCamera(new Vector2(left,0));for(int n=0;n<20;n++)settled=env.ClampCamera(settled);Check(settled.y==env.Dressing.layout.cameraY,"clamp accumulated offset");
     Check(env.CameraTarget(i,new Vector2(left+5,0)).y==env.Dressing.layout.cameraY,"arrival framing offset missing");
    }
   }
  }));
  Test("native-template-ridge-and-solid-core-clearance",()=>{
   using var f=new Fixture();var env=MorpgEnvironmentRuntime.Active;
   foreach(var wave in env.Dressing.waves)foreach(var a in wave.assets.Where(a=>a.kind!="background")){
    Check(Math.Abs(a.rect[2]-a.rect[0]-15.36f)<.001f&&Math.Abs(a.rect[3]-a.rect[1]-5.12f)<.001f,"template stretched");
    float edge=a.rect[3]-a.anchorRow/100;
    Check(Math.Abs(edge-(a.kind=="top"?4:env.Dressing.layout.cameraY-env.Dressing.layout.cameraHalfHeight+env.Dressing.layout.bottomInset))<.001f,"ridge framing");
    foreach(var q in a.colliderRects)Check(a.kind=="top"?q.rect[1]>=2.65f:q.rect[3]<=-2.65f,"solid penetrates core");
   }
  });
  Test("viewport-imported-pivot-rejected-and-tight-bounds-cannot-rescale-canvas",()=>{
   var asset=dressing.waves[0].assets[1];var sprite=(Sprite)Resources.Items[asset.resource];var pivot=sprite.pivot;var bounds=sprite.bounds;
   try{sprite.pivot=new Vector2(1,1);Check(!MorpgWaveDressing.TryLoad(environmentProfile,out _,out var error)&&error.Contains("canvas/pivot"),"wrong import pivot accepted");sprite.pivot=pivot;
    sprite.bounds=new Bounds{size=new Vector3(1,.1f,0)};using var f=new Fixture();var go=GameObject.All.Last(g=>g.name.StartsWith(asset.id+".tile.")&&g.activeInHierarchy);
    Check(Math.Abs(go.transform.localScale.y-(asset.rect[3]-asset.rect[1])/5.12f)<.0001f,"tight alpha bounds scaled transparent canvas twice");
   }finally{sprite.pivot=pivot;sprite.bounds=bounds;}
  });
  Test("legacy-background-normal1-success0-and-dispose-restores",()=>{
   SpriteRenderer legacy=null;
   using(var f=new Fixture(session=>{legacy=new GameObject("Background").AddComponent<SpriteRenderer>();legacy.sprite=new Sprite();MorpgLegacyBackground.Register(session.BattleRuntime,legacy);Check(legacy.enabled,"normal legacy hidden");})){
    Check(!legacy.enabled,"production legacy duplicated dressing");
    Check(GameObject.All.Count(g=>g.name=="wave1.background"&&g.activeInHierarchy&&g.GetComponent<SpriteRenderer>().enabled)==1,"wave1 background duplicate");
   }
   Check(legacy.enabled,"dispose did not restore legacy");
  });
  Test("legacy-background-test-path-after-activation-hidden-and-restored",()=>{
   SpriteRenderer legacy;using(var f=new Fixture()){
    legacy=new GameObject("Background").AddComponent<SpriteRenderer>();legacy.sprite=new Sprite();MorpgLegacyBackground.Register(f.Session.BattleRuntime,legacy);Check(!legacy.enabled,"late test background duplicated");
    f.Route.Dispose();f.Route.Dispose();Check(legacy.enabled,"test background restore leaked");
   }
  });
  Test("legacy-background-dressing-failure-remains-visible",()=>{
   GameSession.Instance=new GameSession();var session=new BattleSession{BattleSO=new Battle.BattleSO{BattleId=BattleMorpgDefinitionValidator.BattleId}};
   var legacy=new GameObject("Background").AddComponent<SpriteRenderer>();MorpgLegacyBackground.Register(session.BattleRuntime,legacy);
   Check(BattleMorpgLiveRoute.TryCreate(true,session,new Resolver(),null,out var route,out var error),error);
   GameObject.ThrowOnAddType="PolygonCollider2D";Check(!route.Activate()&&legacy.enabled,"activation failure lost fallback background");route.Dispose();
   var saved=Resources.Items[MorpgWaveDressing.Resource];Resources.Items.Remove(MorpgWaveDressing.Resource);
   try{Check(!BattleMorpgLiveRoute.TryCreate(true,session,new Resolver(),null,out _,out _)&&legacy.enabled,"preflight failure hid background");}finally{Resources.Items[MorpgWaveDressing.Resource]=saved;}
  });
  Test("legacy-background-owner-isolation-baseline-and-callsite-wiring",()=>{
   var a=new Battle.BattleRuntime();var b=new Battle.BattleRuntime();var sr=new GameObject("Background").AddComponent<SpriteRenderer>();sr.enabled=false;var other=new GameObject("Background").AddComponent<SpriteRenderer>();
   MorpgLegacyBackground.Register(a,sr);MorpgLegacyBackground.Register(b,other);
   var one=MorpgLegacyBackground.Acquire(a);var two=MorpgLegacyBackground.Acquire(a);one.Dispose();Check(!sr.enabled&&other.enabled,"foreign/background lease changed");two.Dispose();two.Dispose();Check(!sr.enabled&&other.enabled,"disabled baseline lost");
   Check(File.ReadAllText(Path.Combine(args[0],"Assets/Scripts/Battle/Core/BattleManager.cs")).Contains("MorpgLegacyBackground.Register(battleRuntime,renderer)"),"production registration missing");
   Check(File.ReadAllText(Path.Combine(args[0],"Assets/Scripts/Battle/Spawn/Sequence/BattleSpawnManager.cs")).Contains("MorpgLegacyBackground.Register(runtime,renderer)"),"test registration missing");
  });
  Test("center-pivot-canvas-PPU-strict-for-every-wave-slot",()=>{
   foreach(var wave in dressing.waves)foreach(var a in wave.assets){
    Check(a.pivot[0]==.5f&&a.pivot[1]==.5f,"binding not canonical center");var sprite=(Sprite)Resources.Items[a.resource];var rect=sprite.rect;float ppu=sprite.pixelsPerUnit;var pivot=sprite.pivot;
    try{
     sprite.rect=new Rect(0,0,1535,512);Check(!MorpgWaveDressing.TryLoad(environmentProfile,out _,out _),"wrong canvas accepted");sprite.rect=rect;
     sprite.pixelsPerUnit=99;Check(!MorpgWaveDressing.TryLoad(environmentProfile,out _,out _),"wrong PPU accepted");sprite.pixelsPerUnit=ppu;
     sprite.pivot=new Vector2(768,512);Check(!MorpgWaveDressing.TryLoad(environmentProfile,out _,out var error)&&error.Contains("actual rect=")&&error.Contains("pivot=("),"wrong pivot accepted or evidence missing");sprite.pivot=pivot;
    }finally{sprite.rect=rect;sprite.pixelsPerUnit=ppu;sprite.pivot=pivot;}
   }
   Check(MorpgWaveDressing.TryLoad(environmentProfile,out _,out var good),good);
  });
  Test("center-pivot-failure-legacy1-then-success-wave1-and-barrierTiles6-legacy0",()=>{
   GameSession.Instance=new GameSession();var session=new BattleSession{BattleSO=new Battle.BattleSO{BattleId=BattleMorpgDefinitionValidator.BattleId}};
   var legacy=new GameObject("Background").AddComponent<SpriteRenderer>();MorpgLegacyBackground.Register(session.BattleRuntime,legacy);
   var sprite=(Sprite)Resources.Items[dressing.waves[0].assets[1].resource];var original=sprite.pivot;
   try{sprite.pivot=new Vector2(768,512);Check(!BattleMorpgLiveRoute.TryCreate(true,session,new Resolver(),null,out _,out _)&&legacy.enabled,"pivot failure suppressed legacy");}finally{sprite.pivot=original;}
   Check(BattleMorpgLiveRoute.TryCreate(true,session,new Resolver(),null,out var route,out var error),error);
   using(route){Check(route.Activate()&&!legacy.enabled,"canonical center did not activate MORPG");
    Check(GameObject.All.Count(g=>g.activeInHierarchy&&g.name=="wave1.background"&&g.GetComponent<SpriteRenderer>().enabled)==1,"wave1 background duplicate");
    Check(GameObject.All.Count(g=>g.activeInHierarchy&&(g.name.StartsWith("wave1.top.tile.")||g.name.StartsWith("wave1.bottom.tile."))&&g.GetComponent<SpriteRenderer>().enabled)==6,"wave1 barriers missing");
    Check(MorpgEnvironmentRuntime.Active.Dressing.interiorPropsEnabled,"interiors disabled");
   }
   Check(legacy.enabled,"legacy lease not restored");
  });
  Test("layout-projection-single-origin-preserves-canonical-contract-and-local-anchors",()=>{
   using var f=new Fixture();var env=MorpgEnvironmentRuntime.Active;
   var canonical=JsonUtility.FromJson<MorpgEnvironmentProfile>(environmentJson);
   for(int i=0;i<3;i++){
    var old=canonical.zones[i];var z=env.Profile.zones[i];float delta=env.Dressing.layout.Origin(i)-old.walkable.Min(v=>v.x);
    Check(Math.Abs(z.entry[0]-old.entry[0]-delta)<.001f&&(old.transitionStaging==null||Math.Abs(z.transitionStaging[0]-old.transitionStaging[0]-delta)<.001f)&&(old.transitionExit==null||Math.Abs(z.transitionExit[0]-old.transitionExit[0]-delta)<.001f),"anchor projection drift");
    foreach(var row in env.Profile.reservations.Where(v=>v.zoneId==z.zoneId)){var before=canonical.reservations.Single(v=>v.reservationId==row.reservationId);Check(Math.Abs(row.position[0]-before.position[0]-delta)<.001f&&row.position[1]==before.position[1]&&row.role==before.role,"spawn identity/projection drift");}
    Check(Math.Abs(z.walkable.Max(v=>v.x)-z.walkable.Min(v=>v.x)-env.Dressing.layout.zoneWidth)<.001f,"zone width changed");
   }
   Check(((TextAsset)Resources.Items[MorpgEnvironmentGeometry.Resource]).text==environmentJson,"canonical profile mutated");
  });
  Test("parallax-world-follow-point20-extremes-zero-uncovered-no-solid-motion",()=>{
   Camera.main=new GameObject("parallax camera").AddComponent<Camera>();using var f=new Fixture();var env=MorpgEnvironmentRuntime.Active;
   var frames=new System.Collections.Generic.List<object>();
   var root=GameObject.All.Last(g=>g.name=="MORPG environment"&&g.activeInHierarchy);var all=GameObject.All.Where(g=>g.transform.IsChildOf(root.transform)).ToArray();
   var fixedObjects=all.Where(g=>g.GetComponent<Collider2D>()!=null||(g.GetComponent<SpriteRenderer>()!=null&&!g.name.EndsWith(".background"))).ToDictionary(g=>g,g=>g.transform.position);
   for(int i=0;i<3;i++){
    float origin=env.Dressing.layout.Origin(i);var limits=env.CameraLimits(i);float hh=Camera.main.orthographicSize,hw=hh*Camera.main.aspect;
    foreach(var view in new[]{"center","left","right","center-return"}){
     float cx=view=="left"?limits.xMin:view=="right"?limits.xMax:origin+env.Dressing.layout.zoneWidth/2;
     Camera.main.transform.position=new Vector3(cx,env.Dressing.layout.cameraY,-10);env.CompleteWarp(i);env.ApplyParallax();
     var bg=all.Single(g=>g.name=="wave"+(i+1)+".background");var rect=env.Dressing.waves[i].assets[0].rect;
     float dx=(cx-origin-env.Dressing.layout.zoneWidth/2)*env.Dressing.layout.parallaxFactor;
     Check(Math.Abs(bg.transform.position.x-(rect[0]+rect[2])/2-dx)<.0001f,"world-follow factor is not .20");
     Check(cx-hw>=rect[0]+dx&&cx+hw<=rect[2]+dx&&Camera.main.transform.position.y-hh>=rect[1]&&Camera.main.transform.position.y+hh<=rect[3],"parallax uncovered viewport");
     var rendered=all.Where(g=>g.activeInHierarchy&&g.GetComponent<SpriteRenderer>()!=null).Select(g=>new {name=g.name,position=new[]{g.transform.position.x,g.transform.position.y},scale=new[]{g.transform.localScale.x,g.transform.localScale.y},crop=new[]{g.GetComponent<SpriteRenderer>().sprite.rect.x,g.GetComponent<SpriteRenderer>().sprite.rect.y,g.GetComponent<SpriteRenderer>().sprite.rect.width,g.GetComponent<SpriteRenderer>().sprite.rect.height}}).ToArray();
     frames.Add(new{wave=i+1,view,camera=new[]{cx,env.Dressing.layout.cameraY},rendered});
    }
   }
   foreach(var pair in fixedObjects)Check(pair.Key.transform.position.x==pair.Value.x&&pair.Key.transform.position.y==pair.Value.y,"parallax moved barrier/prop/collider");
   File.WriteAllText(Path.Combine(args[0],"Artifacts/Engineering/MorpgNativeTiling/runtime-frames.json"),new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(frames));
  });
  Test("invisible-cut-resets-parallax-before-destination-enabled",()=>{
   Camera.main=new GameObject("atomic parallax camera").AddComponent<Camera>();using var f=new Fixture();var env=MorpgEnvironmentRuntime.Active;
   var old=GameObject.All.Last(g=>g.name=="wave.visual.0");var next=GameObject.All.Last(g=>g.name=="wave.visual.1");
   Camera.main.transform.position=new Vector3(env.CameraLimits(0).xMax,env.Dressing.layout.cameraY,-10);env.ApplyParallax();
   Check(env.TryBeginWarp(1,out var error,true),error);Check(!old.activeInHierarchy&&!next.activeInHierarchy,"invisible cut leaked roots");
   float target=env.CameraLimits(1).xMin;Camera.main.transform.position=new Vector3(target,env.Dressing.layout.cameraY,-10);env.CompleteWarp(1);
   var bg=GameObject.All.Last(g=>g.name=="wave2.background"&&g.activeInHierarchy);float center=env.Dressing.layout.Origin(1)+env.Dressing.layout.zoneWidth/2;
   Check(Math.Abs(bg.transform.position.x-center-(target-center)*env.Dressing.layout.parallaxFactor)<.001f&&!old.activeInHierarchy&&next.activeInHierarchy,"destination inherited source origin");
   float x=bg.transform.position.x;for(int n=0;n<10;n++)env.ApplyParallax();Check(bg.transform.position.x==x,"fixed transition camera drift");
  });
  Test("unsafe-layout-parameters-reject-before-activation",()=>{
   var saved=Resources.Items[MorpgWaveDressing.Resource];
   try{foreach(var bad in new[]{dressingJson.Replace("\"zoneGap\": 20","\"zoneGap\": 1"),dressingJson.Replace("\"overlap\": 0.12","\"overlap\": 0.01"),dressingJson.Replace("\"parallaxFactor\": 0.2","\"parallaxFactor\": 0.8")}){
    Resources.Items[MorpgWaveDressing.Resource]=new TextAsset(bad);Check(!MorpgWaveDressing.TryLoad(environmentProfile,out _,out _),"unsafe layout accepted");
   }}finally{Resources.Items[MorpgWaveDressing.Resource]=saved;}
  });
  File.WriteAllLines(Path.Combine(args[0],"Artifacts/Engineering/MorpgDashConsistency/diagnostics.log"),Debug.Messages);
  Console.WriteLine("PASS "+pass+"/"+pass+" live glue simulations (Unity engine stub)");
 }
}
