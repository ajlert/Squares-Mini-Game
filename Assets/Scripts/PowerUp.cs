using UnityEngine;
using UnityEngine.EventSystems;

public class PowerUp : MonoBehaviour, IPointerClickHandler
{
    #region INITIAL_DATA

    public enum PowerUpType
    {
        HAMMER,
        HINT,
        SKIP
    }

    [SerializeField]
    PowerUpType powerUpType;

    GameManager gameManager;

    #endregion

    // Use this for initialization
    void Start ()
    {
        gameManager = GameManager.singleton;
	}

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (powerUpType)
        {
            case PowerUpType.HAMMER:
                {
                    if (gameManager.GetNumberOfFixedGridCells() > 0)
                        gameManager.SwitchHammerForFixedCells();
                }
                break;
            case PowerUpType.HINT:
                {
                    if (!gameManager.DestroyHint())
                        gameManager.ShowHint(this.transform);
                }
                break;
            case PowerUpType.SKIP:
                {
                    gameManager.GenerateBlocksSequence();
                }
                break;
            default:
                break;
        }
    }
}
