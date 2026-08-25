using UnityEngine;
using TMPro;
using System.Text;

public class LobbyUIDebugLogPanel : MonoBehaviour
{
    public TMP_Text logText;
    public int maxLines = 15;
    
    private StringBuilder sb = new StringBuilder();
    private int lineCount = 0;

    public void AddLog(string msg)
    {
        if (logText == null) return;

        sb.AppendLine(msg);
        lineCount++;

        // Keep only recent lines to prevent text overflow
        while (lineCount > maxLines)
        {
            string currentText = sb.ToString();
            int firstNewline = currentText.IndexOf('\n');
            if (firstNewline >= 0)
            {
                sb.Remove(0, firstNewline + 1);
                lineCount--;
            }
            else
            {
                break;
            }
        }

        logText.text = sb.ToString();
    }

    public void Clear()
    {
        sb.Clear();
        lineCount = 0;
        if (logText != null) logText.text = "";
    }
}
