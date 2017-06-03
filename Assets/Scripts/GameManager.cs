using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region INITIAL_DATA

    public static GameManager singleton;
    GameSaver gameSaver;

    [Header("Settings")]
    [SerializeField]
    List<Color> availableColors;
    [SerializeField]
    float oneOrFourCubedBlockChance = 0.33f;
    [SerializeField]
    int minNumberOfSameColoredCells = 3;
    [SerializeField]
    int numberOfColumnsInGrid = 5;
    [SerializeField]
    int numberOfPointsPerCell = 10;
    [Header("UI References")]
    [SerializeField]
    Transform gridTr;
    [SerializeField]
    Text bestScoreValueText;
    [SerializeField]
    Text currentScoreText;
    [SerializeField]
    Transform currentBlockSequenceAreaTr;
    [SerializeField]
    GameObject cubePrefab;
    [SerializeField]
    GameObject hintPrefab;
    [SerializeField]
    Vector2 offsetForHint;
    [SerializeField]
    GameOver gameOverPanel;

    [Header("Cache")]
    [HideInInspector]
    public List<Cube> currentCubeSequence = new List<Cube>(4);
    [HideInInspector]
    public List<Cube> nextCubeSequence = new List<Cube>(4);

    List<GridCell> gridCells;
    List<GridCell> currentlyActiveCells = new List<GridCell>(4);

    GameObject activeHint = null;
    [HideInInspector]
    public bool hammerIsActive = false;

    #endregion

    private void Awake()
    {
        singleton = this;
        gameSaver = this.GetComponent<GameSaver>();
    }

    #region LOADING_SAVING_GAME

    private void OnEnable()
    {
        // save all cells
        gridCells = new List<GridCell>(gridTr.childCount);
        gridCells.AddRange(gridTr.GetComponentsInChildren<GridCell>());

        // load game
        PlayerData playerData = gameSaver.LoadGame();

        // load best score if we have save
        if (playerData != null)
            bestScoreValueText.text = playerData.bestScore;
        else
            bestScoreValueText.text = "0";

        // if there were a fixed cells on grid then continue game
        // otherwise start new game
        if (playerData != null && playerData.indexesOfFixedCells.Count > 0)
            ContinueGame(playerData);
        else
            StartNewGame();
    }

    private void OnApplicationFocus(bool focus)
    {
        // save game when focus is lost
        if (!focus)
            gameSaver.SaveGame(UpdatePlayerData());
    }

    private void OnApplicationQuit()
    {
        // save data on quit as well
        gameSaver.SaveGame(UpdatePlayerData());
    }

    PlayerData UpdatePlayerData()
    {
        // save current and best scores, current and next lengths
        // of cubes sequences
        PlayerData playerData = new PlayerData()
        {
            bestScore = bestScoreValueText.text,
            currentScore = currentScoreText.text,
            currentCubeSequenceLength = currentCubeSequence.Count,
            nextCubeSequenceLength = nextCubeSequence.Count
        };

        Color cubeColor = new Color();
        // save colors for current sequence
        for (int i = 0; i < currentCubeSequence.Count; i++)
        {
            cubeColor = currentCubeSequence[i].GetColor();
            playerData.currentCubeSequenceColors.Add("#" + ColorUtility.ToHtmlStringRGBA(cubeColor));
        }

        // save colors for next sequence
        for (int i = 0; i < nextCubeSequence.Count; i++)
        {
            cubeColor = nextCubeSequence[i].GetColor();
            playerData.nextCubeSequenceColors.Add("#" + ColorUtility.ToHtmlStringRGBA(cubeColor));
        }

        // save grid
        for (int i = 0; i < gridCells.Count; i++)
        {
            if (gridCells[i].isFixed)
            {
                playerData.indexesOfFixedCells.Add(i);
                cubeColor = gridCells[i].GetColor();
                playerData.gridCellsColors.Add("#" + ColorUtility.ToHtmlStringRGBA(cubeColor));
            }
        }

        return playerData;
    }

    #endregion

    #region GAME_MECHANICS

    void ContinueGame(PlayerData playerData)
    {
        // load current score
        currentScoreText.text = playerData.currentScore;

        // load current cube sequence
        for (int j = 0; j < playerData.currentCubeSequenceLength; j++)
        {
            GameObject cube = Instantiate(cubePrefab);
            cube.transform.SetParent(currentBlockSequenceAreaTr, false);
            Cube cube_scr = cube.GetComponent<Cube>();

            string cubeColorHex = playerData.currentCubeSequenceColors[j];
            Color cubeColor;
            if (ColorUtility.TryParseHtmlString(cubeColorHex, out cubeColor))
                cube_scr.SetColor(cubeColor);

            if (j == 0)
                cube_scr.SetAsFirst();

            currentCubeSequence.Add(cube_scr);
        }

        // load next cube sequence
        for (int j = 0; j < playerData.nextCubeSequenceLength; j++)
        {
            GameObject cube = Instantiate(cubePrefab);
            cube.transform.SetParent(currentBlockSequenceAreaTr, false);
            Cube cube_scr = cube.GetComponent<Cube>();

            string cubeColorHex = playerData.nextCubeSequenceColors[j];
            Color cubeColor;
            if (ColorUtility.TryParseHtmlString(cubeColorHex, out cubeColor))
                cube_scr.SetColor(cubeColor);

            if (j == 0)
                cube_scr.SetAsFirst();

            cube.SetActive(false);
            nextCubeSequence.Add(cube_scr);
        }

        // load grid cells
        for (int i = 0; i < gridCells.Count; i++)
        {
            if (playerData.indexesOfFixedCells.Contains(i))
            {
                // get the first color
                string cellColorHex = playerData.gridCellsColors[0];
                Color cubeColor;
                if (ColorUtility.TryParseHtmlString(cellColorHex, out cubeColor))
                    gridCells[i].SetColor(cubeColor);

                // fix this cell
                gridCells[i].isFixed = true;
                // remove used color
                playerData.gridCellsColors.RemoveAt(0);
            }
        }

        // in case the game was shutted down during game over
        CheckIfCurrentSequenceCanBePlaced();
    }

    void StartNewGame()
    {
        // reset current score
        currentScoreText.text = "0";

        // generate 2 sequences on start, then only 1 will be generated at the same time
        GenerateBlocksSequence(2);
    }

    public void RestartGame()
    {
        // clear all generated sequences
        currentCubeSequence.Clear();
        nextCubeSequence.Clear();

        // reset current score
        currentScoreText.text = "0";

        // reset all cells
        for (int i = 0; i < gridCells.Count; i++)
        {
            gridCells[i].ResetCell();
        }

        // generate 2 sequences on start, then only 1 will be generated at the same time
        GenerateBlocksSequence(2);
    }

    public void GenerateBlocksSequence(int numberOfSequences = 1)
    {
        // destroy hint if it was active
        DestroyHint();

        // destroy previous sequence if it's not the first one
        if (currentBlockSequenceAreaTr.childCount > 0)
            DestroyPreviousSequence();

        // clear current sequence if it exists
        if (currentCubeSequence.Count > 0)
            currentCubeSequence.Clear();

        // if we have next sequence then save it to the current and clear next one
        if (nextCubeSequence.Count != 0)
        {
            // show current sequence
            for (int i = 0; i < nextCubeSequence.Count; i++)
            {
                currentCubeSequence.Add(nextCubeSequence[i]);
                currentCubeSequence[i].gameObject.SetActive(true);
            }
            nextCubeSequence.Clear();
        }

        for (int i = 0; i < numberOfSequences; i++)
        {
            int numberOfCubes = GenerateCubesAmount();

            for (int j = 0; j < numberOfCubes; j++)
            {
                GameObject cube = Instantiate(cubePrefab);
                cube.transform.SetParent(currentBlockSequenceAreaTr, false);
                Cube cube_scr = cube.GetComponent<Cube>();
                cube_scr.SetColor(GetNewColor());

                if (j == 0)
                    cube_scr.SetAsFirst();

                // if this is the first sequence and there will be more then 1 sequence
                // then save new sequence as current
                if (numberOfSequences != 1 && i == 0)
                    currentCubeSequence.Add(cube_scr);
                else
                    // else save new sequence as next
                    nextCubeSequence.Add(cube_scr);
            }
        }

        // hide next sequence
        for (int i = 0; i < nextCubeSequence.Count; i++)
        {
            nextCubeSequence[i].gameObject.SetActive(false);
        }

        CheckIfCurrentSequenceCanBePlaced();
    }

    public void ResetActiveGridCells()
    {
        for (int i = 0; i < gridCells.Count; i++)
        {
            // if active but not fixed
            if (gridCells[i].isActive && !gridCells[i].isFixed)
                gridCells[i].ResetCell();
        }

        ClearActiveCells();
    }

    public void FixActiveGridCells()
    {
        // end of current move

        for (int i = 0; i < gridCells.Count; i++)
        {
            // if active but not fixed
            if (gridCells[i].isActive && !gridCells[i].isFixed)
            {
                gridCells[i].isActive = false;
                gridCells[i].isFixed = true;
            }
        }

        ClearActiveCells();
        CheckIfCellsCanBeDestroyed();
    }

    public void AddToActiveCells(GridCell cell)
    {
        currentlyActiveCells.Add(cell);
    }

    void ClearActiveCells()
    {
        currentlyActiveCells.Clear();
    }

    void DestroyPreviousSequence()
    {
        for (int i = 0; i < currentBlockSequenceAreaTr.childCount; i++)
        {
            Transform child = currentBlockSequenceAreaTr.GetChild(i);
            Cube cube = child.GetComponent<Cube>();

            // destroy only if it's not the cube from the next sequence
            if (!nextCubeSequence.Contains(cube))
                Destroy(cube.gameObject);
        }
    }

    void CheckIfCellsCanBeDestroyed()
    {
        // get all fixed cells
        List<GridCell> fixedCells = new List<GridCell>(GetNumberOfFixedGridCells());
        for (int i = 0; i < gridCells.Count; i++)
        {
            // if fixed
            if (gridCells[i].isFixed)
                fixedCells.Add(gridCells[i]);
        }

        // all cells with the same color, shouldn't have more elements than number
        // of fixed cells
        List<GridCell> detectedCells = new List<GridCell>(fixedCells.Count);

        // going through all fixed cells
        for (int i = 0; i < fixedCells.Count; i++)
        {
            GridCell firstCell = fixedCells[i];

            // check if this cell is still fixed (may be not fixed already after destroying)
            if (firstCell.isFixed)
            {
                // add current cell to the list
                detectedCells.Add(firstCell);

                int startCount = detectedCells.Count;
                int endCount = 0;

                // get current color
                Color curColor = detectedCells[0].GetColor();

                // do it until we won't be able to find cells with the same color
                // and near already found cells, i.t. until start count of found cells
                // become equal to end count of found cells
                while (startCount != endCount)
                {
                    // update start count of detected cells
                    startCount = detectedCells.Count;

                    // go again through all fixed cells
                    for (int j = 0; j < fixedCells.Count; j++)
                    {
                        GridCell nextFixedCell = fixedCells[j];

                        // if we have no such a cell in list but it has the same color
                        if (!detectedCells.Contains(nextFixedCell) && SameColors(nextFixedCell.GetColor(), curColor))
                        {
                            // go through all cells which we have found earlier
                            for (int k = 0; k < detectedCells.Count; k++)
                            {
                                GridCell foundCell = detectedCells[k];
                                // here we should check if the next cell is close to
                                // the one of the cells which we have found earlier
                                if (TwoCellsAreNear(foundCell, nextFixedCell))
                                {
                                    detectedCells.Add(nextFixedCell);
                                    break;
                                }
                            }
                        }
                    }
                    // update end count of found cells
                    endCount = detectedCells.Count;
                }

                // if we have found more than 3 cells with the same color
                // and which are near each other, then destroy them and
                // update earned points
                if (endCount >= minNumberOfSameColoredCells)
                {
                    // reset all detected cells
                    for (int k = 0; k < detectedCells.Count; k++)
                    {
                        GridCell cell = detectedCells[k];
                        cell.ResetCell();
                    }

                    // and add points
                    AddPoints(detectedCells.Count);
                }

                // clear list of detected cells
                detectedCells.Clear();
            }
        }
    }

    void AddPoints(int numberOfDestroyedCells)
    {
        int currentNumberOfPoints = int.Parse(currentScoreText.text);
        currentScoreText.text = (currentNumberOfPoints + numberOfDestroyedCells * numberOfPointsPerCell).ToString();
    }

    void CheckIfCurrentSequenceCanBePlaced()
    {
        // get length of current sequence
        int currentSequenceLength = currentCubeSequence.Count;

        // get all non-fixed cells, i.e. empty
        List<GridCell> nonFixedCells = new List<GridCell>(25);
        for (int i = 0; i < gridCells.Count; i++)
        {
            // if not fixed
            if (!gridCells[i].isFixed)
                nonFixedCells.Add(gridCells[i]);
        }

        List<GridCell> emptyCells = new List<GridCell>(nonFixedCells.Count);

        // going through all non-fixed cells
        for (int i = 0; i < nonFixedCells.Count; i++)
        {
            GridCell firstCell = nonFixedCells[i];

            // if current sequence is just a single cube then we are done here
            // as there is at least one non-fixed cell
            if (currentCubeSequence.Count == 1)
                return;

            // add current cell to the list
            emptyCells.Add(firstCell);

            int startCount = emptyCells.Count;
            int endCount = 0;

            // do it until we won't be able to find empty cells near already found cells, 
            // i.e. until start count of found cells
            // become equal to end count of found cells
            while (startCount != endCount)
            {
                // update start count of detected cells
                startCount = emptyCells.Count;

                // go again through all non-fixed cells
                for (int j = 0; j < nonFixedCells.Count; j++)
                {
                    GridCell nextEmptyCell = nonFixedCells[j];

                    // if we have no such a cell in list
                    if (!emptyCells.Contains(nextEmptyCell))
                    {
                        // go through all empty cells which we have found earlier
                        for (int k = 0; k < emptyCells.Count; k++)
                        {
                            GridCell emptyCell = emptyCells[k];

                            // here we should check if the next cell is close to
                            // the one of the empty cells which we have found earlier
                            if (TwoCellsAreNear(emptyCell, nextEmptyCell))
                            {
                                emptyCells.Add(nextEmptyCell);

                                if (emptyCells.Count >= currentSequenceLength && CheckIfCellsFormContinuousLine(emptyCells))
                                    return;

                                break;
                            }
                        }
                    }
                }

                // update end count of found empty cells
                endCount = emptyCells.Count;

                // if number of empty cells already more or equals
                // to the length of the current sequence and these cells form a line
                // so that we can put all sequence to the grid then we are done here
                if (endCount >= currentSequenceLength && CheckIfCellsFormContinuousLine(emptyCells))
                    return;
            }

            // clear all empty cells which have been found on the previous step
            emptyCells.Clear();
        }

        GameOver();
    }

    void GameOver()
    {
        int curScore = int.Parse(currentScoreText.text);
        int bestScore = int.Parse(bestScoreValueText.text);

        // turn on game over panel
        gameOverPanel.gameObject.SetActive(true);
        gameOverPanel.UpdateGameOverInfo(curScore, bestScore);

        // update best score if we earn more points then before
        if (curScore > bestScore)
            bestScoreValueText.text = currentScoreText.text;
    }

    #endregion

    #region POWER_UPS

    public void ShowHint(Transform hintBtnTr)
    {
        // if previous hint is active then destroy it
        DestroyHint();

        GameObject hint = Instantiate(hintPrefab);
        hint.transform.SetParent(hintBtnTr, false);
        hint.transform.localPosition = offsetForHint;
        hint.GetComponent<Hint>().ShowNextCubedBlock(nextCubeSequence);
        activeHint = hint;
    }

    public bool DestroyHint()
    {
        if (activeHint)
        {
            Destroy(activeHint);
            return true;
        }
        return false;
    }

    public void SwitchHammerForFixedCells()
    {
        // if hammer is already active then switch off it
        if (hammerIsActive)
        {
            HideHammerForCellsUnderIt();
            return;
        }

        hammerIsActive = true;
        for (int i = 0; i < gridCells.Count; i++)
        {
            // if fixed
            if (gridCells[i].isFixed)
                gridCells[i].ShowHammer();
        }
    }

    public void HideHammerForCellsUnderIt()
    {
        hammerIsActive = false;
        for (int i = 0; i < gridCells.Count; i++)
        {
            // if under the hammer
            if (gridCells[i].underTheHammer)
                gridCells[i].HideHammer();
        }
    }

    #endregion

    #region UTILITY_FUNCTIONS

    int GenerateCubesAmount()
    {
        int numberOfCubes = 0;

        if (Random.value <= oneOrFourCubedBlockChance)
        {
            if (Random.value < 0.5f)
                numberOfCubes = 1;
            else
                numberOfCubes = 4;
        }
        else
        {
            if (Random.value < 0.5f)
                numberOfCubes = 2;
            else
                numberOfCubes = 3;
        }

        return numberOfCubes;
    }

    Color GetNewColor()
    {
        return availableColors[Random.Range(0, availableColors.Count)];
    }

    public int GetNumberOfActiveGridCells()
    {
        int numberOfActiveCells = 0;
        for (int i = 0; i < gridCells.Count; i++)
        {
            // if active but not fixed
            if (gridCells[i].isActive && !gridCells[i].isFixed)
                numberOfActiveCells++;
        }
        return numberOfActiveCells;
    }

    public int GetNumberOfFixedGridCells()
    {
        int numberOfFixedCells = 0;
        for (int i = 0; i < gridCells.Count; i++)
        {
            // if fixed
            if (gridCells[i].isFixed)
                numberOfFixedCells++;
        }
        return numberOfFixedCells;
    }

    public bool CanBeActivated(GridCell nextCell)
    {
        GridCell prevCell = currentlyActiveCells[currentlyActiveCells.Count - 1];

        // if cells are near each other horizontally or vertically then all good
        // next cell can be placed there
        if (TwoCellsAreNear(prevCell, nextCell))
            return true;

        return false;
    }

    bool SameColors(Color first, Color second)
    {
        // much reliable than directly comparing color as (first == second)
        // because of float numbers in r,g,b,a
        return ColorUtility.ToHtmlStringRGBA(first) == ColorUtility.ToHtmlStringRGBA(second);
    }

    bool CheckIfCellsFormContinuousLine(List<GridCell> cells)
    {
        // for sequences with length less than 4
        // no such a cases, it always be a continuous line
        if (currentCubeSequence.Count < 4)
            return true;

        int numberOfCellsInLine = 1;

        // we must ensure that every next cell is near
        // to the previous one, because there can be situation
        // when we have 4 empty cells and a cube sequence with length = 4,
        // but we won't be able to put this sequence to the grid, 
        // e.g. $ - empty cell
        //     $
        //     $$
        //     $
        for (int i = 0; i < cells.Count; i++)
        {
            GridCell cell = cells[i];

            for (int j = (i + 1); j < cells.Count; j++)
            {
                // if we have already checked needed amount of cells
                // then all good (no need to continue check)
                if (numberOfCellsInLine >= currentCubeSequence.Count)
                    return true;

                GridCell nextCell = cells[j];

                if (TwoCellsAreNear(cell, nextCell))
                {
                    numberOfCellsInLine++;
                    break;
                }
            }
        }

        // if we have needed amount of cells
        // then all good, otherwise there is no place anymore
        if (numberOfCellsInLine >= currentCubeSequence.Count)
            return true;
        else
            return false;
    }

    bool TwoCellsAreNear(GridCell first, GridCell second)
    {
        int indexOfFirstCell = gridCells.IndexOf(first);
        int indexOfSecondCell = gridCells.IndexOf(second);
        int indexDif = Mathf.Abs(indexOfFirstCell - indexOfSecondCell);

        // check indexes and rows
        if ((indexDif == 1 && TwoCellsInTheSameRow(first, second)) ||
            indexDif == numberOfColumnsInGrid)
            return true;

        return false;
    }

    bool TwoCellsInTheSameRow(GridCell first, GridCell second)
    {
        int indexOfFirstCell = gridCells.IndexOf(first);
        int indexOfSecondCell = gridCells.IndexOf(second);

        // for example:
        // we have 5 columns, first cell at index №4 (the last cell of the first row)
        // second cell at index №5 (the first cell of the second row)
        // as a result 4/5 = 0 and 5/5 = 1, 0 != 1, so these two cells in the different rows
        if (indexOfFirstCell / numberOfColumnsInGrid == indexOfSecondCell / numberOfColumnsInGrid)
            return true;

        return false;
    }

    #endregion

    public void OnQuitButtonClick()
    {
        Application.Quit();
    }
}
