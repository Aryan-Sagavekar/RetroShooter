using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MapGenerator : MonoBehaviour
{
    private static MapGenerator instance;

    [Header("Map Generation Settings(Random walk)")]
    [SerializeField] protected Vector2Int startPosition = Vector2Int.zero;
    [SerializeField] private int iterations = 10;
    [SerializeField] private int walkLength = 10;
    [SerializeField] private bool startRandomlyEachIteration = true;

    [Header("Map Generation Settings(BSP and WFC)")]
    [SerializeField] private Vector2Int mapSize = new(100, 100);
    [SerializeField] private int minRoomSize = 20;
    [SerializeField] private int maxRoomSize = 30;

    [Header("Required Objects")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private Transform floorParent;
    [SerializeField] private GameObject player;

    public static MapGenerator Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        GenerateAndInstantiateMap();
    }

    private void GenerateAndInstantiateMap()
    {
        // Get floor positions from the algorithm
        //HashSet<Vector2Int> floorPositions = RandomWalk();
        HashSet<Vector2Int> floorPositions = RandomGenerator.GenerateRandomMap(mapSize, minRoomSize, maxRoomSize);

        foreach (var position in floorPositions)
        {
            Vector3 worldPos = new Vector3(position.x, 0f, position.y); // Using X and Z for 2.5D layout

            Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent);
        }

        int choice = Random.Range(0, floorPositions.Count);
        Vector3 playerPosition = new Vector3(floorPositions.ElementAt(choice).x, 1f, floorPositions.ElementAt(choice).y);

        GameObject character = Instantiate(player, playerPosition, Quaternion.identity);

        GameObject.Find("CameraRig").GetComponent<CameraFollow>().SetTarget(character.transform);
    }

    protected HashSet<Vector2Int> RandomWalk()
    {
        var currentPosition = startPosition;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < iterations; i++)
        {
            var path = RandomGeneration.SimpleRandomWalk(currentPosition, walkLength);

            floorPositions.UnionWith(path);
            if (startRandomlyEachIteration)
                currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
        }

        return floorPositions;
    }
}
