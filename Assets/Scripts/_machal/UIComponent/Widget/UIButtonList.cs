using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using System;

[Serializable]
public class UIButtonData
{
	public string id;
	public string text;
	public Action onClick;

	public UIButtonData(
		string id,
		string text,
		Action onClick)
	{
		this.id = id;
		this.text = text;
		this.onClick = onClick;
	}
}

public class UIButtonList : UIComponent
{
	[Header("References")]
	[AutoBind]
	[SerializeField]
	private Transform contentRoot;

	[Header("Button")]
	[SerializeField]
	private Button buttonPrefab;

	private readonly List<Button> spawnedButtons = new();

	public void SetButtonPrefab(Button prefab)
	{
		buttonPrefab = prefab;
	}

	public void SetButtons(IReadOnlyList<UIButtonData> buttons)
	{
		Clear();

		if (buttons == null)
		{
			return;
		}

		foreach (UIButtonData data in buttons)
		{
			AddButton(data);
		}
	}

	public Button AddButton(UIButtonData data)
	{
		if (buttonPrefab == null)
		{
			Debug.LogError("UIButtonList: buttonPrefab is null.");
			return null;
		}

		if (contentRoot == null)
		{
			contentRoot = transform;
		}

		Button button =
			Instantiate(buttonPrefab, contentRoot);

		TMP_Text text =
			button.GetComponentInChildren<TMP_Text>(true);

		if (text != null)
		{
			text.text = data.text;
		}

		button.onClick.RemoveAllListeners();

		if (data.onClick != null)
		{
			button.onClick.AddListener(() =>
			{
				data.onClick?.Invoke();
			});
		}

		spawnedButtons.Add(button);

		return button;
	}

	public void RemoveButton(Button button)
	{
		if (button == null)
		{
			return;
		}

		spawnedButtons.Remove(button);

		Destroy(button.gameObject);
	}

	public void Clear()
	{
		foreach (Button button in spawnedButtons)
		{
			if (button != null)
			{
				Destroy(button.gameObject);
			}
		}

		spawnedButtons.Clear();
	}
}