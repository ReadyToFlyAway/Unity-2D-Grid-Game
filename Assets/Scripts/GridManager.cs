

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Color = UnityEngine.Color;

public class GridManager : MonoBehaviour
{

    private Block[,] grid;
    [Header("Grid Settings")]

    [SerializeField, Range(2, 20)]
    private int rows;
    [SerializeField, Range(4, 20)]
    private int columns;
    public float spacing = 1.1f;
    

    [Header("Block Prefab")]
    public GameObject blockPrefab;
    public TMPro.TMP_Text infoText;
    public UIManager uiManager;

    private Block firstSelectedBlock;
    private Block secondSelectedBlock;
    private List<Sprite> listSprites = new List<Sprite>();
    private List<Sprite> listOfPairSprites = new List<Sprite>();
    
    bool hasDirectPath = false;
    bool hasOneCornerPath = false;
    bool hasTwoCornerPath = false;
    private int spritePoolIndex = 0;
    public int gameLevel = 1;
    private int gameLevelRow = 2;
    private int gameLevelCol = 4;
    private GameObject[] playObjects = null;
    private GameObject newGame = null;
    private GameObject replayGame = null;
    private GameObject resetSelection = null;
    private GameObject deleteObjects = null;

    public int Rows //public accessor for rows and columns with clamping to ensure they stay within the specified range
    {
        get => rows;
        set => rows = Mathf.Clamp(value, 2, 10);
    }

    public int Columns //public accessor for rows and columns with clamping to ensure they stay within the specified range
    {
        get => columns;
        set => columns = Mathf.Clamp(value, 2, 10);
    }

    private void Start()
    {
        GenerateGrid();
    }

    private void PositionInfoText()
    {
        try
        {
            if (grid != null && grid.Length > 0 && grid[0, 0] != null)
            {
                // Get the world position of the first block in the grid
                Vector3 worldPos = grid[0, 0].transform.position;

                // move above grid
                worldPos.y += 1f;
                //worldPos.x += 2f; // move to the right to be above the control buttons

                // convert to screen position because UI elements are positioned in screen space, while the blocks are positioned in world space.
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

                // assign to UI
                infoText.rectTransform.position = screenPos;
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"PositionInfoText has detected an error! {ex}");
        }
    }

    private void OnValidate()
    {
        rows = Mathf.Clamp(rows, 2, 10); //Put limitaion to number of columns 
        columns = Mathf.Clamp(columns, 4, 10); //Put limitaion to number of rows

        if ((rows * columns) % 2 != 0) //kepping the number of blocks even, so we can always have pairs
        {
            columns++;
        }
    }

    private void Awake()
    {
        try
        {
            rows = 2; 
            columns = 4;
            gameLevel = 1;
            playObjects = GameObject.FindGameObjectsWithTag("tgBlockType");
            newGame = GameObject.FindGameObjectWithTag("tgNewGame");
            replayGame = GameObject.FindGameObjectWithTag("tgReplayGame");
            resetSelection = GameObject.FindGameObjectWithTag("tgResetSelection");
            deleteObjects = GameObject.FindGameObjectWithTag("tgDeleteObjects");
        }
        catch (Exception ex)
        {
            Debug.Log($"Awake has detected an error! {ex}");
        }
    }

