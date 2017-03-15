using System.Collections;
using UnityEngine.Networking;
using UnityEngine;

/// <summary>
///     A GameObject which spawns random PowerUps.
/// </summary>
public class PowerUpSpawner : NetworkBehaviour {

    //================================================================================
    // Prefab components
    //================================================================================

    [Header("Prefabs")]
    [Tooltip("Powerups that should be spawed")]
    public Transform[] powerupPrefabs;

    //================================================================================
    // Prefab Properties
    //================================================================================

    [Header("Properties")]
    [Range(5, 20)]
    public float spawnInterval = 10;

    //================================================================================
    // Private Properties
    //================================================================================

    // Next element (index) to be spawned by Server
    private int nextElement;

    // Random Number provider
    private static System.Random rnd = new System.Random();

    //================================================================================
    // Logic
    //================================================================================

    IEnumerator spawnPowerUp() {

        nextElement = rnd.Next(0, powerupPrefabs.Length);

        yield return new WaitForSeconds(spawnInterval);

        // Spawn random PowerUp
        Transform powerUp = Instantiate(powerupPrefabs[nextElement], transform.position, Quaternion.identity);
        powerUp.parent = transform;
        NetworkServer.Spawn(powerUp.gameObject);
    }

    public void pickUp() {
        if (isServer)
            StartCoroutine(spawnPowerUp());

    }

    public override void OnStartServer() {
        base.OnStartServer();
        StartCoroutine(spawnPowerUp());
    }
}
