using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Dictionary<Vector2Int, GridObject> gridPositions = new Dictionary<Vector2Int, GridObject>();
    private GridMaker grid;

    private void Awake()
    {
        grid = GetComponent<GridMaker>();
    }

    private void Start()
    {
        // Register all grid objects in the scene
        // Replace deprecated FindObjectsOfType with the newer method
        GridObject[] objects = FindObjectsByType<GridObject>(FindObjectsSortMode.None);
        foreach (GridObject obj in objects)
        {
            RegisterGridObject(obj);
        }
    }

    public void RegisterGridObject(GridObject obj)
    {
        if (gridPositions.ContainsKey(obj.gridPosition))
        {
            Debug.LogWarning("Position already occupied: " + obj.gridPosition);
        }
        else
        {
            gridPositions[obj.gridPosition] = obj;
        }
    }

    public void UpdateGridObjectPosition(GridObject obj, Vector2Int oldPos, Vector2Int newPos)
    {
        if (gridPositions.ContainsKey(oldPos) && gridPositions[oldPos] == obj)
        {
            gridPositions.Remove(oldPos);
        }
        
        gridPositions[newPos] = obj;
        
        // Check for sticky blocks around the new position
        CheckForStickyBlocks(obj, oldPos, newPos);
        
        // Check for clingy blocks in the opposite direction
        CheckForClingyBlocks(obj, oldPos, newPos);
    }
    
    private void CheckForStickyBlocks(GridObject movingObj, Vector2Int oldPos, Vector2Int newPos)
    {
        // Calculate move direction correctly
        Vector2Int moveDirection = newPos - oldPos;
        
        // Skip if this is a diagonal movement
        if (Mathf.Abs(moveDirection.x) + Mathf.Abs(moveDirection.y) > 1)
            return;
        
        // For sticky blocks, make sure we check them carefully
        // Only move one sticky block per movement to prevent chain reactions
        bool hasMovedStickyBlock = false;
        
        // Check all four cardinal directions around the OLD position
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        
        foreach (Vector2Int dir in directions)
        {
            // Skip if we've already moved one sticky block
            if (hasMovedStickyBlock)
                break;
                
            Vector2Int adjacentPos = oldPos + dir;
            GridObject adjacentObj = GetObjectAtPosition(adjacentPos);
            
            if (adjacentObj != null && adjacentObj != movingObj)
            {
                // Skip if the adjacent object is a wall
                if (adjacentObj.GetComponent<WallBlock>() != null)
                    continue;
                    
                StickyBlock stickyBlock = adjacentObj.GetComponent<StickyBlock>();
                if (stickyBlock != null)
                {
                    // Try to move the sticky block in the same direction
                    bool moved = stickyBlock.TryMoveWithAdjacent(moveDirection);
                    if (moved)
                    {
                        hasMovedStickyBlock = true;
                    }
                }
            }
        }
    }
    
    private void CheckForClingyBlocks(GridObject movingObj, Vector2Int oldPos, Vector2Int newPos)
    {
        // Calculate move direction
        Vector2Int moveDirection = newPos - oldPos;
        
        // Check the opposite position for a clingy block
        Vector2Int oppositePos = oldPos - moveDirection;
        GridObject oppositeObj = GetObjectAtPosition(oppositePos);
        
        if (oppositeObj != null)
        {
            ClingyBlock clingyBlock = oppositeObj.GetComponent<ClingyBlock>();
            if (clingyBlock != null)
            {
                // Try to pull the clingy block
                clingyBlock.TryPull(moveDirection);
            }
        }
    }

    public bool IsPositionValid(Vector2Int pos)
    {
        return pos.x >= 1 && pos.x <= grid.dimensions.x && 
               pos.y >= 1 && pos.y <= grid.dimensions.y;
    }

    public bool IsPositionOccupied(Vector2Int pos)
    {
        return gridPositions.ContainsKey(pos);
    }

    public GridObject GetObjectAtPosition(Vector2Int pos)
    {
        if (gridPositions.ContainsKey(pos))
            return gridPositions[pos];
        return null;
    }
}

