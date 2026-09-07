using System;
using Battle.Morpg;
class ClockTests
{
 static int count;static void Check(bool b,string e){if(!b)throw new Exception(e);}static void Test(string n,Action a){a();Console.WriteLine("PASS "+n);count++;}
 static MorpgTransitionClock Exit(){var c=new MorpgTransitionClock(7,false);c.Tick(.5f,1,true,true);c.Tick(.08f,2,true,true);Check(c.Phase==MorpgDashPhase.Exit,"exit not started");return c;}
 static void Main(){
 Test("distance-speed14-duration-clamp",()=>{Check(new MorpgTransitionClock(.1f,false).ExitDuration==.32f,"min");Check(new MorpgTransitionClock(100,false).ExitDuration==.65f,"max");Check(new MorpgTransitionClock(7,false).ExitDuration==.5f,"speed");});
 Test("settlement-gate-and-anticipation",()=>{var c=new MorpgTransitionClock(7,false);c.Tick(2,1,false,true);Check(c.Phase==MorpgDashPhase.Cleanup,"unsettled movement");c.Tick(.01f,2,true,true);Check(c.Phase==MorpgDashPhase.Anticipation,"anticipation");});
 Test("exit-last45-percent-alpha-and-invisible-frame",()=>{var c=Exit();c.Tick(.25f,3,true,true);Check(c.Alpha==1,"early fade");c.Tick(.225f,4,true,true);Check(c.Alpha<.1f,"late fade");c.Tick(.03f,5,true,true);Check(c.Alpha==0&&c.WarpReady,"invisible cut");c.MarkWarped();c.Tick(1,5,true,true);Check(c.Phase==MorpgDashPhase.Invisible&&c.Alpha==0,"same-frame visible teleport");c.Tick(.01f,6,true,true);Check(c.Phase==MorpgDashPhase.Entry&&c.Alpha==0,"entry checkpoint");});
 Test("entry-idle-unlock-then-delayed-wave",()=>{var c=Exit();c.Tick(1,3,true,true);c.MarkWarped();c.Tick(.01f,4,true,true);c.Tick(.22f,5,true,true);Check(c.Alpha==1&&!c.UnlockReady,"entry fade");c.Tick(.141f,6,true,true);Check(c.Phase==MorpgDashPhase.Settle,"idle");c.Tick(.159f,7,true,true);Check(!c.UnlockReady,"early unlock");c.Tick(.002f,8,true,true);Check(c.UnlockReady,"unlock missing");c.MarkUnlocked();c.Tick(.199f,9,true,true);Check(!c.WaveReady,"early wave");c.Tick(.002f,10,true,true);Check(c.WaveReady,"wave missing");});
 Test("zero-delta-pause-freezes-all-progress",()=>{var c=Exit();c.Tick(.2f,3,true,true);float age=c.Age,alpha=c.Alpha;c.Tick(0,4,true,true);Check(c.Age==age&&c.Alpha==alpha,"pause advanced");});
 Test("missing-motion-or-clip-obscured-fallback",()=>{var c=new MorpgTransitionClock(7,true);c.Tick(.5f,1,true,true);c.Tick(.12f,2,true,true);Check(c.WarpReady&&c.Alpha==0&&c.ScreenOpacity==1,"fallback cut not obscured");c.MarkWarped();c.Tick(.01f,3,true,true);c.Tick(.16f,4,true,true);Check(c.Alpha==1&&c.ScreenOpacity==0&&c.Phase==MorpgDashPhase.Settle,"fallback restore");});
 Test("root-waits-bounded-without-removing-status",()=>{var c=new MorpgTransitionClock(7,false);c.Tick(.5f,1,true,false);Check(c.Fallback&&c.Phase==MorpgDashPhase.FallbackOut,"bounded root fallback");});
 Test("no-commit-no-entry-no-wave",()=>{var c=Exit();c.Tick(1,3,true,true);c.Tick(100,4,true,true);Check(c.Phase==MorpgDashPhase.Invisible&&!c.WaveReady,"uncommitted transition advanced");});
 Console.WriteLine("PASS "+count+"/"+count+" production transition clock tests");
 }
}
