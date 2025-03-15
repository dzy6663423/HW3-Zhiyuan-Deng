using UnityEngine;

public class SmoothBlock : MonoBehaviour
{
    private GridObject gridObject;
    private GameManager gameManager;
    
    // Track if a sticky push has occurred this frame
    private static bool stickyPushThisFrame = false;
    
    private void Start()
    {
        gridObject = GetComponent<GridObject>();
        gameManager = FindObjectsByType<GameManager>(FindObjectsSortMode.None)[0];
    }
    
    // Reset our static flag each frame
    private void Update()
    {
        stickyPushThisFrame = false;
    }
    
    // Method called when this block is pushed
    public bool TryPush(Vector2Int direction, bool fromSticky = false)
    {
        // If this is a sticky-originated push and we've already had one this frame, block it
        if (fromSticky && stickyPushThisFrame)
            return false;
            
        Vector2Int targetPos = gridObject.gridPosition + direction;
        
        // Check if the position is valid and not occupied
        if (gameManager.IsPositionValid(targetPos) && !gameManager.IsPositionOccupied(targetPos))
        {
            // Move to the target position
            Vector2Int oldPos = gridObject.gridPosition;
            gridObject.gridPosition = targetPos;
            gameManager.UpdateGridObjectPosition(gridObject, oldPos, targetPos);
            
            // If pushed by a sticky block, set the flag to prevent chain reactions
            if (fromSticky)
                stickyPushThisFrame = true;
            
            return true; // Make sure to return true when successfully pushed
        }
        
        return false; // Return false if the push wasn't successful
    }
}
