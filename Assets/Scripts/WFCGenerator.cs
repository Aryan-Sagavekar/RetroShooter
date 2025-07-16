using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using System.Linq;

public enum Direction { North, South, East, West }

[Serializable]
public class WFCTile
{
    public string name;
    public GameObject prefab;

    public string northEdge;
    public string southEdge;
    public string eastEdge;
    public string westEdge;

    public Dictionary<Direction, string> GetEdges()
    {
        return new Dictionary<Direction, string>
        {
            { Direction.North, northEdge },
            { Direction.South, southEdge },
            { Direction.East, eastEdge },
            { Direction.West, westEdge }
        };
    }
    public WFCTile Rotate90()
    {
        return new WFCTile
        {
            name = name + "_R",
            prefab = prefab,
            northEdge = westEdge,
            eastEdge = northEdge,
            southEdge = eastEdge,
            westEdge = southEdge
        };
    }
}

public class Cell
{
    public List<WFCTile> possibleTiles;
    public bool Collapsed => possibleTiles.Count == 1;

    public Cell(List<WFCTile> allTiles)
    {
        possibleTiles = new List<WFCTile>(allTiles);
    }
}

public class WFCGenerator
{
    private int width, height;
    private Cell[,] cells;
    private List<WFCTile> tileSet;

    public WFCGenerator(int width, int height, Cell[,] cells, List<WFCTile> tiles)
    {
        this.width = width;
        this.height = height;
        this.tileSet = tiles;

        cells = new Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
                cells[x, y] = new Cell(tileSet);
        }
    }

    public void Generate(Vector2Int offset, Transform parent)
    {
        while(true)
        {
            Vector2Int? pos = Collapse();
            if (pos == null) break;
            Propogate(pos.Value);
        }

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                WFCTile tile = cells[x, y].possibleTiles[0];
                Vector3 worldPos = new Vector3(offset.x + x, 0f, offset.y + y);
                GameObject.Instantiate(tile.prefab, worldPos, Quaternion.identity, parent);
            }
    }

    public void Generate(Texture2D tileset, int tileSize, GameObject basePrefab, Vector2Int offset, Transform parent)
    {
        var sliced = SliceTileset(tileset, tileSize);
        var tileSet = BuildTileSetFromTextures(sliced, basePrefab);
        this.tileSet = tileSet;

        cells = new Cell[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                cells[x, y] = new Cell(tileSet);

        Generate(offset, parent);
    }

    private List<Texture2D> SliceTileset(Texture2D tileset, int tileSize)
    {
        List<Texture2D> tiles = new List<Texture2D>();
        int tilesX = tileset.width / tileSize;
        int tilesY = tileset.height / tileSize;

        for (int y = 0; y < tilesY; y++)
            for (int x = 0; x < tilesX; x++)
            {
                Texture2D tile = new Texture2D(tileSize, tileSize);
                tile.SetPixels(tileset.GetPixels(x * tileSize, y * tileSize, tileSize, tileSize));
                tile.Apply();
                tile.name = $"tile_{x}_{y}";
                tiles.Add(tile);
            }

        return tiles;
    }

    private List<WFCTile> BuildTileSetFromTextures(List<Texture2D> tiles, GameObject basePrefab)
    {
        List<WFCTile> result = new List<WFCTile>();

        foreach (var tex in tiles)
        {
            WFCTile tile = new WFCTile
            {
                name = tex.name,
                prefab = basePrefab,
                northEdge = GetEdgeSignature(tex, Direction.North),
                southEdge = GetEdgeSignature(tex, Direction.South),
                eastEdge = GetEdgeSignature(tex, Direction.East),
                westEdge = GetEdgeSignature(tex, Direction.West)
            };

            result.Add(tile);
            result.Add(tile.Rotate90());
        }

        return result;
    }

    private string GetEdgeSignature(Texture2D tex, Direction dir)
    {
        int size = tex.width;
        Color[] edgePixels = dir switch
        {
            Direction.North => tex.GetPixels(0, size - 1, size, 1),
            Direction.South => tex.GetPixels(0, 0, size, 1),
            Direction.East => tex.GetPixels(size - 1, 0, 1, size),
            Direction.West => tex.GetPixels(0, 0, 1, size),
            _ => new Color[0]
        };

        return string.Join("|", edgePixels.Select(p => ((int)(p.r * 255)).ToString("X2") + ((int)(p.g * 255)).ToString("X2") + ((int)(p.b * 255)).ToString("X2")));
    }

    private Vector2Int? Collapse()
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        int minEntropy = int.MaxValue;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var cell = cells[x, y];
                if (cell.Collapsed) continue;

                //TODO: Instead of just taking the count we can use a heuristic function
                int entropy = cell.possibleTiles.Count;
                if (entropy < minEntropy)
                {
                    minEntropy = entropy;
                    candidates.Clear();
                }

                if (entropy == minEntropy)
                    candidates.Add(new Vector2Int(x, y));
            }

        if (candidates.Count == 0) return null;

        var chosen = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        var chosenCell = cells[chosen.x, chosen.y];
        var selectedTile = chosenCell.possibleTiles[UnityEngine.Random.Range(0, chosenCell.possibleTiles.Count)];
        chosenCell.possibleTiles = new List<WFCTile> { selectedTile };

        return chosen;
    }

    private void Propogate(Vector2Int start)
    {
        Queue<Vector2Int> toPropogate = new Queue<Vector2Int>();
        toPropogate.Enqueue(start);

        while(toPropogate.Count > 0)
        {
            Vector2Int current = toPropogate.Dequeue();
            var currentTile = cells[current.x, current.y].possibleTiles[0];

            foreach (Direction dir in Enum.GetValues(typeof(Direction))) 
            {
                Vector2Int neighbourPos = GetNeighbourPosition(current, dir);
                if (!IsInBounds(neighbourPos)) continue;

                var neighbourCell = cells[neighbourPos.x, neighbourPos.y];
                if (neighbourCell.Collapsed) continue;

                string requiredEdge = currentTile.GetEdges()[dir];
                Direction oppositeDir = Opposite(dir);

                var validTiles = neighbourCell.possibleTiles
                .Where(t => t.GetEdges()[oppositeDir] == requiredEdge)
                .ToList();

                if (validTiles.Count < neighbourCell.possibleTiles.Count)
                {
                    neighbourCell.possibleTiles = validTiles;
                    toPropogate.Enqueue(neighbourPos);
                }

                if (neighbourCell.possibleTiles.Count == 0)
                    throw new Exception($"Contradiction at {neighbourPos}");
            }
        }
    }

    private Vector2Int GetNeighbourPosition(Vector2Int pos, Direction dir)
    {
        return dir switch
        {
            Direction.North => new Vector2Int(pos.x, pos.y + 1),
            Direction.South => new Vector2Int(pos.x, pos.y - 1),
            Direction.East => new Vector2Int(pos.x + 1, pos.y),
            Direction.West => new Vector2Int(pos.x - 1, pos.y + 1),
            _ => pos,
        };
    }

    private bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x <= width && pos.y <= height;
    }

    private Direction Opposite(Direction dir)
    {
        return dir switch
        {
            Direction.North => Direction.South,
            Direction.East => Direction.West,
            Direction.South => Direction.North,
            Direction.West => Direction.East,
            _ => dir,
        };
    }
}
