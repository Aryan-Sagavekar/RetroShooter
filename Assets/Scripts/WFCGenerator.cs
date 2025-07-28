using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using System.Linq;
using Random = UnityEngine.Random;

public class Cell
{
    public List<int> options;
    public bool collapsed;
    public bool isChecked;
    public int index;

    public Cell(List<Tile> tiles, int index)
    {
        this.options = new List<int>(tiles.Count);
        for(int i=0; i < tiles.Count; i++)
        {
            this.options.Add(i);
        }

        this.collapsed = false;
        this.isChecked = false;
        this.index = index;
    }
}


public class WFCGenerator
{
    private int width, height;
    private Cell[,] grid;
    private List<Tile> tileSet;

    public WFCGenerator(int width, int height, List<Tile> tiles, int? seed)
    {
        //    if (seed.HasValue)
        //        UnityEngine.Random.InitState(seed.Value);

        this.width = width;
        this.height = height;
        this.tileSet = tiles;

        grid = new Cell[width, height];
        int count = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            { 
                grid[x, y] = new Cell(tileSet, count);  count++; 
            }
    }

    public string GetSymbolForTile(int cellIndex)
    {
        Color centerColor = tileSet[cellIndex].img.GetPixel(75, 75);

        centerColor.r = Mathf.Round(centerColor.r);
        centerColor.g = Mathf.Round(centerColor.g);
        centerColor.b = Mathf.Round(centerColor.b);

        if (centerColor == Color.black)
        {
            return "H";
        } else if (centerColor == Color.red)
        {
            return "R";
        } else if (centerColor == Color.green)
        {
            return "I";
        } else if (centerColor == Color.blue)
        {
            return "P";
        }

        return "E";
    }

    public Cell[,] Generate()
    {
        while (true)
        {
            List<Cell> gridList = new List<Cell>();
            foreach (Cell cell in grid)
            {
                if (!cell.collapsed)
                    gridList.Add(cell);
            }

            if (gridList.Count == 0)
            {
                Debug.Log("All cells collapsed. WFC complete.");
                break;
            }

            gridList.Sort((a, b) => a.options.Count.CompareTo(b.options.Count));

            int minOptions = gridList[0].options.Count;
            int stopIndex = gridList.FindIndex(cell => cell.options.Count > minOptions);
            if (stopIndex != -1)
            {
                gridList = gridList.GetRange(0, stopIndex);
            }

            Cell chosenCell = gridList[Random.Range(0, gridList.Count)];
            chosenCell.collapsed = true;
            int pick = chosenCell.options[Random.Range(0, chosenCell.options.Count)];
            chosenCell.options = new List<int> { pick };

            Debug.Log("Collapsed one cell. Remaining uncollapsed: " + gridList.Count);

            reduceEntropy(chosenCell, 0);
        }

        return grid;
    }

    public void reduceEntropy(Cell pickedCell, int depth)
    {
        if (depth > 2) return;

        if (pickedCell.isChecked) return;

        int i = (pickedCell.index % width);
        int j = (pickedCell.index / width);

        // Right
        if (i + 1 < width)
        {
            Cell rightCell = grid[i + 1, j];
            if (rightCell != null && !rightCell.collapsed)
            {
                List<int> validOptions = new List<int>();
                foreach (var option in pickedCell.options)
                {
                    validOptions.AddRange(tileSet[option].neighbors[Dir.Right]);
                }

                if (validOptions.Count > 0)
                {
                    rightCell.options.ForEach(option => validOptions.Contains(option));
                    reduceEntropy(rightCell, depth + 1);
                }
            }
        } 
        // Left
        if (i - 1 >= 0)
        {
            Cell leftCell = grid[i - 1, j];
            if (leftCell != null && !leftCell.collapsed)
            {
                List<int> validOptions = new List<int>();
                foreach (var option in pickedCell.options)
                {
                    validOptions.AddRange(tileSet[option].neighbors[Dir.Left]);
                }

                if (validOptions.Count > 0)
                {
                    leftCell.options.ForEach(option => validOptions.Contains(option));
                    reduceEntropy(leftCell, depth + 1);
                }
            }
        }
        // Up
        if (j - 1 >= 0)
        {
            Cell upCell = grid[i, j - 1];
            if (upCell != null && !upCell.collapsed)
            {
                List<int> validOptions = new List<int>();
                foreach (var option in pickedCell.options)
                {
                    validOptions.AddRange(tileSet[option].neighbors[Dir.Up]);
                }

                if (validOptions.Count > 0)
                {
                    upCell.options.ForEach(option => validOptions.Contains(option));
                    reduceEntropy(upCell, depth + 1);
                }
            }
        }
        // Down
        if (j + 1 < height)
        {
            Cell downCell = grid[i, j + 1];
            if (downCell != null && !downCell.collapsed)
            {
                List<int> validOptions = new List<int>();
                foreach (var option in pickedCell.options)
                {
                    validOptions.AddRange(tileSet[option].neighbors[Dir.Down]);
                }

                if (validOptions.Count > 0)
                {
                    downCell.options.ForEach(option => validOptions.Contains(option));
                    reduceEntropy(downCell, depth + 1);
                }
            }
        }
    }
}
