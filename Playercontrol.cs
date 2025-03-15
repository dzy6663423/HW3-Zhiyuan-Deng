using UnityEngine;

public class Playercontrol : MonoBehaviour
{
    private GridObject gridObject;
    private GameManager gameManager;

    private void Start()
    {
        gridObject = GetComponent<GridObject>();
        gameManager = FindObjectsByType<GameManager>(FindObjectsSortMode.None)[0];
    }

    private void Update()
    {
        Vector2Int movement = Vector2Int.zero;

        // Get input
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            movement = new Vector2Int(0, -1);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            movement = new Vector2Int(0, 1);
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            movement = new Vector2Int(-1, 0);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            movement = new Vector2Int(1, 0);
        
        // Try to move if we have input
        if (movement != Vector2Int.zero)
        {
            Vector2Int newPos = gridObject.gridPosition + movement;
            
            // Check if the position is valid
            if (gameManager.IsPositionValid(newPos))
            {
                // Check if position is occupied
                if (!gameManager.IsPositionOccupied(newPos))
                {
                    
                    Vector2Int oldPos = gridObject.gridPosition;
                    gridObject.gridPosition = newPos;
                    gameManager.UpdateGridObjectPosition(gridObject, oldPos, newPos);
                }
                else
                {
                    // Try to push a block if there is one
                    GridObject blockObj = gameManager.GetObjectAtPosition(newPos);
                    
                    // Check if it's a sticky block - can't push these if they're stuck to something
                    StickyBlock stickyBlock = blockObj.GetComponent<StickyBlock>();
                    if (stickyBlock != null)
                    {
                       
                        if (stickyBlock.IsStuckToSomething())
                        {
                            
                            return;
                        }
                        
                        // Try to push the non-stuck sticky block
                        bool pushed = stickyBlock.TryMoveWithAdjacent(movement);
                        
                        // If we successfully pushed the block, move the player and exit
                        if (pushed)
                        {
                            Vector2Int oldPos = gridObject.gridPosition;
                            gridObject.gridPosition = newPos;
                            gameManager.UpdateGridObjectPosition(gridObject, oldPos, newPos);
                        }
                        
                        // Whether pushed or not, we're done processing this movement
                        return;
                    }
                    
                    // Check if it's a smooth block
                    SmoothBlock smoothBlock = blockObj.GetComponent<SmoothBlock>();
                    if (smoothBlock != null)
                    {
                        // Try to push the smooth block
                        bool pushed = smoothBlock.TryPush(movement);
                        
                        // If we successfully pushed the block, move the player
                        if (pushed)
                        {
                            Vector2Int oldPos = gridObject.gridPosition;
                            gridObject.gridPosition = newPos;
                            gameManager.UpdateGridObjectPosition(gridObject, oldPos, newPos);
                        }
                    }
                    
                    // Check if it's a clingy block - can't push these
                    ClingyBlock clingyBlock = blockObj.GetComponent<ClingyBlock>();
                    if (clingyBlock != null)
                    {
                        // Can't push clingy blocks, do nothing
                    }
                }
            }
        }
    }
}