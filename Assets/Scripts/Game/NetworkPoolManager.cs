using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
///     The NetworkPoolManager class generates Object pools which can be used for efficient
///     instantiation of GameObjects during gameplay. It also registers custom spawn handlers
///     for all pooled Objects, so they can be spawned over the network.
/// </summary>
public class NetworkPoolManager : MonoBehaviour {

    // Serializable -> Visible in Editor
    /// <summary>
    ///     An entry for the pool, consisting of the prefab and the pool size.
    /// </summary>
    [Serializable]
    public struct PoolEntry {
        public Transform prefab;
        public int size;
    }

    //================================================================================
    // Prefab components (Inspector)
    //================================================================================

    [Header("Object Pool")]
    [Tooltip("Prefabs to register in Pool")]
    public PoolEntry[] poolEntrys;

    //================================================================================
    // Private properties
    //================================================================================

    // Map (networkId -> Queue with pooled GameObjects)
    private Dictionary<NetworkHash128, Queue<GameObject>> poolMap = new Dictionary<NetworkHash128, Queue<GameObject>>();

    // Singleton
    static NetworkPoolManager _instance;

    //================================================================================
    // Public properties
    //================================================================================

    // Singleton getter
    public static NetworkPoolManager instance {
        get {
            return _instance;
        }
    }

    //================================================================================
    // Logic
    //================================================================================

    /// <summary>
    ///     Fills the NetworkPool with instances of the specified prefab
    ///     according to specified size.
    /// </summary>
    /// <param name="prefab">Prefab to be pooled</param>
    /// <param name="size">Amount of pooled prefabs</param>
    public void createNetworkPool(Transform prefab, int size) {
        createNetworkPool(prefab.gameObject, size);
    }

    /// <summary>
    ///     Fills the NetworkPool with instances of the specified prefab
    ///     according to specified size.
    /// </summary>
    /// <param name="prefab">Prefab to be pooled</param>
    /// <param name="size">Amount of pooled prefabs</param>
    public void createNetworkPool(GameObject prefab, int size) {
        NetworkHash128 assetId = prefab.GetComponent<NetworkIdentity>().assetId;

        if (!poolMap.ContainsKey(assetId)) {
            poolMap.Add(assetId, new Queue<GameObject>());

            for(int i = 0; i < size; i++) {
                GameObject go = Instantiate(prefab);
                go.SetActive(false);
                poolMap[assetId].Enqueue(go);
            }

        }

        ClientScene.RegisterSpawnHandler(assetId, SpawnObject, UnSpawnObject);
    }

    /// <summary>
    ///     Only used for offline spawning.
    ///     Gets the next available Instance out of the Pool, enables it, sets its position
    ///     and returns it to the caller. Returns null if requestes assetId does not exist inside pool.
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">Position at wich the GameObject should be spawned</param>
    /// <returns>The requestet pooled GameObject or null if the pool doesn't contain it.</returns>
    public GameObject Instantiate(GameObject prefab, Vector3 position) {
        NetworkHash128 assetId = prefab.GetComponent<NetworkIdentity>().assetId;

        return Instantiate(assetId, position);
    }

    /// <summary>
    ///     Only used for offline spawning.
    ///     Gets the next available Instance out of the Pool, enables it, sets its position
    ///     and returns it to the caller. Returns null if requestes assetId does not exist inside pool.
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">Position at wich the GameObject should be spawned</param>
    /// <returns>The requestet pooled GameObject or null if the pool doesn't contain it.</returns>
    public GameObject Instantiate(Transform prefab, Vector3 position) {
        NetworkHash128 assetId = prefab.GetComponent<NetworkIdentity>().assetId;

        return Instantiate(assetId, position);
    }

    /// <summary>
    ///     Gets the next available Instance out of the Pool, enables it, sets its position
    ///     and returns it to the caller. Returns null if requestes assetId does not exist inside pool.
    /// </summary>
    /// <param name="assetId">The GameObjects unique NetworkHash128</param>
    /// <param name="position">Position at wich the GameObject should be spawned</param>
    /// <returns>The requestet pooled GameObject or null if the pool doesn't contain it.</returns>
    public GameObject Instantiate(NetworkHash128 assetId, Vector3 position) {

        if (poolMap.ContainsKey(assetId)){
            GameObject go = poolMap[assetId].Dequeue();
            poolMap[assetId].Enqueue(go);

            go.transform.position = position;
            go.SetActive(true);
            return go;
        }

        return null;
    }

    public void Destroy(GameObject spawned) {
        spawned.SetActive(false);
    }

    public GameObject SpawnObject(Vector3 position, NetworkHash128 assetId) {
        return Instantiate(assetId, position);
    }

    public void UnSpawnObject(GameObject spawned) {
        Destroy(spawned);
    }

    //================================================================================
    // Unity
    //================================================================================

    void Awake() {
        // Set singleton instance if not null
        if (_instance == null)
            _instance = FindObjectOfType<NetworkPoolManager>();
    }

    void Start() {
        // Fill object pool
        for (int i = 0; i < poolEntrys.Length; i++) {
            createNetworkPool(poolEntrys[i].prefab, poolEntrys[i].size);
        }
    }
}
