using UnityEngine;
using Skills.Dto;

public class SkillProjectileMono : MonoBehaviour
{
    // runtime refresh (fixed interval)
    private float _refreshTimer;
    private const float REFRESH_INTERVAL = 0.2f;

    private SkillProjectileDto _dto;
    private Transform _caster;

    private SkillProjectileHitMono _hitMono;
    private SkillProjectileMoveMono _moveMono;
    private SkillProjectileLifeTimeMono _lifeMono;
    private int _spawnOrder;
    private int _currentMaxProjectileCount = 1;
    private bool _hasPendingAdditionalOrbitSpawn;
    private int _pendingOrbitSpawnCurrentCount;
    private int _pendingOrbitSpawnDesiredCount;

    public void Initialize(SkillProjectileDto dto)
    {
        if (dto == null)
            return;

        _dto = dto;
        _refreshTimer = 0f;
    }

    public void Initialize(
        SkillProjectileDto dto,
        Transform caster,
        SkillUpgradeMono.SkillUpgradeData upgradeData,
        Vector2 spawnPosition,
        Vector2 targetPosition,
        float resolvedScale,
        float resolvedLifetime,
        int spawnOrder,
        int maxProjectileCount)
    {
        if (dto == null)
        {
            Debug.LogError("SkillProjectileDto is null", this);
            return;
        }

        _dto = dto;
        _caster = caster;
        _spawnOrder = Mathf.Max(0, spawnOrder);
        _currentMaxProjectileCount = Mathf.Max(1, maxProjectileCount);
        _hasPendingAdditionalOrbitSpawn = false;
        _pendingOrbitSpawnCurrentCount = 0;
        _pendingOrbitSpawnDesiredCount = 0;

        transform.localScale = Vector3.one * Mathf.Max(0.01f, resolvedScale);

        _hitMono = GetComponentInChildren<SkillProjectileHitMono>(true);
        _moveMono = GetComponent<SkillProjectileMoveMono>() ?? gameObject.AddComponent<SkillProjectileMoveMono>();
        _lifeMono = GetComponent<SkillProjectileLifeTimeMono>() ?? gameObject.AddComponent<SkillProjectileLifeTimeMono>();

        if (_hitMono != null)
        {
            _hitMono.SetContext(new SkillProjectileHitMono.HitRuntimeContext
            {
                owner = caster,
                upgradeData = upgradeData
            });
        }

        if (dto.moveConfig != null)
        {
            if (dto.moveConfig.MoveType == SkillProjectileMoveDto.MoveType.Orbit)
            {
                SkillProjectileMoveDto orbitDto = dto.moveConfig.CreateDto(transform, spawnPosition, targetPosition);
                orbitDto.spawnOrder = _spawnOrder;
                orbitDto.maxProjectileCount = _currentMaxProjectileCount;
                _moveMono.Initialize(orbitDto, caster, transform, spawnPosition, targetPosition);
            }
            else
            {
                _moveMono.Initialize(dto.moveConfig, caster, transform, spawnPosition, targetPosition);
            }
        }
        else
        {
            Debug.LogWarning("Projectile move config is not assigned.", this);
        }

        _lifeMono.StartLife(Mathf.Max(0.01f, resolvedLifetime));
        _refreshTimer = 0f;
    }

    private void Update()
    {
        if (_hasPendingAdditionalOrbitSpawn)
        {
            _hasPendingAdditionalOrbitSpawn = false;

            if (_dto != null && _dto.sourceSkill != null && _caster != null)
            {
                _dto.sourceSkill.SpawnAdditionalOrbitProjectiles(
                    _caster,
                    _pendingOrbitSpawnCurrentCount,
                    _pendingOrbitSpawnDesiredCount);
            }
        }

        // runtime upgrade refresh (fixed interval)
        if (_dto != null && _dto.useRuntimeUpgradeRefresh && _caster != null)
        {
            _refreshTimer += Time.deltaTime;
            if (_refreshTimer >= REFRESH_INTERVAL)
            {
                _refreshTimer = 0f;
                RefreshUpgrade();
            }
        }
    }

    private void RefreshUpgrade()
    {
        if (_dto == null || _dto.sourceSkill == null)
            return;

        var upgradeMono = _caster != null ? _caster.GetComponentInParent<SkillUpgradeMono>() : null;
        if (upgradeMono == null)
            return;

        var upgradeData = upgradeMono.GetUpgradeData(_dto.sourceSkill);

        int desiredMaxProjectileCount = Mathf.Max(1, Mathf.RoundToInt(upgradeData.projectileCountAdd));
        if (_dto.sourceSkill != null)
        {
            desiredMaxProjectileCount = Mathf.Max(
                1,
                _dto.sourceSkill.ProjectileCount + Mathf.RoundToInt(upgradeData.projectileCountAdd));
        }

        if (_spawnOrder == 0 && desiredMaxProjectileCount > _currentMaxProjectileCount)
        {
            _hasPendingAdditionalOrbitSpawn = true;
            _pendingOrbitSpawnCurrentCount = _currentMaxProjectileCount;
            _pendingOrbitSpawnDesiredCount = desiredMaxProjectileCount;
        }

        _currentMaxProjectileCount = desiredMaxProjectileCount;

        float desiredScale = Mathf.Max(0.01f, _dto.sourceSkill.ProjectileScale + upgradeData.projectileScaleAdd);
        transform.localScale = Vector3.one * desiredScale;

        if (_hitMono != null)
        {
            _hitMono.ApplyUpgradeData(upgradeData);
        }

        // Runtime move upgrade apply (Orbit count 등)
        if (_moveMono != null)
        {
            var controller = _moveMono.GetController();
            if (controller != null)
            {
                controller.ApplyRuntimeUpgrade(upgradeData);
            }
        }
    }
}