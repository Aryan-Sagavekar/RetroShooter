using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public enum Dir
{
    Right,
    Left,
    Up,
    Down,
}

public class Tile
{
    public Texture2D img;
    public int index;
    public int tileSize;
    public Dictionary<Dir, List<int>> neighbors;

    public Tile(Texture2D img, int index, int tileSize)
    {
        this.img = img;
        this.index = index;
        this.tileSize = tileSize;
        neighbors = new Dictionary<Dir, List<int>>();

        neighbors[Dir.Right] = new List<int>();
        neighbors[Dir.Left] = new List<int>();
        neighbors[Dir.Up] = new List<int>();
        neighbors[Dir.Down] = new List<int>();
    }

    public void calculateNeighbors(List<Tile> tiles)
    {
        for (int i = 0; i < tiles.Count; i++) 
        {
            if (this.Overlapping(tiles[i], Dir.Right))
            {
                this.neighbors[Dir.Right].Add(i);
            }
            if (this.Overlapping(tiles[i], Dir.Left))
            {
                this.neighbors[Dir.Left].Add(i);
            }
            if (this.Overlapping(tiles[i], Dir.Up))
            {
                this.neighbors[Dir.Up].Add(i);
            }
            if (this.Overlapping(tiles[i], Dir.Down))
            {
                this.neighbors[Dir.Down].Add(i);
            }
        }
    }

    private bool ColorsAreSimilar(Color a, Color b, float tolerance = 0.05f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }


    public bool Overlapping(Tile otherTile, Dir direction)
    {
        if (direction == Dir.Right)
        {
            for (int i = 1; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    Color colorA = this.img.GetPixel(i * tileSize + (tileSize/2), j * tileSize + (tileSize/2));
                    Color colorB = otherTile.img.GetPixel((i - 1) * tileSize + (tileSize / 2), j * tileSize + (tileSize / 2));

                    if (!ColorsAreSimilar(colorA, colorB))
                    {
                        return false;
                    }
                }

        }
        else if (direction == Dir.Left)
        {
            for (int i = 1; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    Color colorA = this.img.GetPixel((i - 1) * tileSize + (tileSize / 2), j * tileSize + (tileSize / 2));
                    Color colorB = otherTile.img.GetPixel(i * tileSize + (tileSize / 2), j * tileSize + (tileSize / 2));

                    if (!ColorsAreSimilar(colorA, colorB))
                    {
                        return false;
                    }
                }

        } else if (direction == Dir.Up)
        {
            for (int j = 1; j < 3; j++)
                for (int i = 0; i < 3; i++)
                {
                    Color colorA = this.img.GetPixel(i * tileSize + (tileSize / 2), (j - 1) * tileSize + (tileSize / 2));
                    Color colorB = otherTile.img.GetPixel(i * tileSize + (tileSize / 2), j * tileSize + (tileSize / 2));

                    if (!ColorsAreSimilar(colorA, colorB))
                    {
                        return false;
                    }
                }

        }
        else if (direction == Dir.Down)
        {
            for (int j = 1; j < 3; j++)
                for (int i = 0; i < 3; i++)
                {
                    Color colorA = this.img.GetPixel(i * tileSize + (tileSize / 2), j * tileSize + (tileSize / 2));
                    Color colorB = otherTile.img.GetPixel(i * tileSize + (tileSize / 2), (j - 1) * tileSize + (tileSize / 2));

                    if (!ColorsAreSimilar(colorA, colorB))
                    {
                        return false;
                    }
                }

        }
        return true;
    }
}


public class TrainingScript : MonoBehaviour
{
    private List<Tile> extractedTiles;
    [SerializeField] private int tileSize = 50;
    [SerializeField] private RectTransform tilesDisplayParent;

    private float displayScale = 1f;

    //Helper function to debug the tile extraction
    private void DisplayTiles(Texture2D tileTexture)
    {
        Texture2D centerTile = new Texture2D(tileSize, tileSize);
        centerTile.SetPixels(tileTexture.GetPixels(tileSize, tileSize, tileSize, tileSize));
        centerTile.Apply();
        GameObject tileGO = new GameObject("Tile_" + extractedTiles.Count);
        tileGO.transform.SetParent(tilesDisplayParent, false); // Set parent and maintain local position/scale

        // Add a RawImage component to the new GameObject
        RawImage rawImage = tileGO.AddComponent<RawImage>();
        rawImage.texture = centerTile; // Assign the extracted tile texture
         
        // Set the size of the RawImage. Use displayScale for better visibility.
        RectTransform rt = rawImage.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(displayScale, displayScale);
    }

