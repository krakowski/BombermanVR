using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

/// <summary>
///     The CustomLobbyPlayer class contains all information about a specific player inside
///     the lobby and handles all its events.
/// </summary>
public class CustomLobbyPlayer : NetworkLobbyPlayer {

    //================================================================================
    // Prefab components (Inspector)
    //================================================================================

    [Header("UI Resources")]
    [Tooltip("Player images used in the Lobby")]
    public Sprite[] playerSprites;
    [Space(10)]

    [Header("UI References")]
    [Tooltip("Player name Text shown in the Lobby")]
    public Text playerNameText;
    [Tooltip("Player image shown in the Lobby")]
    public Image playerImage;
    [Tooltip("Join Button")]
    public Button joinButton;
    [Tooltip("Ready Button")]
    public Button readyButton;
    [Tooltip("Waiting Button")]
    public Button waitingButton;

    //================================================================================
    // Network properties
    //================================================================================

    [HideInInspector]
    [SyncVar(hook = "OnPlayerNameChanged")]
    public string playerName = "";

    [HideInInspector]
    [SyncVar(hook = "OnPlayerColorIndexChanged")]
    public int playerColorIndex = 0;

    //================================================================================
    // Private properties
    //================================================================================

    //used on server to avoid assigning the same color to two player
    private static List<int> colorInUse = new List<int>();

    public static void ResetColorsInUse() {
        colorInUse.Clear();
    }

    public override void OnClientEnterLobby() {
        base.OnClientEnterLobby();

        CustomLobbyManager.lobbyManagerSingleton.playerList.AddPlayer(this);

        if (isLocalPlayer) {
            SetupLocalPlayer();
        } else {
            SetupOtherPlayer();
        }

        OnPlayerNameChanged(playerName);
        OnPlayerColorIndexChanged(playerColorIndex);
    }

    public override void OnStartAuthority() {
        base.OnStartAuthority();

        SetupLocalPlayer();
    }

    void SetupOtherPlayer() {

        readyButton.gameObject.SetActive(false);
        waitingButton.gameObject.SetActive(true);
        joinButton.gameObject.SetActive(false);
       
        OnClientReady(false);
    }

    void SetupLocalPlayer() {

        readyButton.gameObject.SetActive(false);
        waitingButton.gameObject.SetActive(false);
        joinButton.gameObject.SetActive(true);

        CmdColorChange();

        if (playerName == "")
            CmdNameChanged(RandomNameGenerator.generatePlayerName());
    }

    public void onJoinClicked() {
        joinButton.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(true);
        SendReadyToBeginMessage();
    }

    public void ToggleJoinButton(bool enabled) {
        joinButton.gameObject.SetActive(enabled);
        readyButton.gameObject.SetActive(!enabled);
    }

    public override void OnClientReady(bool readyState) {
        if (readyState) {
            readyButton.gameObject.SetActive(true);
            waitingButton.gameObject.SetActive(false);
            joinButton.gameObject.SetActive(false);
        } else {
            readyButton.gameObject.SetActive(false);
            waitingButton.gameObject.SetActive(!isLocalPlayer);
            joinButton.gameObject.SetActive(isLocalPlayer);
        }
    }

    //================================================================================
    // Network client calls
    //================================================================================

    /// <summary>
    ///     Updates the countdown message on all connected clients.
    ///     Should be called on the Server.
    /// </summary>
    /// <param name="countdown">Seconds until match starts</param>
    [ClientRpc]
    public void RpcUpdateCountdown(int countdown) {
        CustomLobbyManager.lobbyManagerSingleton.countdownPanel.SetMessage("Match Starting in " + countdown);
        CustomLobbyManager.lobbyManagerSingleton.countdownPanel.gameObject.SetActive(countdown != 0);
    }

    //================================================================================
    // Network hooks
    //================================================================================

    public void OnPlayerNameChanged(string name) {
        playerName = name;
        playerNameText.text = playerName;
    }

    public void OnPlayerColorIndexChanged(int index) {
        playerColorIndex = index;
        playerImage.sprite = playerSprites[playerColorIndex];
    }

    //================================================================================
    // Network commands
    //================================================================================

    /// <summary>
    ///     Changes the Players name on the Server, which in turn sends the
    ///     updated name to all Clients.
    /// </summary>
    /// <param name="name"></param>
    [Command]
    public void CmdNameChanged(string name) {
        playerName = name;
    }

    [Command]
    public void CmdColorChange() {
        int idx = 0;

        int inUseIdx = colorInUse.IndexOf(idx);

        idx = (idx + 1) % playerSprites.Length;

        bool alreadyInUse = false;

        do {
            alreadyInUse = false;
            for (int i = 0; i < colorInUse.Count; ++i) {
                if (colorInUse[i] == idx) {//that color is already in use
                    alreadyInUse = true;
                    idx = (idx + 1) % playerSprites.Length;
                }
            }
        }
        while (alreadyInUse);

        if (inUseIdx >= 0) {//if we already add an entry in the colorTabs, we change it
            colorInUse[inUseIdx] = idx;
        } else {//else we add it
            colorInUse.Add(idx);
        }

        playerColorIndex = idx;
    }

}
