using UnityEngine;
using UnityEngine.UI;

public class AutoScrollTester : MonoBehaviour
{
	[SerializeField] private AutoScrollView autoScrollView;
	[SerializeField] private GameObject itemPrefab;

	public void AddItem()
	{
		autoScrollView.AddItem(itemPrefab, OnItemCreated);
	}

	private void OnItemCreated(GameObject itemObject)
	{
		Button button = itemObject.GetComponent<Button>();

		if (button != null)
		{
			button.onClick.AddListener(() =>
			{
				Debug.Log("Clicked: " + itemObject.name);
			});
		}
	}

	public void ClearItems()
	{
		autoScrollView.ClearItems();
	}
}