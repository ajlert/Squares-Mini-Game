using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    #region INITIAL_DATA

    [SerializeField]
    Button restartBtn;
    [SerializeField]
    Text scoreValueText;
    [SerializeField]
    GameObject bestScorePanel;

    #endregion

    // Use this for initialization
    void Start ()
    {
        restartBtn.onClick.AddListener(OnRestartClick);
	}

    public void UpdateGameOverInfo(int curScore, int bestScore)
    {
        scoreValueText.text = curScore.ToString();

        // if this is a new best score then turn on best score panel
        if (curScore > bestScore)
            bestScorePanel.SetActive(true);
    }

    private void OnDisable()
    {
        // turn off best score panel if it was active
        if (bestScorePanel.activeSelf)
            bestScorePanel.SetActive(false);
    }

    void OnRestartClick()
    {
        // restart game
        GameManager.singleton.RestartGame();

        // disable game over panel
        this.gameObject.SetActive(false);
    }
}