    public List<Tile> ExtractTiles(Texture2D roomImage)
    {
        extractedTiles = new List<Tile>();

        for (int j = 0; j <= roomImage.height - 3 * tileSize; j += tileSize)
        {
            for (int i = 0; i <= roomImage.width - 3 * tileSize; i += tileSize)
            {
                Tile newTile = new Tile(new Texture2D(3 * tileSize, 3 * tileSize), extractedTiles.Count, tileSize);
                newTile.img.SetPixels(roomImage.GetPixels(i, j, 3 * tileSize, 3 * tileSize));
                newTile.img.filterMode = FilterMode.Point;
                newTile.img.Apply();

                extractedTiles.Add(newTile);
            }
        }

        foreach (Tile tile in extractedTiles)
        {
            tile.calculateNeighbors(extractedTiles);
        }

        return extractedTiles;
        //DebugWFCGenerator(extractedTiles);
    }

    public void DebugWFCGenerator(List<Tile> tiles)
    {
        WFCGenerator wfcGenerator = new WFCGenerator(10, 10, tiles, 549234);
        Cell [,] detailedRoom = wfcGenerator.Generate();

        for (int j=0; j < 10; j++)
            for(int i = 0; i < 10; i++)
            {
                int index = detailedRoom[i, j].options[0];
                DisplayTiles(tiles[index].img);
            }
    }
}

//public class TrainingScript : MonoBehaviour
//{
//    [SerializeField] List<StatData> colorSymbolPairList;
//    private Dictionary<Color32, char> colorSymbolPair;
//    [SerializeField] private int tileSize = 50;

//    private void ConvertListToDictionary()
//    {
//        colorSymbolPair = new Dictionary<Color32, char>();
//        foreach (var element in colorSymbolPairList)
//        {
//            Color32 roundedColor = (Color32)element.color;
//            colorSymbolPair[roundedColor] = element.symbol;
//        }
//    }

//    private char[,] GetSymbolGrid(Texture2D tex)
//    {
//        ConvertListToDictionary();
//        int gridSize = tex.width / tileSize;
//        char[,] grid = new char[gridSize, gridSize];

//        for (int y = 0; y < gridSize; y++)
//        {
//            for (int x = 0; x < gridSize; x++)
//            {
//                Color32 col = GetTileColor(tex, x * tileSize, y * tileSize, tileSize);
//                if (!colorSymbolPair.ContainsKey(col))
//                {
//                    Debug.LogError($"Color {col} not mapped in colorSymbolPair");
//                    grid[x, y] = 'E'; // fallback
//                }
//                else
//                {
//                    grid[x, y] = colorSymbolPair[col];
//                }
//            }
//        }
//        return grid;
//    }

//    private Color32 GetTileColor(Texture2D tex, int startX, int startY, int tileSize)
//    {
//        return (Color32)tex.GetPixel(startX + tileSize / 2, startY + tileSize / 2);
//    }

//    private string PatchToString(char[,] patch)
//    {
//        int size = patch.GetLength(0);
//        StringBuilder sb = new StringBuilder();
//        for (int y = 0; y < size; y++)
//            for (int x = 0; x < size; x++)
//                sb.Append(patch[x, y]);
//        return sb.ToString();
//    }

//    public List<WFCTile> BuildTileSetFromRoomImages(List<Texture2D> roomImages)
//    {
//        var result = new List<WFCTile>();
//        HashSet<string> seen = new HashSet<string>();
//        int patchSize = 5;
//        int tileId = 0;

//        foreach (var room in roomImages)
//        {
//            var symbols = GetSymbolGrid(room);
//            int w = symbols.GetLength(0), h = symbols.GetLength(1);

//            for (int y = 0; y <= h - patchSize; y++)
//            {
//                for (int x = 0; x <= w - patchSize; x++)
//                {
//                    char[,] patch = new char[patchSize, patchSize];
//                    for (int dy = 0; dy < patchSize; dy++)
//                        for (int dx = 0; dx < patchSize; dx++)
//                            patch[dx, dy] = symbols[x + dx, y + dy];

//                    string id = PatchToString(patch);
//                    if (seen.Contains(id)) continue;
//                    seen.Add(id);

//                    string northEdge = new string(Enumerable.Range(0, patchSize).Select(i => patch[i, 0]).ToArray());
//                    string southEdge = new string(Enumerable.Range(0, patchSize).Select(i => patch[i, patchSize - 1]).ToArray());
//                    string eastEdge = new string(Enumerable.Range(0, patchSize).Select(i => patch[patchSize - 1, i]).ToArray());
//                    string westEdge = new string(Enumerable.Range(0, patchSize).Select(i => patch[0, i]).ToArray());

//                    var tile = new WFCTile
//                    {
//                        name = $"AutoTile_{tileId++}",  // Ensure unique names
//                        northEdge = northEdge,
//                        southEdge = southEdge,
//                        eastEdge = eastEdge,
//                        westEdge = westEdge,
//                    };

//                    result.Add(tile);
//                }
//            }
//        }

//        Debug.Log($"Generated {result.Count} unique tiles from input images.");
//        return result;
//    }
//}
