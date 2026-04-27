using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EquipmentSkillResolver를 사용하는 정식 투사체 발사 테스트용 MonoBehaviour.
/// 키 입력 시 EquipmentSkillSO + InstanceData를 해석하여 ProjectileRuntimeData를 만들고 발사한다.
/// </summary>
public class SkillTestMono : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EquipmentSkillSO equipmentSkill;
    [SerializeField] private EquipmentInventoryMono equipmentInventory;
    [SerializeField] private UIEquipmentUpgradeMono equipmentUpgradeUi;
    [SerializeField] private ProjectileEntity projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject target;

    [Header("Input")]
    [SerializeField] private KeyCode fireKey = KeyCode.Space;
    [SerializeField] private KeyCode acquireKey = KeyCode.A;
    [SerializeField] private KeyCode upgradeKey = KeyCode.U;

    [Header("Instance Data")]
    [SerializeField] private EquipmentGrade currentGrade = EquipmentGrade.Common;
    [SerializeField, Min(1)] private int currentRuneSlotCount = 1;
    [SerializeField] private ElementType mainElement = ElementType.None;
    [SerializeField] private ElementType[] subElements;
    [SerializeField] private float projectileLifetimeOverride = -1f;

    private ProjectileFactory projectileFactory;
    private EquipmentSkillResolver resolver;
    private OwnedEquipmentData testOwnedEquipment;

    private void Awake()
    {
        projectileFactory = new ProjectileFactory();
        resolver = new EquipmentSkillResolver();

        if (equipmentInventory == null)
        {
            equipmentInventory = GetComponent<EquipmentInventoryMono>();
        }

        if (equipmentUpgradeUi == null)
        {
            equipmentUpgradeUi = FindObjectOfType<UIEquipmentUpgradeMono>();
        }

        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(acquireKey))
        {
            AcquireTestEquipment();
        }

        if (Input.GetKeyDown(upgradeKey))
        {
            UpgradeTestEquipment();
        }

        if (Input.GetKeyDown(fireKey))
        {
            Fire();
        }
    }

    private void AcquireTestEquipment()
    {
        if (equipmentSkill == null)
        {
            Debug.LogError("SkillTestMono: equipmentSkill is null. Cannot acquire test equipment.", this);
            return;
        }

        if (equipmentInventory == null)
        {
            Debug.LogError("SkillTestMono: equipmentInventory is null. Add EquipmentInventoryMono or assign it.", this);
            return;
        }

        testOwnedEquipment = equipmentInventory.Acquire(equipmentSkill, currentGrade);
        if (testOwnedEquipment == null)
        {
            Debug.LogError("SkillTestMono: failed to acquire equipment.", this);
            return;
        }

        Debug.Log($"[Inventory Test] Acquired {testOwnedEquipment.DisplayName} / grade={testOwnedEquipment.CurrentGrade} / instanceId={testOwnedEquipment.InstanceId}", this);
    }

    private void UpgradeTestEquipment()
    {
        if (equipmentInventory == null)
        {
            Debug.LogError("SkillTestMono: equipmentInventory is null. Cannot upgrade test equipment.", this);
            return;
        }

        if (testOwnedEquipment == null)
        {
            Debug.LogWarning("[Inventory Test] No acquired test equipment. Press acquire key first.", this);
            return;
        }

        bool upgraded = equipmentInventory.TryUpgrade(testOwnedEquipment);
        Debug.Log($"[Inventory Test] Upgrade result={upgraded} / grade={testOwnedEquipment.CurrentGrade}", this);
    }

    private void Fire()
    {
        OwnedEquipmentData activeOwnedEquipment = GetActiveOwnedEquipment();
        EquipmentSkillSO activeEquipmentSkill = activeOwnedEquipment != null && activeOwnedEquipment.EquipmentSo != null
            ? activeOwnedEquipment.EquipmentSo
            : equipmentSkill;

        if (activeEquipmentSkill == null)
        {
            Debug.LogError("SkillTestMono: active equipment skill is null.", this);
            return;
        }

        EquipmentSkillInstanceData instanceData = BuildInstanceData(activeOwnedEquipment);
        EquipmentSkillRuntimeData runtime = resolver.Resolve(activeEquipmentSkill, instanceData);
        if (runtime == null)
        {
            Debug.LogError("SkillTestMono: resolver.Resolve returned null.", this);
            return;
        }

        Vector2 spawnPosition = firePoint.position;
        Vector2 direction = ResolveDirection(spawnPosition);

        ProjectileRuntimeData projectileData = resolver.ResolveProjectileRuntime(
            runtime,
            gameObject,
            target,
            spawnPosition,
            direction);

        if (projectileData == null)
        {
            Debug.LogError("SkillTestMono: resolver.ResolveProjectileRuntime returned null.", this);
            return;
        }

        if (projectileData.damageProfile != null)
        {
            Debug.Log($"[Upgrade Test] Damage = {projectileData.damageProfile.baseDamage}", this);
        }
        else
        {
            Debug.LogWarning("[Upgrade Test] damageProfile is null", this);
        }

        if (runtime.projectilePrefab == null)
        {
            Debug.LogError("SkillTestMono: runtime.projectilePrefab is null.", this);
            return;
        }

        projectileFactory.Spawn(runtime.projectilePrefab, projectileData);
    }

    private OwnedEquipmentData GetActiveOwnedEquipment()
    {
        if (equipmentUpgradeUi != null && equipmentUpgradeUi.SelectedEquipment != null)
        {
            return equipmentUpgradeUi.SelectedEquipment;
        }

        return testOwnedEquipment;
    }

    private EquipmentSkillInstanceData BuildInstanceData(OwnedEquipmentData activeOwnedEquipment)
    {
        if (activeOwnedEquipment != null)
        {
            EquipmentSkillInstanceData ownedInstanceData = activeOwnedEquipment.ToInstanceData();
            ownedInstanceData.mainElement = mainElement;
            ownedInstanceData.subElements = subElements != null ? new List<ElementType>(subElements) : new List<ElementType>();
            ownedInstanceData.projectilePrefab = projectilePrefab;

            if (projectileLifetimeOverride > 0f)
            {
                ownedInstanceData.projectileLifetimeOverride = projectileLifetimeOverride;
            }

            return ownedInstanceData;
        }

        return new EquipmentSkillInstanceData
        {
            equipmentId = equipmentSkill != null ? equipmentSkill.EquipmentId : string.Empty,
            currentGrade = currentGrade,
            currentRuneSlotCount = Mathf.Max(1, currentRuneSlotCount),
            mainElement = mainElement,
            subElements = subElements != null ? new List<ElementType>(subElements) : new List<ElementType>(),
            projectilePrefab = projectilePrefab,
            projectileLifetimeOverride = projectileLifetimeOverride
        };
    }

    private Vector2 ResolveDirection(Vector2 spawnPosition)
    {
        if (target != null)
        {
            Vector2 toTarget = (Vector2)target.transform.position - spawnPosition;
            if (toTarget.sqrMagnitude > 0.0001f)
            {
                return toTarget.normalized;
            }
        }

        return transform.right;
    }
}