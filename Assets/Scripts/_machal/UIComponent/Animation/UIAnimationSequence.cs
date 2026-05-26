using System;
using System.Collections.Generic;
using UnityEngine;

public class UIAnimationSequence : MonoBehaviour
{
    [Serializable]
    public class Step
    {
        public List<UIAnimationBase> animations = new List<UIAnimationBase>();
    }

    public List<Step> sequenceSteps = new List<Step>();

    private int currentStepIndex = -1;
    private int runningAnimationsCount = 0;
    private Action onSequenceComplete;

    public void Append(UIAnimationBase anim)
    {
        if (anim == null) return;
        var step = new Step();
        step.animations.Add(anim);
        sequenceSteps.Add(step);
    }

    public void Join(UIAnimationBase anim)
    {
        if (anim == null) return;
        if (sequenceSteps.Count == 0) Append(anim);
        else sequenceSteps[sequenceSteps.Count - 1].animations.Add(anim);
    }

    public void Play(Action onComplete = null)
    {
        Stop();
        onSequenceComplete = onComplete;
        currentStepIndex = 0;
        PlayCurrentStep();
    }

    private void PlayCurrentStep()
    {
        if (currentStepIndex >= sequenceSteps.Count)
        {
            var cb = onSequenceComplete;
            onSequenceComplete = null;
            cb?.Invoke();
            return;
        }

        var step = sequenceSteps[currentStepIndex];
        runningAnimationsCount = step.animations.Count;

        if (runningAnimationsCount == 0)
        {
            OnAnimationComplete();
            return;
        }

        foreach (var anim in step.animations)
        {
            anim.Play(OnAnimationComplete);
        }
    }

    private void OnAnimationComplete()
    {
        runningAnimationsCount--;
        if (runningAnimationsCount <= 0)
        {
            currentStepIndex++;
            PlayCurrentStep();
        }
    }

    public void Stop()
    {
        foreach (var step in sequenceSteps)
        {
            foreach (var anim in step.animations)
            {
                anim.Stop();
            }
        }
        currentStepIndex = -1;
        runningAnimationsCount = 0;
        onSequenceComplete = null;
    }

    public void SkipToEnd()
    {
        if (currentStepIndex >= 0 && currentStepIndex < sequenceSteps.Count)
        {
            var currentStep = sequenceSteps[currentStepIndex];
            foreach (var anim in currentStep.animations)
            {
                anim.SkipToEnd();
            }

            for (int i = currentStepIndex + 1; i < sequenceSteps.Count; i++)
            {
                foreach (var anim in sequenceSteps[i].animations)
                {
                    anim.Play();
                    anim.SkipToEnd();
                }
            }
        }
    }

    public void Clear()
    {
        Stop();
        sequenceSteps.Clear();
    }

    [ContextMenu("Test Play")]
    public void TestPlay() => Play();
    
    [ContextMenu("Test Stop")]
    public void TestStop() => Stop();
    
    [ContextMenu("Test Skip")]
    public void TestSkip() => SkipToEnd();
}
