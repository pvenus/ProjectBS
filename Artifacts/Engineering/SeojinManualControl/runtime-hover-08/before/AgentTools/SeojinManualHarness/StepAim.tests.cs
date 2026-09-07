 // Additional tests execute the actual production combo coroutine and direction fallback.
 foreach(int grade in new[]{1,2,3}){
  var r=new EquipmentSkillRuntimeData();r.sourceEquipment.EquipmentId=$"skill.character.seojin.{grade}.basic_attack.basic_attack";
  h=new();var mouse=new Vector2(1,0);int captures=0;var times=new List<float>();
  Func<ManualSkillAim> capture=()=>{captures++;times.Add(h.Clock);return new ManualSkillAim{Mode=SkillAimMode.Direction,Direction=mouse};};
  h.Run(h.Start(r,false,new ManualSkillAim{Mode=SkillAimMode.Direction,Direction=Vector2.right},capture),t=>{
   if(t>=.94f)mouse=new Vector2(1,1);else if(t>=.64f)mouse=new Vector2(0,-1);else if(t>=.16f)mouse=new Vector2(-1,0);
  });
  Check(captures==3&&Near(times[0],0)&&Near(times[1],.48f)&&Near(times[2],.78f),$"G{grade} captures only actual step starts");
  Check(h.HitDirections[0].x==1&&h.HitDirections[1].x==-1&&h.HitDirections[2].y==-1,$"G{grade} A B C hit snapshots immutable through contact");
  Check(h.Anim.Directions.Zip(h.HitDirections,(a,b)=>a.x==b.x&&a.y==b.y).All(x=>x),$"G{grade} facing and hit/projectile/VFX context share step direction");
  Check(h.Anim.Locks.Count==3&&h.Anim.Locks.Zip(h.HitDirections,(a,b)=>a.x==b.x&&a.y==b.y).All(x=>x),$"G{grade} flip lock uses same immutable source");
  Check(h.Anim.FacingOwner==null&&h.comboFacingOwners.Count==0,$"G{grade} completion releases own facing lease");
  Check(h.Cooldowns==1&&Near(h.CooldownTimes[0],grade==1?.78f:1.06f),$"G{grade} cooldown timing preserved");
 }
 foreach(float release in new[]{.16f,.64f}){
  h=new();int captures=0;h.Run(h.Start(point:false,aim:new ManualSkillAim{Mode=SkillAimMode.Direction,Direction=Vector2.right},stepAim:()=>{captures++;return new ManualSkillAim{Mode=SkillAimMode.Direction,Direction=Vector2.right};}),t=>{if(t>=release)h.Held=false;});
  Check(captures==(release<.5f?1:2)&&h.Hits.Count==captures,"release does not read next snapshot but completes current hit");Check(h.Anim.FacingOwner==null,"release restores facing lease");
 }
 h=new();int canceledReads=0;var canceled=h.Start(point:false,aim:new ManualSkillAim{Mode=SkillAimMode.Direction,Direction=Vector2.right},stepAim:()=>{canceledReads++;return new ManualSkillAim{Mode=SkillAimMode.Direction,Direction=Vector2.right};});canceled.MoveNext();var firstOwner=h.Anim.FacingOwner;h.comboGenerations[1]=2;h.Anim.ReleaseFacingLock(firstOwner);var nextOwner=new object();h.Anim.AcquireFacingLock(nextOwner,new Vector2(-1,0));h.comboFacingOwners[1]=nextOwner;h.Run(canceled);Check(canceledReads==1&&h.Hits.Count==0,"canceled generation cannot capture next aim or hit");Check(h.Anim.FacingOwner==nextOwner&&h.comboFacingOwners[1]==nextOwner,"old finally cannot release new facing owner");
 h=new();int invalidReads=0;h.Run(h.Start(point:false,aim:new ManualSkillAim{Mode=SkillAimMode.Direction,Direction=Vector2.right},stepAim:()=>{invalidReads++;return new ManualSkillAim{Mode=SkillAimMode.Direction,Direction=invalidReads==3?default:Vector2.right};}));Check(h.Hits.Count==2&&h.Cooldowns==0&&invalidReads==3,"invalid provider fails closed before third cooldown commit");
 h=new();var disposable=h.Start(point:false,aim:new ManualSkillAim{Mode=SkillAimMode.Direction,Direction=Vector2.right});disposable.MoveNext();((IDisposable)disposable).Dispose();Check(h.Anim.FacingOwner==null&&h.comboFacingOwners.Count==0,"guard disposal releases manual facing on teardown");
 foreach(var bad in new[]{new ControlVector(0,0),new ControlVector(.001f,.001f),new ControlVector(float.NaN,0),new ControlVector(float.PositiveInfinity,0)}){
  var d=ManualGameplayAdapter.ResolveComboStepDirection(bad,new ControlVector(-1,1),new ControlVector(1,-1));Check(d.X<0&&d.Y>0,"epsilon or invalid mouse uses current WASD");
  d=ManualGameplayAdapter.ResolveComboStepDirection(bad,default,new ControlVector(-1,-1));Check(d.X<0&&d.Y<0,"no WASD uses last facing at this step");
 }
 var valid=ManualGameplayAdapter.ResolveComboStepDirection(new ControlVector(0,4),new ControlVector(-1,0),new ControlVector(-1,-1));Check(valid.X==0&&valid.Y==1,"valid mouse wins over WASD per step");
 var noInput=ManualGameplayAdapter.ResolveComboStepDirection(default,default,default);Check(noInput.X==1&&noInput.Y==0,"missing all directions safe right fallback");

 foreach(float blockedAt in new[]{.16f,.48f}){
  h=new();int reads=0;h.Run(h.Start(point:false,aim:new ManualSkillAim{Mode=SkillAimMode.Direction,Direction=Vector2.right},stepAim:()=>{reads++;return new ManualSkillAim{Mode=SkillAimMode.Direction,Direction=Vector2.right};}),t=>{if(t>=blockedAt)h.Manager.ManualComboAuthorized=false;});
  Check(reads==1&&h.Hits.Count==(blockedAt<.2f?0:1)&&h.Anim.FacingOwner==null,"live authorization blocks hit or next capture even before core cancellation");
 }
