using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class LobbyUITestPanel : MonoBehaviour
{
    public Transform buttonContainer;
    public Button buttonPrefab;

    public void AddButton(string label, UnityAction onClick)
    {
        if (buttonPrefab == null || buttonContainer == null)
        {
            Debug.LogWarning("[UITest] buttonPrefab or buttonContainer is not assigned in LobbyUITestPanel.");
            return;
        }

        Button newBtn = Instantiate(buttonPrefab, buttonContainer);
        newBtn.gameObject.SetActive(true);
        newBtn.onClick.AddListener(onClick);

        TMP_Text text = newBtn.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = label;
        }
    }
}
