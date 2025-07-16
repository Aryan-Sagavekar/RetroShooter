using System.Collections.Generic;
using UnityEngine;

public static class RandomGeneration
{
    public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startingPosition, int walkLength)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();

        path.Add(startingPosition);
        var previousPosition = startingPosition;

        for (int i = 0; i < walkLength; i++) 
        {
            var newPosition = previousPosition + Direction2D.GetRandomCardinalDirection();
            path.Add(newPosition);
            previousPosition = newPosition;
        }

        return path;
    }
}

public static class Direction2D
{
    public static List<Vector2Int> cardinalDirectionsList = new List<Vector2Int>()
    {
        new Vector2Int(0, 1), //Up
        new Vector2Int(1, 0), //Right
        new Vector2Int(0, -1), //Down
        new Vector2Int(-1, 0), //Left
    };

    public static Vector2Int GetRandomCardinalDirection()
    {
        return cardinalDirectionsList[Random.Range(0, cardinalDirectionsList.Count)];
    }
}
