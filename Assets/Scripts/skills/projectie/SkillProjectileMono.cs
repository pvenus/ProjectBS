using UnityEngine;
using Skills.Dto;

public class SkillProjectileMono : MonoBehaviour
{
    private float _life;

    public void Initialize(SkillProjectileDto dto)
    {
        if (dto == null)
            return;

        _life = Mathf.Max(0.05f, dto.lifetime);
    }

    private void Update()
    {
        _life -= Time.deltaTime;
        if (_life <= 0f)
        {
            CleanupAndDestroy();
        }
    }

    private void CleanupAndDestroy()
    {
        //Destroy(gameObject);
    }
}