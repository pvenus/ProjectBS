using UnityEngine;

namespace Battle.Presentation.Ambient
{
    public sealed class BattleAmbientActor : MonoBehaviour
    {
        private BattleAmbientController.AmbientKind kind;
        private Camera targetCamera;
        private SpriteRenderer spriteRenderer;
        private Sprite[] animationFrames;
        private float framesPerSecond;
        private Vector2 viewportPadding;
        private Vector3 origin;
        private Vector3 baseScale;
        private float age;
        private float lifetime;
        private float phase;
        private float speed;
        private float rotationSpeed;
        private float movementDirection;
        private bool infiniteLifetime;

        public BattleAmbientController.AmbientKind Kind => kind;

        public void Initialize(
            BattleAmbientController.AmbientKind ambientKind,
            Sprite[] frames,
            float animationFramesPerSecond,
            Camera camera,
            float duration,
            float scale,
            Vector2 padding,
            float direction,
            float movementSpeed,
            bool loopForever)
        {
            kind = ambientKind;
            animationFrames = frames;
            framesPerSecond = Mathf.Max(1f, animationFramesPerSecond);
            targetCamera = camera;
            lifetime = Mathf.Max(0.1f, duration);
            infiniteLifetime = loopForever;
            viewportPadding = padding;
            spriteRenderer = GetComponent<SpriteRenderer>();
            origin = transform.position;
            phase = Random.Range(0f, Mathf.PI * 2f);
            movementDirection = Mathf.Sign(direction);
            speed = Mathf.Max(0f, movementSpeed);
            rotationSpeed = Random.Range(90f, 180f) * (movementDirection == 0f ? 1f : movementDirection);
            baseScale = Vector3.one * scale * Random.Range(0.85f, 1.15f);
            transform.localScale = baseScale;
        }

        private void Update()
        {
            age += Time.deltaTime;
            float normalizedAge = infiniteLifetime ? 0f : Mathf.Clamp01(age / lifetime);

            if (spriteRenderer != null && animationFrames is { Length: > 0 })
            {
                int frameIndex = Mathf.FloorToInt(age * framesPerSecond)
                    % animationFrames.Length;
                if (animationFrames[frameIndex] != null)
                {
                    spriteRenderer.sprite = animationFrames[frameIndex];
                }
            }

            switch (kind)
            {
                case BattleAmbientController.AmbientKind.BirdFlock:
                    UpdateBirds();
                    break;
                case BattleAmbientController.AmbientKind.WindGust:
                    UpdateWind();
                    break;
                case BattleAmbientController.AmbientKind.DryLeaves:
                    UpdateLeaves();
                    break;
                case BattleAmbientController.AmbientKind.GrassTuft:
                    UpdateGrass();
                    break;
            }

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = infiniteLifetime
                    ? Mathf.Clamp01(age / 0.5f)
                    : Mathf.Clamp01(Mathf.Min(normalizedAge / 0.12f, (1f - normalizedAge) / 0.18f));
                spriteRenderer.color = color;
            }

            if ((!infiniteLifetime && age >= lifetime) || HasLeftCamera())
            {
                Destroy(gameObject);
            }
        }

        private void UpdateBirds()
        {
            transform.position += Vector3.right * (movementDirection * speed * Time.deltaTime);
            transform.position += Vector3.up * (Mathf.Sin(Time.time * 3.2f + phase) * 0.004f);
        }

        private void UpdateWind()
        {
            transform.position += new Vector3(movementDirection, 0.05f, 0f) * (speed * Time.deltaTime);
        }

        private void UpdateLeaves()
        {
            transform.position += new Vector3(movementDirection, -0.08f, 0f) * (speed * Time.deltaTime);
            transform.position += Vector3.up * (Mathf.Abs(Mathf.Sin(Time.time * 4.2f + phase)) * 0.018f);
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        private void UpdateGrass()
        {
            transform.position = origin;
            transform.rotation = Quaternion.identity;
        }

        private bool HasLeftCamera()
        {
            if (targetCamera == null || kind == BattleAmbientController.AmbientKind.GrassTuft)
            {
                return false;
            }

            Vector3 viewport = targetCamera.WorldToViewportPoint(transform.position);
            return viewport.x > 1f + viewportPadding.x
                || viewport.x < -viewportPadding.x
                || viewport.y < -viewportPadding.y;
        }
    }

    public sealed class BattleAmbientGroup : MonoBehaviour
    {
        private float lifetime;
        private float age;

        public float Lifetime => lifetime;
        public float Age => age;

        public void Initialize(float groupLifetime)
        {
            lifetime = Mathf.Max(0.1f, groupLifetime);
            age = 0f;
        }

        private void Update()
        {
            age += Time.deltaTime;
            if (age >= lifetime || transform.childCount == 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
