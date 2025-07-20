using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
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
    [SerializeField] private GameObject collectablePrefab;
    [SerializeField] private GameObject harmfulObjectPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private Transform floorParent;
    [SerializeField] private GameObject player;

    [Header("Training Settings")]
    [SerializeField] private bool train;
    [SerializeField] private List<Texture2D> roomImages;

    private TrainingScript trainer;
    private List<Tile> newTiles;

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

        trainer = GameObject.Find("GameManager").GetComponent<TrainingScript>();
    }

    private void Start()
    {
        newTiles = trainer.ExtractTiles(roomImages[5]);
        GenerateAndInstantiateMap();
    }

    private List<Tile> LoadRules(string path)
    {
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        List<Tile> tiles = JsonUtility.FromJson<List<Tile>>(json);
        return tiles;
    }

    private void SaveRules(List<Tile> tiles, string path)
    {
        string json = JsonUtility.ToJson(tiles, true);
        File.WriteAllText(path, json);
    }

    private void GenerateAndInstantiateMap()
    {
        // Get floor positions from the algorithm
        //HashSet<Vector2Int> floorPositions = RandomWalk();
        KeyValuePair<HashSet<Vector2Int>, Dictionary<Vector2Int, string>> completeMap = RandomGenerator.GenerateRandomMap(mapSize, minRoomSize, maxRoomSize, newTiles);
        HashSet<Vector2Int> floorPositions = completeMap.Key;
        Dictionary<Vector2Int, string> positionValues = completeMap.Value;
        HashSet<Vector2Int> spawnPositions = new HashSet<Vector2Int>();

        foreach (var position in floorPositions)
        {
            Vector3 worldPos = new Vector3(position.x, 0f, position.y); // Using X and Z for 2.5D layout
            if (positionValues.ContainsKey(position))
            {
                switch (positionValues[position])
                {
                    case "P":
                        Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent);
                        spawnPositions.Add(position);
                        break;
                    case "I":
                        Instantiate(collectablePrefab, worldPos + new Vector3(0, 0.4f, 0), Quaternion.identity, floorParent);
                        Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent);
                        break;
                    case "H":
                        //Instantiate(floorPrefab, worldPos - new Vector3(0, 1f, 0), Quaternion.identity, floorParent);
                        break;
                    case "E":
                        Instantiate(wallPrefab, worldPos + new Vector3(0, 1, 0), Quaternion.identity, floorParent);
                        break;
                    case "R":
                        Instantiate(harmfulObjectPrefab, worldPos + new Vector3(0, 0.6f, 0), Quaternion.identity, floorParent);
                        Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent);
                        break;

                    default: break;
                }
            }
            else
            {
                Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent);
            }
        }

        // TODO: Dont spawn the player anywhere. spawn him in the 1st room
        int choice = Random.Range(0, spawnPositions.Count);
        Vector3 playerPosition = new Vector3(spawnPositions.ElementAt(choice).x, 1f, spawnPositions.ElementAt(choice).y);

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
