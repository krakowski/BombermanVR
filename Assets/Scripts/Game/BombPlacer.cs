using UnityEngine;
using UnityEngine.Networking;

/// <summary>
///     The BombPlacer class handles placing bombs during gameplay.
///     Properties like the time to wait until another bomb may be placed can
///     be adjusted here.
/// </summary>
public class BombPlacer : NetworkBehaviour {

    //================================================================================
    // Prefab components (Inspector)
    //================================================================================

    [Header("Prefabs")]
    [Tooltip("Bomb prefab")]
    public Transform bombPrefab;

    [Header("Properties")]
    [Tooltip("Bombs won't spawn inside those masks colliders")]
    public LayerMask selectionMask;

    [Tooltip("Minimum distance at which the Player is able to interact with tiles")]
    public float interactionDistance = 2.0f;

    [Range(1, 5)]
    [Tooltip("Time to wait before next bomb can be placed")]
    public float waitTime = 3.0f;

    //================================================================================
    // Private properties
    //================================================================================

    // Player camera (for raycasting)
    private Camera cam;

    // Tile center offset
    private Vector3 offset = new Vector3(0.5f, 0, 0.5f);

    // Plane below gamefield (for raycasting)
    private Plane p;

    // Ray (for raycasting)
    private Ray ray;

    // Time at which the last bomb was placed
    private float lastBombTime;

    //================================================================================
    // Server commands
    //================================================================================

    [Command]
    void CmdPlaceBomb(Vector3 position) {
            // Instantiate on Server from Pool
            GameObject bomb = NetworkPoolManager.instance.Instantiate(bombPrefab.gameObject, position + new Vector3(0, 0.5f, 0));
            // Instantiate on Clients
            NetworkServer.Spawn(bomb, bombPrefab.GetComponent<NetworkIdentity>().assetId);
    }

    //================================================================================
    // Start/Update
    //================================================================================

    void Start() {
        cam = transform.GetChild(0).gameObject.GetComponent<Camera>();
        lastBombTime = -3f;
        // Create Plane for raycasting
        p = new Plane(Vector3.up, Vector3.zero);
    }

    void Update() {

        if (!isLocalPlayer)
            return;

        // Place Bomb on Fire1
        if (Input.GetButton("Fire1")) {
            if (Time.time - lastBombTime < waitTime)
                return;

            // Cast ray from camera center in look direction
            ray = new Ray(cam.transform.position, cam.transform.forward);
            float rayDistance;
            if (p.Raycast(ray, out rayDistance)) {
                // Determine tile center position
                Vector3 hit = ray.GetPoint(rayDistance) + offset;
                hit.x = Mathf.Floor(hit.x);
                hit.z = Mathf.Floor(hit.z);
                // Check if player is able to reach selected tile
                if (Vector3.Distance(ray.origin, hit) <= interactionDistance) {
                    // Check if bomb can be placed at this position
                    if (Physics.OverlapBox(hit, Vector3.one * 0.4f, Quaternion.identity, selectionMask).Length == 0) {
                        // Place bomb
                        CmdPlaceBomb(hit);
                        lastBombTime = Time.time;
                    }
                }
            }
        }
    }

}
