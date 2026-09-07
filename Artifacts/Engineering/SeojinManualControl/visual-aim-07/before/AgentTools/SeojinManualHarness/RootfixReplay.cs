using System;using System.Reflection;using Character.Control;
class Replay {
 class Port:IManualGameplay {
  public bool Busy{get;set;}public int Calls,CancelCount;public Func<bool> Chain;
  public AimSnapshot Capture(int slot,AimSnapshot aim)=>aim;public bool Available(int s)=>true;public bool Ready(int s)=>s==0;
  public bool Fire(int s,AimSnapshot a,ControlVector d,Func<bool> chain){Calls++;Busy=true;Chain=chain;return true;}
  public void Move(ControlVector d){}public void Clear(bool interrupt){if(interrupt)CancelCount++;}
 }
 static void Main(){
  var core=new ManualControlCore();var port=new Port();var input=new ManualInputFrame{Aim=new AimSnapshot(new ControlVector(10,0),0,-4,32,4)};
  core.Tick(input,ControlReason.Manual,false,new ControlVector(1,0),port);
  input.AttackHeld=true;core.Tick(input,ControlReason.Manual,false,new ControlVector(1,0),port);
  input.SlotDown=1;for(int i=0;i<50;i++)core.Tick(input,ControlReason.Manual,false,new ControlVector(1,0),port);
  bool held=core.AttackHeld,chain=port.Chain();var field=typeof(ManualControlCore).GetField("hasPending",BindingFlags.NonPublic|BindingFlags.Instance);bool pending=field!=null&&(bool)field.GetValue(core);
  port.Busy=false;input.SlotDown=0;core.Tick(input,ControlReason.Manual,false,new ControlVector(1,0),port);
  Console.WriteLine("{\"held\":"+held.ToString().ToLower()+",\"pending\":"+pending.ToString().ToLower()+",\"chain\":"+chain.ToString().ToLower()+",\"basicCalls\":"+port.Calls+",\"cancelCount\":"+port.CancelCount+"}");
 }
}
