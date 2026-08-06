using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class AutoBindBehaviour : MonoBehaviour
{
#if UNITY_EDITOR

    protected virtual void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        AutoBindEditorUtility.Bind(this);
    }

#endif
}