    private void Update()
    {
        try
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Ignore clicks on UI completely
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                HandleClick();
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Update has detected an error! {ex}");
        }
    }
    public void RestartGame()
    {
        StartCoroutine(RestartGameRoutine());
    }

    private System.Collections.IEnumerator RestartGameRoutine()
    {
        if (grid != null) ClearGridOfBlocks(grid);

        spritePoolIndex = 0;
        grid = null;

        yield return null; // wait for the next frame to ensure all blocks are cleared before generating a new grid
        GenerateGrid();
    }

    private List<Sprite> CreateListOfSprites()
    {
        try 
        { 
            List<Sprite> blockSprites = new List<Sprite>(playObjects.Length); //for empty list use List<Sprite> listSprites = new List<Sprite>();
            foreach (GameObject obj in playObjects){
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite !=null) {
                    blockSprites.Add(sr.sprite);

                    //don't need to render these sprites in the scene, so we can disable the SpriteRenderer component to hide them
                    sr.enabled = false;
                }
            }

            return blockSprites;
        }
        
        catch (Exception ex)
        {
            Debug.Log($"CreateListOfSprites has detected an error! {ex}");
            return null;
        }
    }
    private void CreatePairSprites()
    {
        listOfPairSprites.Clear();
        spritePoolIndex = 0;
        int nrRowsForSprite = rows;
        int nrOfBlocks = (nrRowsForSprite) * columns;
        int nrOfSprites = listSprites.Count;

        if (nrOfSprites == 0) {
            Debug.LogError("No sprites found.");
            return;
        }

        if (nrOfBlocks % 2 != 0)
        {
            Debug.LogError("Number of blocks must be even.");
            return;
        }

        int totalPairs = nrOfBlocks / 2;
        int pairsPerSprite = totalPairs / nrOfSprites;
        int remainingPairs = totalPairs % nrOfSprites;

        // For each sprite, add pairsPerSprite pairs to the lsitSprites
        foreach (Sprite sprite in listSprites)
        {
            for (int i = 0; i < pairsPerSprite; i++)
            {
                listOfPairSprites.Add(sprite);
                listOfPairSprites.Add(sprite);
            }
        }

        // If there are remaining pairs, add them randomly from the list of sprites
        for (int i = 0; i < remainingPairs; i++)
        {
            Sprite extraSprite = listSprites[UnityEngine.Random.Range(0, listSprites.Count)];
            listOfPairSprites.Add(extraSprite);
            listOfPairSprites.Add(extraSprite);
        }

        // Shuffle the lsitSprites to ensure random distribution of pairs
        ShufflePairSpriteList(listOfPairSprites);
    }
    private void HandleClick()
    {
        try 
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Clicked on UI - ignore grid");
                return;
            }
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            // Raycast with zero direction to detect colliders at the clicked position
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
        
            if (hit.collider == null)
                return;

            Block clickedBlock = hit.collider.GetComponent<Block>();
            

            if (clickedBlock == null)
                return;

            if (clickedBlock.blockType.name == "PlayNewGame_0") {
                RestartGame();
                return;
            }
            if (clickedBlock.blockType.name == "ReplayTheSameAgain_0") {
                ResetGrid();
                return;
            }

            // Reset the selection
            if (clickedBlock.blockType.name == "ResetSelection_0")
            {
                if (firstSelectedBlock != null || secondSelectedBlock != null)
                {
                    ResetSelectedBlocks();
                }
                return;
            }

            // delete is pressed without 1,2 selections, return
            if ((firstSelectedBlock == null || secondSelectedBlock == null) && clickedBlock.blockType.name == "DeleteObjects_0")
            {
                return;
            }

            // If no block selected yet
            if (firstSelectedBlock == null) {
            firstSelectedBlock = clickedBlock;
            firstSelectedBlock.Select();
            return;
            }

            // If clicking same block → deselect
            if (clickedBlock == firstSelectedBlock){
                firstSelectedBlock.Deselect();
                firstSelectedBlock = null;
                return;
            }

            //  check if second block is selected, if not select it and check for match
            if (secondSelectedBlock == null){
                secondSelectedBlock = clickedBlock;
                secondSelectedBlock.Select();
            }

            // If delete button is clicked, check if the selected blocks can be matched and if so, destroy them
            if (firstSelectedBlock != null && secondSelectedBlock != null)
            {
                CheckMatch();
                if (hasDirectPath || hasOneCornerPath || hasTwoCornerPath)
                {
                    if (clickedBlock.blockType.name == "DeleteObjects_0")
                    {
                        DestroyMatchBlocks(firstSelectedBlock, secondSelectedBlock);
                        ResetSelectedBlocks();
                        if (IsGameOver())
                        {
                            // Handle game over logic here
                            string msg = $"Congratulations! You've matched all the blocks! Level {gameLevel} completed.";
                            infoText.text = msg;
                            AnimateText();

                            //Move to the next level, if current level is less than 10, otherwise reset to level 1
                            if (gameLevel >= 10) gameLevel = 0;

                            uiManager.SetPlayLevel(gameLevel);
                            gameLevel++;
                            NextGameLevel(gameLevel);
                        }
                    }
                }
                else ResetSelectedBlocks();

                return;
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"HandleClick has detected an error! {ex}");
        }
    }

    public void AnimateText()
    {
        StartCoroutine(ScaleText());
    }

    // text will scale up from its original size to 1.5 times its size over the course of 1 second, creating a simple animation effect when the game is won.
    private System.Collections.IEnumerator ScaleText()
    {
        Vector3 start = Vector3.one;
        Vector3 target = Vector3.one * 1.5f;

        float time = 0;

        while (time < 1f){
            infoText.transform.localScale = Vector3.Lerp(start, target, time);
            time += Time.deltaTime;
            yield return null; // wait for the next frame
        }
        infoText.transform.localScale = start; // Reset the scale to the original size
    }

    private bool IsGameOver()
    {
        try
        {
            foreach (Block block in grid)
            {

                if (block != null && !block.isRemoved && block.rowNumber > 0)
                    return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.Log("IsGameOver has detected an error!" + ex);
            return false;
        }
    }

    public void NextGameLevel(int gameLevel)
    {
        spritePoolIndex = 0;
        switch (gameLevel)
        {
            case 1:
                gameLevelRow = 2;
                gameLevelCol = 4;
                break;
            case 2:
                gameLevelRow = 3;
                gameLevelCol = 4;
                break;
            case 3:
                gameLevelRow = 4;
                gameLevelCol = 4;
                break;
            case 4:
                gameLevelRow = 4;
                gameLevelCol = 5;
                break;
            case 5:
                gameLevelRow = 6;
                gameLevelCol = 4;
                break;
            case 6:
                gameLevelRow = 5;
                gameLevelCol = 6;
                break;
            case 7:
                gameLevelRow = 8;
                gameLevelCol = 5;
                break;
            case 8:
                gameLevelRow = 6;
                gameLevelCol = 8;
                break;
            case 9:
                gameLevelRow = 7;
                gameLevelCol = 6;
                break;
            case 10:
                gameLevelRow = 8;
                gameLevelCol = 8;
                break;
            default:
                gameLevelRow = 2;
                gameLevelCol = 4;
                break;
        }
        StartCoroutine(RestartGameRoutine());
    }

    private void CheckMatch()
    {
        try 
        { 
            if (firstSelectedBlock == null || secondSelectedBlock == null)
                return;

            if (firstSelectedBlock.blockType != secondSelectedBlock.blockType){
                firstSelectedBlock.Deselect(); secondSelectedBlock.Deselect(); firstSelectedBlock = null; secondSelectedBlock = null;
                return;
            }

            if (firstSelectedBlock.isRemoved || secondSelectedBlock.isRemoved) { return; }

            Sprite selSprite = firstSelectedBlock.GetComponent<SpriteRenderer>().sprite;
            if (!HasDirectPath(firstSelectedBlock, secondSelectedBlock)) {
                if (!HasOneCorner(firstSelectedBlock, secondSelectedBlock)) {
                    if (HasTwoCorner(firstSelectedBlock, secondSelectedBlock))
                        hasTwoCornerPath = true;
                    else
                        hasTwoCornerPath = false;
                }
                else hasOneCornerPath = true;
            }
            else hasDirectPath = true;


            //First mark the blocks to be destroyed in red and the matched blocks in green, so we can see the path between them
            if (hasTwoCornerPath || hasOneCornerPath || hasDirectPath)
                MarkDestroyMatchBlocks(firstSelectedBlock, secondSelectedBlock);
        }
        catch (Exception ex)
        {
            Debug.Log($"CheckMatch has detected an error! {ex}");
        }
    }

    private void InitGrid()
    {
        try
        {
            rows = gameLevelRow;
            columns = gameLevelCol;
            spritePoolIndex = 0;
            listSprites = CreateListOfSprites();
            CreatePairSprites();
            rows++; // Add an extra row for the control buttons at the top
            grid = new Block[rows, columns];
        }
        catch (Exception ex)
        {
            Debug.Log($"InitGrid has detected an error! {ex}");
        }
    }

    // Create Grid
    private void GenerateGrid()
    {
        try
        {
            InitGrid();
            float totalWidth = (columns - 1) * spacing;
            float totalHeight = (rows - 1) * spacing;

            // center the grid around the origin for camera view
            float startX = -totalWidth / 2f;
            float startY = totalHeight / 2f;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {

                    Vector2 position = new Vector2(startX + col * spacing, startY - row * spacing);

                    /*====================================================================================
                     Instantiate the block prefab at the calculated position
                     Instantiate parameters:
                          - blockPrefab => what to create
                          - position => where to place it
                          - Quaternion.identity => no rotation
                          - transform => who is the parent (so we can keep the hierarchy organized)
                     =====================================================================================*/
                    GameObject newPrefebBlock = Instantiate(blockPrefab, position, Quaternion.identity, transform);
                    
                    //if (row == 0 && col > 3)
                    //    continue;
                   
                    if (row == 0)
                    {
                        // first row to put reset buttons
                        newPrefebBlock.name = "ResetPlayGame";
                        Block newBlockAtachedToResetPrefeb = newPrefebBlock.GetComponent<Block>();
                        GameObject controlButton = null;
                       
                        if (col == 0) controlButton = newGame;
                        if (col == 1) controlButton = replayGame;
                        if (col == 2) controlButton = resetSelection;
                        if (col == 3) controlButton = deleteObjects;
                        
                        if (controlButton != null)
                        {
                            SpriteRenderer rs = controlButton.GetComponent<SpriteRenderer>();
                            Sprite resetSprite = rs.sprite;
                            newBlockAtachedToResetPrefeb.Initialize(row, col, resetSprite, false);
                            grid[row, col] = newBlockAtachedToResetPrefeb;
                            rs.enabled = false;
                        }

                        if (col > 2) break; // skip the rest of the columns in the first row

                    }

                    else{
                        newPrefebBlock.name = "Block_" + row + "_" + col;

                        Block newBlock = newPrefebBlock.GetComponent<Block>();

                        if (listSprites != null && listSprites.Count > 0 && newBlock != null){
                            Sprite randomSprite = RandomSpriteToAssign(); // Get the next sprite from the prepared level one sprite lsitSprites
                            if (randomSprite == null) {
                                Debug.LogError($"No sprite available for block [{row},{col}]");
                                Destroy(newPrefebBlock);
                                continue;// Do not create a block here, move to next column”
                            }

                            // call initialize method of the new block, passing the current row, column, and the randomly generated color. 
                            newBlock.Initialize(row, col, randomSprite, false); //create one block at 2Dvector given
                            grid[row, col] = newBlock; // Assing this new block to position indicated by row and column 
                        }
                    }
                }
            }
            PositionInfoText();
        }
        catch (Exception ex)
        {
            Debug.LogError("GenerateGrid error: " + ex);
        }
    }

    private void ShufflePairSpriteList(List<Sprite> lsitSprites)
    {
        for (int i = 0; i < lsitSprites.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, lsitSprites.Count);

            Sprite temp = lsitSprites[i];

            lsitSprites[i] = lsitSprites[randomIndex];
            lsitSprites[randomIndex] = temp;
        }
    }

    private Sprite RandomSpriteToAssign()
    {
        if (spritePoolIndex >= listOfPairSprites.Count)
        {
            Debug.LogError("No more sprites left.");
            return null;
        }

        Sprite spriteToAssign = listOfPairSprites[spritePoolIndex];
        spritePoolIndex++;
        return spriteToAssign;
    }
 
    private bool HasDirectPath(Block blockA, Block blockB)
    {
        try
        {
            if (blockA == null || blockB == null) return false;
            if (blockA == blockB) return false;
            if (blockA.isRemoved || blockB.isRemoved) return false;
            if (blockA.blockType != blockB.blockType) return false;

            if (blockA.rowNumber == blockB.rowNumber)
                return HasHorizontalPath(blockA, blockB);

            if (blockA.columnNumber == blockB.columnNumber)
                return HasVerticalPath(blockA, blockB);

            return false;
        }
        
        catch (Exception ex)
        {
            Debug.Log($"HasDirectPath has detected an error! {ex}");
            return false;
        }
    }

    private bool HasVerticalPath(Block blockA, Block blockB)
    {
        try
        {
            int col = blockA.columnNumber;
            int minRow = Mathf.Min(blockA.rowNumber, blockB.rowNumber);
            int maxRow = Mathf.Max(blockA.rowNumber, blockB.rowNumber);

            for (int row = minRow + 1; row < maxRow; row++)
            {
                Block betweenBlock = grid[row, col];
                if (betweenBlock != null && !betweenBlock.isRemoved)
                    return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.Log($"HasVerticalPath has detected an error! {ex}");
            return false;
        } 
    }
    
    private bool HasHorizontalPath(Block a, Block b)
    {
        if (a.rowNumber != b.rowNumber) return false;

        int row = a.rowNumber;
        int minCol = Mathf.Min(a.columnNumber, b.columnNumber);
        int maxCol = Mathf.Max(a.columnNumber, b.columnNumber);

        for (int col = minCol + 1; col < maxCol; col++)
        {
            if (!IsCellPassable(row, col, a, b))
                return false;
        }

        return true;
    }

    private void DestroyMatchBlocks(Block blockA, Block blockB)
    {
        try
        {
            grid[blockA.rowNumber, blockA.columnNumber].isRemoved = true;
            grid[blockB.rowNumber, blockB.columnNumber].isRemoved = true;

            grid[blockA.rowNumber, blockA.columnNumber].GetComponent<SpriteRenderer>().enabled = false;
            grid[blockB.rowNumber, blockB.columnNumber].GetComponent<SpriteRenderer>().enabled = false;
        }
        catch (Exception ex)
        {
            Debug.Log($"DestroyMatchBlocks has detected an error! {ex}");
        }
    }

    private void MarkDestroyMatchBlocks(Block blockA, Block blockB)
    {
        try
        {
            grid[blockA.rowNumber, blockA.columnNumber].GetComponent<SpriteRenderer>().color = Color.red;
            grid[blockB.rowNumber, blockB.columnNumber].GetComponent<SpriteRenderer>().color = Color.red;
        }
        catch (Exception ex)
        {
            Debug.Log($"MarkDestroyMatchBlocks has detected an error! {ex}");
        }
    }

    private bool HasOneCorner(Block blockA, Block blockB)
    {
        try
        {
            if (blockA == null || blockB == null) return false;
            if (blockA.rowNumber == blockB.rowNumber || blockA.columnNumber == blockB.columnNumber)
                return false;

            int corner1Row = blockA.rowNumber;
            int corner1Col = blockB.columnNumber;

            int corner2Row = blockB.rowNumber;
            int corner2Col = blockA.columnNumber;

            bool pathUsingCorner1 =
                IsCellPassable(corner1Row, corner1Col, blockA, blockB) &&
                IsClearLine(blockA.rowNumber, blockA.columnNumber, corner1Row, corner1Col, blockA, blockB) &&
                IsClearLine(corner1Row, corner1Col, blockB.rowNumber, blockB.columnNumber, blockA, blockB);

            bool pathUsingCorner2 =
                IsCellPassable(corner2Row, corner2Col, blockA, blockB) &&
                IsClearLine(blockA.rowNumber, blockA.columnNumber, corner2Row, corner2Col, blockA, blockB) &&
                IsClearLine(corner2Row, corner2Col, blockB.rowNumber, blockB.columnNumber, blockA, blockB);

            return pathUsingCorner1 || pathUsingCorner2;
        }
        catch (Exception ex)
        {
            Debug.Log($"HasOneCorner has detected an error! {ex}");
            return false;
        }
    }

    // This method checks if the path between two blocks is clear (i.e., no other blocks are in the way) for either a horizontal or vertical line.
    // It takes the starting and ending coordinates, as well as the start and end blocks to ensure they are not considered as obstacles.
    private bool IsClearLine(int row1, int col1, int row2, int col2, Block start, Block end)
    {
        // Horizontal
        if (row1 == row2)
        {
            int minCol = Mathf.Min(col1, col2);
            int maxCol = Mathf.Max(col1, col2);

            for (int col = minCol + 1; col < maxCol; col++)
            {
                if (!IsCellPassable(row1, col, start, end))
                    return false;
            }

            return true;
        }

        // Vertical
        if (col1 == col2)
        {
            int minRow = Mathf.Min(row1, row2);
            int maxRow = Mathf.Max(row1, row2);

            for (int row = minRow + 1; row < maxRow; row++)
            {
                if (!IsCellPassable(row, col1, start, end))
                    return false;
            }

            return true;
        }

        return false;
    }

    // This method checks if a cell is passable, meaning it is either empty, the start block, or the end block.
    private bool IsCellPassable(int row, int col, Block start, Block end)
    {
        try { 
            Block cell = grid[row, col];
            if (cell == null) return true;

            if (cell == start || cell == end) return true;

            return cell.isRemoved;
        }
        catch (Exception ex)
        {
            Debug.Log($"IsCellPassable has detected an error! {ex}");
            return false;
        }
    }
    private bool HasTwoCorner(Block blockA, Block blockB)
    {
        try
        {
            if (blockA == null || blockB == null) return false;

            // Try all rows
            for (int row = 0; row < rows; row++)
            {
                // Check both pivot points are passable
                if (!IsCellPassable(row, blockA.columnNumber, blockA, blockB))
                    continue;

                if (!IsCellPassable(row, blockB.columnNumber, blockA, blockB))
                    continue;

                bool path =
                    IsClearLine(blockA.rowNumber, blockA.columnNumber, row, blockA.columnNumber, blockA, blockB) &&
                    IsClearLine(row, blockA.columnNumber, row, blockB.columnNumber, blockA, blockB) &&
                    IsClearLine(row, blockB.columnNumber, blockB.rowNumber, blockB.columnNumber, blockA, blockB);

                if (path)
                    return true;
            }

            // Try all columns
            for (int col = 0; col < columns; col++)
            {
                if (!IsCellPassable(blockA.rowNumber, col, blockA, blockB))
                    continue;

                if (!IsCellPassable(blockB.rowNumber, col, blockA, blockB))
                    continue;

                bool path =
                    IsClearLine(blockA.rowNumber, blockA.columnNumber, blockA.rowNumber, col, blockA, blockB) &&
                    IsClearLine(blockA.rowNumber, col, blockB.rowNumber, col, blockA, blockB) &&
                    IsClearLine(blockB.rowNumber, col, blockB.rowNumber, blockB.columnNumber, blockA, blockB);

                if (path)
                    return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.Log($"HasTwoCorner has detected an error! {ex}");
            return false;
        }
    }

    void ClearGridOfBlocks(Block[,] grid, bool isBetweenGrid =false)
    {
        try
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    if (grid[row, col] != null)
                    {
                        if (!isBetweenGrid) Destroy(grid[row, col].gameObject);// remove object from scene
                        grid[row, col] = null; // remove reference from array
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"ClearGridOfBlocks has detected an error! {ex}");
        }
    }

    private void ResetSelectedBlocks()
    {
        try
        {
            if (firstSelectedBlock != null) {firstSelectedBlock.Deselect(); firstSelectedBlock.GetComponent<SpriteRenderer>().color = Color.white;}
            if (secondSelectedBlock != null) {secondSelectedBlock.Deselect(); secondSelectedBlock.GetComponent<SpriteRenderer>().color = Color.white;}

            firstSelectedBlock = null;
            secondSelectedBlock = null;
            hasDirectPath = false;
            hasOneCornerPath = false;
            hasTwoCornerPath = false;
        }
        catch (Exception ex)
        {
            Debug.Log($"ResetSelectedBlocks has detected an error! {ex}");
        }
    }

    private void ResetGrid()
    {
        try
        {
            foreach (Block block in grid)
            {
                if (block != null)
                {
                    block.isRemoved = false;
                    block.GetComponent<SpriteRenderer>().sprite = block.blockType;
                    block.GetComponent<SpriteRenderer>().color = Color.white;
                    block.GetComponent<SpriteRenderer>().enabled = true;
                    block.isSelected = false;

                }
            }
            ResetSelectedBlocks();
            infoText.text = "";
        }
        catch (Exception ex)
        {
            Debug.Log($"ResetGrid has detected an error! {ex}");
        }
    }


}