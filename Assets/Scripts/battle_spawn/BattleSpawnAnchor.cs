using UnityEngine;

public sealed class BattleSpawnAnchor : MonoBehaviour
{
    [SerializeField] private string anchorId;

    public string AnchorId => anchorId;
}
