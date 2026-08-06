using UnityEngine;
using UnityEngine.UI;

public class ScrollViewItemSpawner : MonoBehaviour
{
	[Header("Scroll View")]
	[SerializeField] private ScrollRect scrollRect;

	[Header("Item")]
	[SerializeField] private GameObject itemPrefab;

	public ItemTester Tester;

	public GameObject AddItem()
	{
		if (scrollRect == null)
		{
			Debug.LogError("[ScrollViewItemSpawner] ScrollRect is null.");
			return null;
		}

		if (scrollRect.content == null)
		{
			Debug.LogError("[ScrollViewItemSpawner] ScrollRect.content is null.");
			return null;
		}

		if (itemPrefab == null)
		{
			Debug.LogError("[ScrollViewItemSpawner] Item Prefab is null.");
			return null;
		}

		GameObject itemObject = Instantiate(itemPrefab, scrollRect.content);
		itemObject.GetComponentInChildren<Button>().onClick.AddListener(Tester.OnClickEvent);

		return itemObject;
	}

	public void ClearItems()
	{
		if (scrollRect == null || scrollRect.content == null)
		{
			return;
		}

		for (int i = scrollRect.content.childCount - 1; i >= 0; i--)
		{
			Destroy(scrollRect.content.GetChild(i).gameObject);
		}
	}

	private void Reset()
	{
		if (scrollRect == null)
		{
			scrollRect = GetComponentInChildren<ScrollRect>();
		}
	}
}