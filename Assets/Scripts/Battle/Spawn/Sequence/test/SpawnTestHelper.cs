using System.Collections.Generic;
using UnityEngine;
using Character;

/// <summary>
/// 소환된 몬스터의 일괄 삭제(즉사) 기능만 제공하는 테스트 헬퍼 컴포넌트
/// </summary>
public sealed class SpawnTestHelper : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("[SpawnTestHelper] 단축키 K 입력 감지 -> 활성 적 즉사 처리 개시");
            KillAllActiveEnemies();
        }
    }

    [ContextMenu("Kill All Active Enemies (Press K)")]
    public void KillAllActiveEnemies()
    {
        if (EnemyRegistry.Instance == null)
        {
            Debug.LogError("[SpawnTestHelper] EnemyRegistry.Instance가 존재하지 않습니다.");
            return;
        }

        IReadOnlyList<GameObject> enemies = EnemyRegistry.Instance.ActiveEnemies;
        if (enemies == null) return;

        int count = enemies.Count;
        Debug.Log($"[SpawnTestHelper] 활성 상태인 {count}마리의 적을 모두 파괴합니다.");

        List<GameObject> list = new List<GameObject>(enemies);
        for (int i = 0; i < list.Count; i++)
        {
            GameObject enemy = list[i];
            if (enemy == null) continue;

            Character.CharacterManager manager = enemy.GetComponent<Character.CharacterManager>();
            if (manager != null)
            {
                manager.TakeDamage(999999f);
            }
            else
            {
                Destroy(enemy);
            }
        }
    }
}
