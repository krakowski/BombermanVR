using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

/// <summary>
///     The MapManager class generates a map using a text file and places crates randomly based on a seed
///     specified by the server.
/// </summary>
public class MapManager : NetworkBehaviour {

    //================================================================================
    // Prefab components (Inspector)
    //================================================================================

    [Header("Prefabs")]
    [Tooltip("Tile that should be used inside the game")]
    public Transform tilePrefab;
    [Tooltip("Wall that should be used inside the game")]
    public Transform wallPrefab;
    [Tooltip("Crate that should be used inside the game")]
    public Transform cratePrefab;
    [Tooltip("ItemSpawner that should be used inside the game")]
    public Transform itemSpawnerPrefab;
    [Tooltip("PlayerStart that should be used inside the game")]
    public Transform playerStartPrefab;
    [Tooltip("Border that should be used inside the game")]
    public Transform borderPrefab;
    [Tooltip("Black Plane under the map for filling out gaps between tiles")]
    public Transform groundOutlinePrefab;
    [Space(10)]

    [Header("Assets")]
    [Tooltip("Map Asset (text file)")]
    public TextAsset mapAsset;
    [Space(10)]

    //================================================================================
    // Component Properties
    //================================================================================

    [Header("Map Properties")]
    [Range(0, 1)]
    [Tooltip("Space between each tile")]
    public float outlinePercent = 0.05f;

    //================================================================================
    // Network properties
    //================================================================================

    [SyncVar(hook = "OnCrateCountChanged")]
    [Tooltip("The amount of crates")]
    public int crateCount = 16;

    [SyncVar(hook = "OnRandomSeedChanged")]
    [Tooltip("Random seed for map creation")]
    public int randomSeed = 42;

    //================================================================================
    // Private Properties
    //================================================================================

    private const char CRATE = 'C';
    private const char WALL = 'W';
    private const char ITEM = 'I';
    private const char BORDER = 'B';
    private const char EMPTY = 'E';
    private const char PLAYERSTART = 'P';
    private const char RESERVERD = 'R';

    // 2D representation of the map
    private char[,] map;

    // 2D size of the map
    private Vector2 mapSize;

    // List containing empty positions for crates
    private List<Vector3> emptyPositionsList;

    //================================================================================
    // SyncVar Hooks
    //================================================================================

    /// <summary>
    ///     Called whenever crateCount value changes on the Server.
    /// </summary>
    /// <param name="value">New crateCount value</param>
    public void OnCrateCountChanged(int value) {
        crateCount = value;
        createMap();
    }

    /// <summary>
    ///     Called whenever randomSeed value changes on the Server.
    /// </summary>
    /// <param name="value">New randomSeed value</param>
    public void OnRandomSeedChanged(int value) {
        randomSeed = value;
        createMap();
    }


    /// <summary>
    ///     Maps map position to world position.
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y"> y coordinate</param>
    /// <returns>world position</returns>
    private Vector3 coordinateToPosition(int x, int y) {
        // Map coordinate to world position
        return new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
    }

