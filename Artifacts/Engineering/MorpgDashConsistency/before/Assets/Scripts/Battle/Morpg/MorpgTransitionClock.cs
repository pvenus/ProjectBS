using System;

namespace Battle.Morpg
{
    internal interface IMorpgPresentedTransition
    {
        bool TickTransition(float delta, out bool warp, out bool unlock, out bool wave, out string error);
        void TransitionWarped();
        void TransitionUnlocked();
        void TransitionWaveStarted();
    }
    internal enum MorpgDashPhase { Cleanup, Anticipation, Exit, FallbackOut, Invisible, Entry, FallbackIn, Settle, AwaitUnlock, WaveDelay, Ready }

    // One phase boundary per simulation tick preserves an actual invisible frame even on large deltas.
    internal sealed class MorpgTransitionClock
    {
        internal MorpgDashPhase Phase { get; private set; }
        internal bool Fallback { get; private set; }
        internal float Age { get; private set; }
        internal float ExitDuration { get; }
        internal float Progress => Phase==MorpgDashPhase.Exit ? Clamp(Age/ExitDuration) : Clamp(Age/.36f);
        internal float Alpha { get; private set; } = 1;
        internal float ScreenOpacity { get; private set; }
        internal bool WarpReady => Phase==MorpgDashPhase.Invisible && !warped;
        internal bool UnlockReady => Phase==MorpgDashPhase.AwaitUnlock;
        internal bool WaveReady => Phase==MorpgDashPhase.Ready;
        private bool warped;
        private int invisibleFrame;
        private float blockedAge, fallbackStartAlpha=1f;
        internal MorpgTransitionClock(float distance, bool fallback)
        { ExitDuration=Math.Max(.32f,Math.Min(.65f,distance/14f));Fallback=fallback; }
        private static float Clamp(float v)=>Math.Max(0,Math.Min(1,v));
        private static float Smooth(float v){v=Clamp(v);return v*v*(3-2*v);}
        private void Enter(MorpgDashPhase phase){Phase=phase;Age=0;}
        internal void UseFallback(float? previousAlpha=null){Alpha=Math.Min(Alpha,previousAlpha??Alpha);fallbackStartAlpha=Alpha;Fallback=true;Enter(MorpgDashPhase.FallbackOut);}
        internal void Tick(float delta,int frame,bool rewardsReady,bool canMove)
        {
            if(delta<=0 || float.IsNaN(delta) || float.IsInfinity(delta))return;
            Age+=delta;
            switch(Phase)
            {
                case MorpgDashPhase.Cleanup:
                    if(Age<.5f || !rewardsReady)return;
                    if(!canMove && !Fallback){blockedAge+=delta;if(blockedAge<.5f)return;Fallback=true;}
                    Enter(Fallback?MorpgDashPhase.FallbackOut:MorpgDashPhase.Anticipation);break;
                case MorpgDashPhase.Anticipation: if(Age>=.08f)Enter(MorpgDashPhase.Exit);break;
                case MorpgDashPhase.Exit:
                    Alpha=1-Smooth((Progress-.55f)/.45f);
                    if(Age>=ExitDuration){Alpha=0;invisibleFrame=frame;Enter(MorpgDashPhase.Invisible);}break;
                case MorpgDashPhase.FallbackOut:
                    Alpha=fallbackStartAlpha*(1-Clamp(Age/.06f));ScreenOpacity=Clamp(Age/.12f);
                    if(Age>=.12f){Alpha=0;ScreenOpacity=1;invisibleFrame=frame;Enter(MorpgDashPhase.Invisible);}break;
                case MorpgDashPhase.Invisible:
                    if(warped && frame>invisibleFrame)Enter(Fallback?MorpgDashPhase.FallbackIn:MorpgDashPhase.Entry);break;
                case MorpgDashPhase.Entry:
                    Alpha=Clamp(Age/.22f);
                    if(Age>=.36f){Alpha=1;Enter(MorpgDashPhase.Settle);}break;
                case MorpgDashPhase.FallbackIn:
                    Alpha=Clamp(Age/.16f);ScreenOpacity=1-Alpha;
                    if(Age>=.16f){Alpha=1;ScreenOpacity=0;Enter(MorpgDashPhase.Settle);}break;
                case MorpgDashPhase.Settle: if(Age>=(Fallback?.10f:.16f))Enter(MorpgDashPhase.AwaitUnlock);break;
                case MorpgDashPhase.WaveDelay: if(Age>=.20f)Enter(MorpgDashPhase.Ready);break;
            }
        }
        internal void MarkWarped(){warped=true;}
        internal void MarkUnlocked(){if(UnlockReady)Enter(MorpgDashPhase.WaveDelay);}
    }
}
