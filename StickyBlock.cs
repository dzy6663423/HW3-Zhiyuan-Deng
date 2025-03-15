using UnityEngine;
using System.Collections.Generic;

public class StickyBlock : MonoBehaviour
{
    private GridObject gridObject;
    private GameManager gameManager;
    private static bool isAnyBlockMovingThisFrame = false;
    private bool isBeingMoved = false; // Flag to prevent recursive infinite loops

    private void Start()
    {
        gridObject = GetComponent<GridObject>();
        gameManager = FindObjectsByType<GameManager>(FindObjectsSortMode.None)[0];
    }
    
    private void Update()
    {
        isAnyBlockMovingThisFrame = false;
    }

    // Method called when an adjacent block moves
    public bool TryMoveWithAdjacent(Vector2Int direction)
    {
        // Prevent recursive calls if we're already being moved
        if (isBeingMoved || isAnyBlockMovingThisFrame)
            return false;

        isAnyBlockMovingThisFrame = true;
        
        Vector2Int targetPos = gridObject.gridPosition + direction;
        
        // Check if the position is valid
        if (gameManager.IsPositionValid(targetPos))
        {
            // If position is not occupied, move there
            if (!gameManager.IsPositionOccupied(targetPos))
            {
                // Set flag to prevent recursive movements
                isBeingMoved = true;
                
                // Move the sticky block
                Vector2Int oldPos = gridObject.gridPosition;
                gridObject.gridPosition = targetPos;
                Debug.Log($"Moving StickyBlock from {oldPos} to {targetPos}");
                gameManager.UpdateGridObjectPosition(gridObject, oldPos, targetPos);
                
                // Reset flag
                isBeingMoved = false;
                return true;
            }
            else
            {
                // If there's a smooth block, try to push it just one space
                GridObject blockObj = gameManager.GetObjectAtPosition(targetPos);
                SmoothBlock smoothBlock = blockObj?.GetComponent<SmoothBlock>();
                
                if (smoothBlock != null)
                {
                    // Try to push the smooth block in the same direction
                    bool smoothPushed = smoothBlock.TryPush(direction, true); // Pass true for fromSticky
                    
                    if (smoothPushed)
                    {
                        // Set flag to prevent recursive movements
                        isBeingMoved = true;
                        
                        // Move the sticky block
                        Vector2Int oldPos = gridObject.gridPosition;
                        gridObject.gridPosition = targetPos;
                        Debug.Log($"Moving StickyBlock from {oldPos} to {targetPos} after pushing smooth block");
                        gameManager.UpdateGridObjectPosition(gridObject, oldPos, targetPos);
                        
                        // Reset flag
                        isBeingMoved = false;
                        return true; // Return true to indicate successful movement
                    }
                    
                    return false; // Return false if smooth block couldn't be pushed
                }
            }
        }
        
        isAnyBlockMovingThisFrame = false;
        return false;
    }

    // Add this method to check if the sticky block is stuck to something
    public bool IsStuckToSomething()
    {
        // We need to get the player position to determine push direction
        GridObject playerObj = FindObjectsByType<Playercontrol>(FindObjectsSortMode.None)[0].GetComponent<GridObject>();
        
        // Calculate direction from sticky to player
        Vector2Int dirToPlayer = new Vector2Int(
            Mathf.Clamp(playerObj.gridPosition.x - gridObject.gridPosition.x, -1, 1),
            Mathf.Clamp(playerObj.gridPosition.y - gridObject.gridPosition.y, -1, 1)
        );
        
        // Calculate the direction the block would move (opposite of player direction)
        Vector2Int moveDirection = -dirToPlayer;
        
        // We only care about checking if there's something blocking in the direction of movement
        Vector2Int targetPos = gridObject.gridPosition + moveDirection;
        GridObject targetObj = gameManager.GetObjectAtPosition(targetPos);
        
        // If there's an object in the target position
        if (targetObj != null)
        {
            // If it's a wall or clingy block, we can't push
            if (targetObj.GetComponent<WallBlock>() != null || 
                targetObj.GetComponent<ClingyBlock>() != null)
            {
                return true; // Found a wall or clingy block in our path, so we're stuck
            }
            
            // If it's a smooth block, check if it can be pushed
            SmoothBlock smoothBlock = targetObj.GetComponent<SmoothBlock>();
            if (smoothBlock != null)
            {
                // If the smooth block can't be pushed, then we're stuck
                return !smoothBlock.TryPush(moveDirection);
            }
        }
        
        return false; // Nothing blocking our path
    }
}