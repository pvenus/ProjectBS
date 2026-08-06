using UnityEngine;

public class ScrollTester : MonoBehaviour
{
	[SerializeField] private ScrollViewItemSpawner spawner;

	public void AddOne()
	{
		spawner.AddItem();
	}

	public void Clear()
	{
		spawner.ClearItems();
	}
}
