using UnityEngine;

/// <summary>
/// Represents a single logical tile in the warehouse grid.
/// </summary>
[System.Serializable]
public class GridCell
{
    public int X;
    public int Y;
    public Vector3 WorldPosition;
    public bool IsOccupied;
    public GameObject OccupiedObject;

    public GridCell(int x, int y, Vector3 worldPosition)
    {
        X = x;
        Y = y;
        WorldPosition = worldPosition;
        IsOccupied = false;
        OccupiedObject = null;
    }
}
