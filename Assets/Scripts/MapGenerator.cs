using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;

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
    [SerializeField] private GameObject[] enemyPrefab;
    [SerializeField] private GameObject collectablePrefab;
    [SerializeField] private GameObject harmfulObjectPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject extractionPrefab;
    [SerializeField] private Transform floorParent;
    [SerializeField] private GameObject player;

    [Header("Training Settings")]
    [SerializeField] private bool train;
    [SerializeField] private List<Texture2D> roomImages;

    private TrainingScript trainer;
    private List<Tile> newTiles;
    private NavMeshSurface navMeshSurface;

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
        //DontDestroyOnLoad(this.gameObject);

        trainer = GameObject.Find("GameManager").GetComponent<TrainingScript>();
        navMeshSurface = GetComponent<NavMeshSurface>();
    }

    private void Start()
    {
        newTiles = trainer.ExtractTiles(roomImages[5]);
        GenerateAndInstantiateMap();

        if(navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh baked for the randomly generated map!");
        }
    }

    private void GenerateAndInstantiateMap()
    {
        // Get floor positions from the algorithm
        //HashSet<Vector2Int> floorPositions = RandomWalk();
        KeyValuePair<HashSet<Vector2Int>, Dictionary<Vector2Int, string>> completeMap = RandomGenerator.GenerateRandomMap(mapSize, minRoomSize, maxRoomSize, newTiles);
        HashSet<Vector2Int> floorPositions = completeMap.Key;
        Dictionary<Vector2Int, string> positionValues = completeMap.Value;

        foreach (var position in floorPositions)
        {
            Vector3 worldPos = new Vector3(position.x, 0f, position.y); // Using X and Z for 2.5D layout
            if (positionValues.ContainsKey(position))
            {
                switch (positionValues[position])
                {
                    case "PS":
                        Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent);
                        GameObject character = Instantiate(player, worldPos + new Vector3(0f, 0.2f, 0f), Quaternion.identity);
                        GameObject.Find("CameraRig").GetComponent<CameraFollow>().SetTarget(character.transform);
                        GameManager.Instance.currentPlayer = character;
                        break;
                    case "ES":
                        Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent);
                        int randomEnemy = Random.Range(0f, 1f) <= 0.65f ? 0 : 1;
                        Instantiate(enemyPrefab[randomEnemy], worldPos + new Vector3(0f, 0.2f, 0f), Quaternion.identity);
                        break;
                    case "C":
                        Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent);
                        break;
                    case "P":
                        if (Random.Range(0.0f, 1f) <= 0.05f)
                        {
                            Instantiate(collectablePrefab, worldPos + new Vector3(0, 0.4f, 0), Quaternion.Euler(0, 0, 90));
                        }
                        Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent);
                        break;
                    //case "I":
                    //    Debug.Log("Item found");
                    //    Instantiate(collectablePrefab, worldPos + new Vector3(0, 0.4f, 0), Quaternion.identity);
                    //    Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent);
                    //    break;
                    case "H":
                        //Instantiate(floorPrefab, worldPos - new Vector3(0, 1f, 0), Quaternion.identity, floorParent);
                        if (Random.Range(0.0f, 1f) >= 0.50f)
                        {
                            Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent);
                        }
                        break;
                    case "E":
                        if (Random.Range(0.0f, 1f) <= 0.80f)
                        {
                            Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent);
                        } else
                        {
                            Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent);
                            Instantiate(wallPrefab, worldPos + new Vector3(0, 0.6f, 0), Quaternion.identity, floorParent);
                        }
                        break;
                    case "W":
                        Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent);
                        Instantiate(wallPrefab, worldPos + new Vector3(0, 0.6f, 0), Quaternion.identity, floorParent);
                        break;
                    case "R":
                        if (Random.Range(0.0f, 1f) <= 0.40f)
                        {
                            Instantiate(harmfulObjectPrefab, worldPos + new Vector3(0, 1.5f, 0), Quaternion.Euler(90, 0, 0), floorParent);
                        }
                        Instantiate(floorPrefab, worldPos, Quaternion.identity, floorParent);
                        break;

                    case "G":
                        Instantiate(extractionPrefab, worldPos + new Vector3(0, 0.6f, 0), Quaternion.identity);
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

        // Create kill zone plane below map
        GameObject killZone = new GameObject("KillZone");
        BoxCollider killCollider = killZone.AddComponent<BoxCollider>();
        killCollider.isTrigger = true;

        // Position and scale based on map size
        float killZoneHeight = -5f;
        Vector3 killCenter = new Vector3(mapSize.x / 2f, killZoneHeight, mapSize.y / 2f);
        Vector3 killSize = new Vector3(mapSize.x * 2f, 1f, mapSize.y * 2f);

        killCollider.center = Vector3.zero;
        killCollider.size = killSize;

        killZone.transform.position = killCenter;
        killZone.layer = LayerMask.NameToLayer("Ignore Raycast");
        killZone.tag = "DeadZone";

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

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        // Optional: clean up generated content
        if (navMeshSurface != null)
        {
            Destroy(navMeshSurface.navMeshData); // Destroy the generated NavMesh data
            navMeshSurface.RemoveData(); // Clear baked NavMesh from scene
            navMeshSurface = null;
            Debug.Log("NavMeshSurface destroyed.");
        }

        newTiles = null;
        trainer = null;

        Debug.Log("MapGenerator destroyed.");
    }

}
