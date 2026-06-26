using UnityEngine;

public abstract class SpawnContentSO : ScriptableObject
{
    [SerializeField] protected string contentId;
    [Min(0f)]
    [SerializeField] protected float spawnDelay = 0f;

    public string ContentId => contentId;
    public float SpawnDelay => spawnDelay;

    public virtual void Initialize(string id, float spawnDelay = 0f)
    {
        this.contentId = id;
        this.spawnDelay = spawnDelay;
    }
}
