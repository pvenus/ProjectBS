using UnityEngine;
[System.Serializable]
public class SkillProjectileVisualDto
{
    public AnimationClip animationClip;
    public Vector2 direction = Vector2.right;
    public int sortingOrder;
    public bool ensureSpriteRenderer = true;

    public string sortingLayerName = "Default";
}