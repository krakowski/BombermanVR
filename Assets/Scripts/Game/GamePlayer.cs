using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

/// <summary>
///     The GamePlayer class handles all logic (movement, damage, etc.) related to the player.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(BombPlacer))]
public class GamePlayer : NetworkBehaviour {

    //================================================================================
    // Network properties
    //================================================================================
    
    [HideInInspector]
    [SyncVar(hook = "OnPlayerNameChanged")]
    public string playerName = "";

    [HideInInspector]
    [SyncVar(hook = "OnPlayerColorIndexChanged")]
    public int playerColorIndex;

    //================================================================================
    // Prefab components (Inspector)
    //================================================================================


    [Header("Network Management")]
    [Tooltip("Components that should be deactivated on all other players")]
    public GameObject[] deactivateOnRemote;

    [Tooltip("Components that should be deactivated on all other players")]
    public GameObject[] deactivateOnLocal;
    [Space(10)]

    //--------------------------------------------------------------------------------
    [Header("GamePlayer Properties")]
    [Tooltip("Sets wether the player should be a spectator or not")]
    public bool isSpectator = false;

    [Range(1, 5)]
    [Tooltip("The players speed")]
    public float walkSpeed = 2f;
    [Space(10)]

    //--------------------------------------------------------------------------------
    [Header("GamePlayer Components")]
    [Tooltip("The players health component")]
    public Health health;

    [Tooltip("The players character controller")]
    public CharacterController controller;

    [Tooltip("The players main camera")]
    public Camera vrCamera;

    [Tooltip("The players renderer")]
    public Renderer playerRenderer;

    [Tooltip("Materials which can be applied to players body")]
    public Material[] playerMaterials;
    [Space(10)]

    //--------------------------------------------------------------------------------
    [Header("UI References")]
    [Tooltip("Name above the players character")]
    public Text playerNameText;

    //--------------------------------------------------------------------------------
    [Header("Position References")]
    [Tooltip("Point from which the player should observe the game")]
    public Transform spectatorPosition;

    //================================================================================
    // Private properties
    //================================================================================

    // Used in Update()
    private Vector3 moveDirection;

    private HUD hud;

    private LeaderBoard leaderBoard;

    private float spawnTime;

    //================================================================================
    // Logic
    //================================================================================


    /// <summary>
    /// Local GamePlayers Update()-method
    /// </summary>
    void LocalUpdate() {
        if (isSpectator)
            return;

        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = vrCamera.transform.TransformDirection(moveDirection);
        if (moveDirection != Vector3.zero)
            controller.SimpleMove(moveDirection * walkSpeed);
    }

    /// <summary>
    /// Remote GamePlayers Update()-method
    /// </summary>
    void RemoteUpdate() {

    }

    /// <summary>
    /// Deactivates all Components set via the Inspector.
    /// </summary>
    private void DeactivateComponents() {
        GameObject[] toDeactivate = isLocalPlayer ? deactivateOnLocal : deactivateOnRemote;

        for (int i = 0; i < toDeactivate.Length; i++) {
            toDeactivate[i].SetActive(false);
        }
    }

    void initPlayer() {
        playerNameText.text = playerName;

    }

    public void heal(int amount) {
        health.heal(amount);
    }

    public void damage(int amount) {
        // Server applies damage
        if (!isServer)
            return;

        health.damage(amount);

        if (health.isDead() && !isSpectator) {
            isSpectator = true;
            RpcSetAsSpectator();

            int activePlayers = 0;
            GamePlayer lastActivePlayer = null;
            GamePlayer current;
            for(int i = 0; i < NetworkServer.connections.Count; i++) {
                for(int j = 0; j < NetworkServer.connections[i].playerControllers.Count; j++) {
                    current = NetworkServer.connections[i].playerControllers[j].gameObject.GetComponent<GamePlayer>();
                    if (!current.isSpectator) {
                        activePlayers++;
                        lastActivePlayer = current;
                    }     
                }
            }

            // Set last active player as Spectator
            if (activePlayers == 1) {
                lastActivePlayer.isSpectator = true;
                lastActivePlayer.RpcSetAsSpectator();
                RpcShowLeaderboard();
            }
                
        }
    }

