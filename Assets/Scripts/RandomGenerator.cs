using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class BSPNode
{
    public RectInt Area;
    public BSPNode Left;
    public BSPNode Right;
    public RectInt? Room;

    public BSPNode(RectInt area) { this.Area = area; }

    public bool isLeaf => Left == null || Right == null;
}

public class RandomGenerator
{
    private static HashSet<Vector2Int> map;

    public static HashSet<Vector2Int> GenerateRandomMap(Vector2Int mapSize, int minRoomSize, int maxRoomSize)
    {
        map = new HashSet<Vector2Int>();
        BSPNode root = new BSPNode(new RectInt(0,0, mapSize.x, mapSize.y));
        List<RectInt> rooms = new List<RectInt>();
        SplitSpace(root, minRoomSize);

        CreateRoom(root, rooms, minRoomSize, maxRoomSize);

        ConnectRooms(rooms);

        return map;
    }

    private static void SplitSpace(BSPNode node, int minRoomSize)
    {
        if (node.Area.width < minRoomSize * 2 && node.Area.height < minRoomSize * 2) return;

        if (node.Area.width < node.Area.height)
        {
            int splitY = Random.Range(minRoomSize, node.Area.height - minRoomSize);
            node.Left = new BSPNode(new RectInt(node.Area.x, node.Area.y, node.Area.width, splitY));
            node.Right = new BSPNode(new RectInt(node.Area.x, node.Area.y + splitY, node.Area.width, node.Area.height - splitY));
        } else
        {
            int splitX = Random.Range(minRoomSize, node.Area.height - minRoomSize);
            node.Left = new BSPNode(new RectInt(node.Area.x, node.Area.y, splitX, node.Area.height));
            node.Right = new BSPNode(new RectInt(node.Area.x + splitX, node.Area.y, node.Area.width - splitX, node.Area.height));
        }

        SplitSpace(node.Left, minRoomSize);
        SplitSpace(node.Right, minRoomSize);
    }

    private static void CreateRoom(BSPNode node, List<RectInt> roomList, int minRoomSize, int maxRoomSize)
    {
        if (node.isLeaf)
        {
            int roomWidth = Random.Range(minRoomSize, Mathf.Min(node.Area.width, maxRoomSize));
            int roomHeight = Random.Range(minRoomSize, Mathf.Min(node.Area.height, maxRoomSize));

            int roomX = Random.Range(node.Area.x, node.Area.xMax - roomWidth);
            int roomY = Random.Range(node.Area.y, node.Area.yMax - roomHeight);

            RectInt room = new RectInt(roomX, roomY, roomWidth - 5, roomHeight - 5);
            node.Room = room;
            roomList.Add(room);

            for (int x = room.xMin; x <= room.xMax; x++)
                for(int y = room.yMin; y <= room.yMax; y++)
                {
                    map.Add(new Vector2Int(x, y));
                }
        } else
        {
            if (node.Left != null) CreateRoom(node.Left, roomList, minRoomSize, maxRoomSize);
            if (node.Right != null) CreateRoom(node.Right, roomList, minRoomSize, maxRoomSize);
        }
    }

    private static void ConnectRooms(List<RectInt> rooms)
    {
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            CreateCorridoor(rooms[i], rooms[i+1]);
        }
    }

    private static void CreateCorridoor(RectInt roomA, RectInt roomB)
    {
        Vector2Int centerA = new Vector2Int(roomA.x + roomA.width / 2, roomA.y + roomA.height / 2);
        Vector2Int centerB = new Vector2Int(roomB.x + roomB.width / 2, roomB.y + roomB.height / 2);

        Vector2Int current = centerA;

        // TODO: Add vice versa for more corridoor shapes
        while (current.x != centerB.x)
        {
            map.Add(current);
            current.x += current.x < centerB.x ? 1 : -1;
        }

        while (current.y != centerB.y)
        {
            map.Add(current);
            current.y += current.y < centerB.y ? 1 : -1;
        }

        map.Add(centerB);
    }

}
