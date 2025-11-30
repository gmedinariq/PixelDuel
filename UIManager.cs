using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Text")]
    public TextMeshProUGUI txtMessage;
    public TextMeshProUGUI txtRounds;
    public TextMeshProUGUI txtScore;
    public TextMeshProUGUI txtTimer;

    [Header("End Panel")]
    public GameObject panelEnd;
    public TextMeshProUGUI txtEndMessage;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void ShowMessage(string msg)
    {
        if (txtMessage) txtMessage.text = msg;
    }

    public void UpdateRounds(int current, int max)
    {
        if (txtRounds) txtRounds.text = current + " / " + max;
    }

    public void UpdateScore(int s)
    {
        if (txtScore) txtScore.text = "Score: " + s;
    }

    public void UpdateTimer(float t)
    {
        if (txtTimer) txtTimer.text = t.ToString("0.00");
    }

    public void ShowEnd(string msg)
    {
        if (panelEnd) panelEnd.SetActive(true);
        if (txtEndMessage) txtEndMessage.text = msg;
    }
}