    public float getLifeTime() {
        return Time.time - spawnTime;
    }

    public override void OnStartLocalPlayer() {
        controller = GetComponent<CharacterController>();
        vrCamera = transform.GetChild(0).GetComponent<Camera>();
        transform.GetChild(1).GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    

    IEnumerator removeSpeedEffect(float seconds) {
        // Wait until Effect wears off
        yield return new WaitForSeconds(seconds);
        walkSpeed = 2f;
    }

    public void speedUp(float seconds) {
        walkSpeed *= 2;
        StartCoroutine(removeSpeedEffect(seconds));
    }

    //================================================================================
    // SyncVar hooks
    //================================================================================

    public void OnPlayerColorIndexChanged(int index) {
        playerColorIndex = index;
        Material[] mats = playerRenderer.materials;
        mats[1] = playerMaterials[playerColorIndex];
        playerRenderer.materials = mats;
    }

    public void OnPlayerNameChanged(string name) {
        playerName = name;
    }

    //================================================================================
    // Server commands
    //================================================================================

    [Command]
    public void CmdChangePlayerName(string name) {
        playerName = name;
    }

    [Command]
    public void CmdPlayerReadyToRestart() {
        leaderBoard = FindObjectOfType<LeaderBoard>();
        leaderBoard.playersReady++;

        int activePlayers = NetworkServer.connections.Count;

        if (leaderBoard.playersReady == activePlayers) {

            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

            // Reset all connected Players
            for (int i = 0; i < NetworkServer.connections.Count; i++) {
                for (int j = 0; j < NetworkServer.connections[i].playerControllers.Count; j++) {
                    GamePlayer g = NetworkServer.connections[i].playerControllers[j].gameObject.GetComponent<GamePlayer>();
                    g.isSpectator = false;
                    g.RpcResetPlayer(spawnPoints[i].transform.position + new Vector3(0.0f, 1.5f, 0.0f));
                }
            }

            leaderBoard.playersReady = 0;

        }
    }

    [Command]
    public void CmdPlayerNotReadyToRestart() {
        leaderBoard = FindObjectOfType<LeaderBoard>();
        leaderBoard.playersReady--;
    }

    //================================================================================
    // Server commands
    //================================================================================

    [ClientRpc]
    public void RpcSetAsSpectator() {
        transform.position = spectatorPosition.position;
        vrCamera.GetComponent<GvrPointerPhysicsRaycaster>().enabled = false;
        hud.Hide();
        playerRenderer.enabled = false;
        isSpectator = true;

        leaderBoard = FindObjectOfType<LeaderBoard>();
        leaderBoard.AddPlayer(this);
    }

    [ClientRpc]
    public void RpcShowLeaderboard() {
        leaderBoard = FindObjectOfType<LeaderBoard>();
        leaderBoard.ToggleVisibility(true);
    }

    [ClientRpc]
    public void RpcResetPlayer(Vector3 position) {
        leaderBoard = FindObjectOfType<LeaderBoard>();
        leaderBoard.ToggleVisibility(false);
        leaderBoard.Reset();

        transform.position = position;
        vrCamera.GetComponent<GvrPointerPhysicsRaycaster>().enabled = true;
        hud.Show();
        playerRenderer.enabled = true;
        isSpectator = false;

        health.heal(3);
    }

    //================================================================================
    // Unity
    //================================================================================

    void Start() {
        spawnTime = Time.time;
        hud = GetComponent<HUD>();
        DeactivateComponents();
        initPlayer();
    }

    void Update() {
        // Distinguish between local and remote players
        if (isLocalPlayer)
            LocalUpdate();
        else
            RemoteUpdate();
    }

}
