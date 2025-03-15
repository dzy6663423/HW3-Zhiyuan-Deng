using UnityEngine;

public class ClingyBlock : MonoBehaviour
{
    private GridObject gridObject;
    private GameManager gameManager;

    private void Start()
    {
        gridObject = GetComponent<GridObject>();
        gameManager = FindObjectsByType<GameManager>(FindObjectsSortMode.None)[0];
    }

    // Method called when trying to pull this block
    public bool TryPull(Vector2Int direction)
    {
        Vector2Int targetPos = gridObject.gridPosition + direction;
        
        // Check if the position is valid and not occupied
        if (gameManager.IsPositionValid(targetPos) && !gameManager.IsPositionOccupied(targetPos))
        {
            // Move the clingy block
            Vector2Int oldPos = gridObject.gridPosition;
            gridObject.gridPosition = targetPos;
            gameManager.UpdateGridObjectPosition(gridObject, oldPos, targetPos);
            return true;
        }
        
        return false;
    }
}