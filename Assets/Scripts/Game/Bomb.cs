using UnityEngine;
using UnityEngine.Networking;
using System;

/// <summary>
///     A bomb which can be placed on the map by a player. Properties like the amount
///     of damage to be inflicted on the player can be adjusted here.
/// </summary>
public class Bomb : NetworkBehaviour, Explodable {

    //================================================================================
    // Prefab components (Inspector)
    //================================================================================
    
    [Header("Prefabs")]
    [Tooltip("Volumetric Explosion Prefab spawned at explosion")]
    public Transform explosionPrefabVol;

    [Tooltip("Explosion Sound played on explosion")]
    public AudioSource explosionSound;

    [Header("Properties")]
    [Tooltip("Layer to check for collisions")]
    public LayerMask explosionLayer;

    [Range(1, 5)]
    [Tooltip("Explosion duration")]
    public float explosionTime = 3.0f;

    [Range(1, 5)]
    [Tooltip("Explosion range")]
    public int explosionRange = 1;

    [Range(1, 3)]
    [Tooltip("Explosion damage")]
    public int damageAmount = 1;

    //================================================================================
    // Private properties
    //================================================================================

    // Time (for reference)
    private float time = 0;

    // Set to true when bomb explodes
    private bool exploded = false;

    //================================================================================
    // Logic
    //================================================================================

    /// <summary>
    ///     Checks if explosion hit a collider and acts
    ///     according to its type.
    /// </summary>
    /// <param name="hits">Raycast hits</param>
    private void checkExplosion(RaycastHit[] hits) {
        
        Array.Sort<RaycastHit>(hits, (x, y) => x.distance.CompareTo(y.distance));
        for(int i = 0; i < hits.Length; i++) {
            RaycastHit hit = hits[i];
            if (hit.transform.gameObject.tag.Equals("Wall"))
                break;
            if (hit.transform.gameObject.tag.Equals("Explodable")) {
                Explodable c = hit.transform.gameObject.GetComponent<Explodable>();
                c.explode();
            }
            if (hit.transform.gameObject.tag.Equals("GamePlayer")) {
                GamePlayer p = hit.transform.gameObject.GetComponent<GamePlayer>();
                p.damage(damageAmount);
            }
        }
        
    }

    /// <summary>
    ///     Creates an Explosion in the specified direction if no
    ///     collider blocks it.
    /// </summary>
    /// <param name="direction">Direction in which the explosion will be spawned</param>
    private void createExplosions(Vector3 direction) {
        for(int i = 1; i <= explosionRange; i++) {
            RaycastHit hit;
            Physics.Raycast(transform.position, direction, out hit, i, explosionLayer);

            if (!hit.collider) {
              NetworkPoolManager.instance.Instantiate(explosionPrefabVol, transform.position + i * direction);
            } else {
                break;
            }

        }

    }

    /// <summary>
    ///     Instantiates Explosions and Sound.
    /// </summary>
    public void explode() {
        // Instantiate explosionSound and destroy it after 5 seconds
        Destroy(Instantiate(explosionSound.gameObject, transform.position, Quaternion.identity) as GameObject, 5f);
        // Create Explosion on bomb position
        NetworkPoolManager.instance.Instantiate(explosionPrefabVol, transform.position);
        // Create Explosions for all directions
        createExplosions(Vector3.right);
        createExplosions(Vector3.back);
        createExplosions(Vector3.left);
        createExplosions(Vector3.forward);

        RaycastHit[] hits;

        // x-Axis Collisions
        hits = Physics.BoxCastAll(gameObject.transform.position, new Vector3(0f, 0.3f, 0.3f), Vector3.right, Quaternion.identity, 0.5f + explosionRange);
        checkExplosion(hits);

        hits = Physics.BoxCastAll(gameObject.transform.position, new Vector3(0f, 0.3f, 0.3f), Vector3.left, Quaternion.identity, 0.5f + explosionRange);
        checkExplosion(hits);

        // z-Axis Collisions
        hits = Physics.BoxCastAll(gameObject.transform.position, new Vector3(0.3f, 0.3f, 0f), Vector3.forward, Quaternion.identity, 0.5f + explosionRange);
        checkExplosion(hits);

        hits = Physics.BoxCastAll(gameObject.transform.position, new Vector3(0.3f, 0.3f, 0f), Vector3.back, Quaternion.identity, 0.5f + explosionRange);
        checkExplosion(hits);

        exploded = true;
    }

    void OnEnable() {
        time = 0;
        exploded = false;
        transform.localScale = new Vector3(0, 0, 0);
    }

    //================================================================================
    // Start/Update
    //================================================================================

    void Start() {
        // Bomb grows inside Update()
        gameObject.transform.localScale = new Vector3(0, 0, 0);
    }

    void Update() {
        // Don't update if bomb already exploded
        if (exploded)
            return;

        if (time < explosionTime) {
            // Increase Bomb size
            gameObject.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, new Vector3(0.75f, 0.75f, 0.75f), .1f);
            time += Time.deltaTime;
        } else {

            // Explode after explosionTime seconds
            explode();

            // Server removes bomb instances
            if (isServer) {
                NetworkPoolManager.instance.UnSpawnObject(gameObject);
                NetworkServer.UnSpawn(gameObject);
            }
        }
    }

    public override void OnNetworkDestroy() {
        // Server could unspawn object before client spawns
        // explosions. To prevent this behaviour, the bomb explodes
        // if Server unspawns it and it hasn't exploded yet.
        if(!exploded)
            explode();
    }
}
