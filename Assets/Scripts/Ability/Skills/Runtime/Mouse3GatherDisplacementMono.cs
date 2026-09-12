using Character;
using UnityEngine;

namespace Skill
{
    [DisallowMultipleComponent]
    public sealed class Mouse3GatherDisplacementMono : MonoBehaviour
    {
        private CharacterManager target;
        private MovementMono movement;
        private Vector2 center;
        private float remaining;
        private float stopRadius;
        private float speed;

        public void Begin(Vector2 destination, float distance, float duration, float clearance)
        {
            target ??= GetComponent<CharacterManager>() ?? GetComponentInParent<CharacterManager>();
            movement ??= target != null
                ? target.GetComponent<MovementMono>() ?? target.GetComponentInChildren<MovementMono>()
                : null;
            if (target == null || movement == null) { enabled = false; return; }
            center = destination;
            remaining = Mathf.Max(0f, distance);
            stopRadius = Mathf.Max(0f, clearance);
            speed = remaining / Mathf.Max(Time.fixedDeltaTime, duration);
            enabled = remaining > 0f;
        }

        private void FixedUpdate()
        {
            if (target == null || movement == null || !target.IsTargetable || remaining <= .0001f)
            { enabled = false; return; }
            Vector2 delta = center - (Vector2)target.transform.position;
            float available = Mathf.Max(0f, delta.magnitude - stopRadius);
            float step = Mathf.Min(remaining, Mathf.Min(available, speed * Time.fixedDeltaTime));
            if (step <= .0001f || !movement.ApplyDisplacement(delta, step))
            { enabled = false; return; }
            remaining -= step;
        }

        private void OnDisable()
        {
            remaining = 0f;
            speed = 0f;
        }
    }
}