    /// <summary>
    ///     Fills the map with objects (walls, borders, tiles, itemspawners).
    /// </summary>
    private void fillMap() {
        // Find old container
        string container = "MapContainer";
        Transform containerTransform = transform.FindChild(container);

        // Destroy old container if present
        if (containerTransform) {
            DestroyImmediate(containerTransform.gameObject);
        }

        // Create new map container
        Transform mapContainer = new GameObject(container).transform;
        mapContainer.parent = transform;

        // Create object containers and set them to static
        Transform wallContainer = new GameObject("WallContainer").transform;
        wallContainer.gameObject.isStatic = true;
        Transform borderContainer = new GameObject("BorderContainer").transform;
        borderContainer.gameObject.isStatic = true;
        Transform tileContainer = new GameObject("TileContainer").transform;
        tileContainer.gameObject.isStatic = true;

        wallContainer.parent = mapContainer;
        borderContainer.parent = mapContainer;
        tileContainer.parent = mapContainer;

        // Iterate over mapState
        for (int y = 0; y < mapSize.y; y++) {
            for(int x = 0; x < mapSize.x; x++) {
                // Get world position for coordinate
                Vector3 pos = coordinateToPosition(x, y);

                // Place tiles
                Transform tile = (Transform)Instantiate(tilePrefab, pos, Quaternion.Euler(new Vector3(90, 0, 0)));
                tile.transform.localScale = Vector3.one * (1 - outlinePercent);
                tile.parent = tileContainer;

                // Place Objects
                switch (map[x, y]) {
                    case WALL:
                        Transform wall = (Transform)Instantiate(wallPrefab, pos, Quaternion.identity);
                        wall.parent = wallContainer;

                        tile.GetComponent<MapTile>().enabled = false;
                        tile.GetComponent<BoxCollider>().enabled = false;
                        break;
                    case ITEM:
                        if (!isServer)
                            break;

                        Transform itemSpawner = Instantiate(itemSpawnerPrefab, pos, Quaternion.identity);
                        itemSpawner.parent = mapContainer;
                        // Send spawn information to clients
                        NetworkServer.Spawn(itemSpawner.gameObject);

                        tile.GetComponent<MapTile>().enabled = false;
                        tile.GetComponent<BoxCollider>().enabled = false;
                        break;
                    case BORDER:
                        Transform border = (Transform)Instantiate(borderPrefab, pos, Quaternion.identity);
                        border.parent = borderContainer;

                        tile.GetComponent<MapTile>().enabled = false;
                        tile.GetComponent<BoxCollider>().enabled = false;
                        break;
                    case EMPTY:
                        emptyPositionsList.Add(coordinateToPosition(x,y));
                        break;
                    default:
                        break;
                        
                }
            }
        }

        // Enable Static Batching for walls, borders and tiles.
        StaticBatchingUtility.Combine(wallContainer.gameObject);
        StaticBatchingUtility.Combine(borderContainer.gameObject);
        StaticBatchingUtility.Combine(tileContainer.gameObject);
        
    }

    /// <summary>
    ///     Indicates if map has still empty positions left.
    /// </summary>
    /// <returns>true: empty positions left - false: no empty positions left</returns>
    public bool hasEmptyPosition() {
        return emptyPositionsList.Count != 0;
    }

    /// <summary>
    ///     Creates the map.
    /// </summary>
    public void createMap() {

        // Editor calls this method frequently.
        // Instance should be created only once.
        if(emptyPositionsList == null)
            emptyPositionsList = new List<Vector3>();

        // Delete old list entries (if present).
        emptyPositionsList.Clear();

        // Read map from TextAsset
        string[] lines = mapAsset.text.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None);

        map = new char[lines[0].Length, lines.Length];

        for(int y = 0; y < lines.Length; y++) {
            string line = lines[y];
            for(int x = 0; x < line.Length; x++) {
                map[y, x] = line[x];
            }
        }


        mapSize.x = map.GetLength(0);
        mapSize.y = map.GetLength(1);

        // Stretch black Plane under the map to match map size
        groundOutlinePrefab.localScale = new Vector3(mapSize.x, 1, mapSize.y);

        fillMap();

        // Shuffle empty positions for random crate positions
        Utils.shuffle(emptyPositionsList, randomSeed);

        placeCrates();
    }

    /// <summary>
    ///     Randomly places crates on the map.
    /// </summary>
    private void placeCrates() {
        // Find old container
        string container = "CrateContainer";
        Transform containerTransform = transform.FindChild(container);

        // Destroy old container if present
        if (containerTransform) {
            DestroyImmediate(containerTransform.gameObject);
        }

        // Create new map container
        Transform crateContainer = new GameObject(container).transform;
        crateContainer.parent = transform;

        // Place crates
        for (int i = 0; i < crateCount; i++) {
            if (!hasEmptyPosition())
                return;

            Vector3 pos = getRandomEmptyPosition();
            Transform playerStart = (Transform)Instantiate(cratePrefab, pos, Quaternion.identity);
            playerStart.parent = crateContainer;
        }

        // Enable Static Batching for crates
        StaticBatchingUtility.Combine(crateContainer.gameObject);
    }

    /// <summary>
    ///     Supplies the next random empty position and deletes
    ///     it from the list.
    /// </summary>
    /// <returns></returns>
    private Vector3 getRandomEmptyPosition() {

        // Get next random empty position
        Vector3 pos = emptyPositionsList[0];
        // Remove random Position from List
        emptyPositionsList.RemoveAt(0);

        return pos;
    }

    //================================================================================
    // Server
    //================================================================================

    public override void OnStartServer() {
        // Server sets seed and crate count
        randomSeed = Random.Range(0, 100);
        crateCount = Random.Range(10, 30);
    }

    //================================================================================
    // Unity
    //================================================================================

    void Start() {
        createMap();
    }
}
