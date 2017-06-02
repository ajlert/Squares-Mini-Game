using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GridCell : MonoBehaviour, IPointerEnterHandler, IPointerUpHandler, IPointerDownHandler
{
    #region INITIAL_DATA

    [SerializeField]
    Image backgroundImg;
    [SerializeField]
    Color initialColor;
    [SerializeField]
    GameObject hammer;

    /// <summary>
    /// Become active when not all sequence is placed on grid
    /// </summary>
    //[HideInInspector]
    public bool isActive = false;
    /// <summary>
    /// Become fixed when all sequence is placed on grid
    /// </summary>
    //[HideInInspector]
    public bool isFixed = false;
    /// <summary>
    /// When a Hammer power up is activated
    /// </summary>
    [HideInInspector]
    public bool underTheHammer = false;

    GameManager gameManager;

    #endregion

    // Use this for initialization
    void Start ()
    {
        gameManager = GameManager.singleton;
    }

    public void ResetCell()
    {
        isFixed = false;
        isActive = false;
        backgroundImg.color = initialColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isFixed && !gameManager.hammerIsActive)
        {
            // beginning, update color for first cube
            backgroundImg.color = gameManager.currentCubeSequence[0].GetColor();
            isActive = true;
            gameManager.AddToActiveCells(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // we shouldn't be able to put new cubes on already active or fixed cells
        if (isFixed || isActive)
            return;

        int numberOfActiveCells = gameManager.GetNumberOfActiveGridCells();
        // if number of activated cells is more than 0 (it means that we have started to put cubed block)
        // and this number is less than the length of current block and this cell can be activated 
        // (e.g. is located near the previous activated cell, but not diagonally) then update color
        if (numberOfActiveCells > 0 && numberOfActiveCells < gameManager.currentCubeSequence.Count &&
            gameManager.CanBeActivated(this))
        {
            backgroundImg.color = gameManager.currentCubeSequence[numberOfActiveCells].GetColor();
            isActive = true;
            gameManager.AddToActiveCells(this);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // we shouldn't be able to put the last cube from sequence on already fixed cell
        if (isFixed)
        {
            // if hammer is active then clear this cell (i.e. use Hammer power up) and
            // turn off hammer
            if (underTheHammer)
            {
                ResetCell();
                gameManager.HideHammerForCellsUnderIt();
            }
            return;
        }

        int numberOfActiveCells = gameManager.GetNumberOfActiveGridCells();
        if (numberOfActiveCells == gameManager.currentCubeSequence.Count)
        {
            // ending, fix active cells and generate next cubed block
            gameManager.FixActiveGridCells();
            gameManager.GenerateBlocksSequence();
        }
        else
        {
            // not all cubes from sequence were put on grid, reset all cells
            gameManager.ResetActiveGridCells();
        }
    }

    public void ShowHammer()
    {
        hammer.SetActive(true);
        underTheHammer = true;
    }

    public void HideHammer()
    {
        hammer.SetActive(false);
        underTheHammer = false;
    }

    public Color GetColor()
    {
        return backgroundImg.color;
    }

    public void SetColor(Color color)
    {
        backgroundImg.color = color;
    }
}
