using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

/// <summary>
///     The CustomLobbyManager class handles all logic related to the Lobby (game creation, game joining, etc.)
///     and manages all UI elements seen in the main menu.
/// </summary>
[RequireComponent(typeof(PlayerLobbyHook))]
public class CustomLobbyManager : NetworkLobbyManager {

    //================================================================================
    // Prefab Components (Inspector)
    //================================================================================

    [Header("UI References")]
    [Tooltip("Main Menu UI root")]
    public RectTransform mainPanel;

    [Tooltip("Lobby UI root")]
    public RectTransform lobbyPanel;

    [Tooltip("Lobby UI CountdownPanel")]
    public CountdownPanel countdownPanel;

    [Tooltip("Information Panel")]
    public RectTransform infoPanel;

    [Tooltip("Lobby UI PlayerList")]
    public CustomLobbyPlayerList playerList;
    [Space(10)]

    

    //================================================================================
    // Prefab Properties
    //================================================================================

    [Header("Lobby Properties")]
    [Tooltip("Time before match starts after every player is ready")]
    public float prematchCountdown = 3.0f;
    [Space(10)]

    //================================================================================
    // Private/Protected Properties
    //================================================================================

    // Current panel shown to user
    private RectTransform currentPanel;

    // LobbyHook transfers LobbyPlayer properties to GamePlayer
    protected PlayerLobbyHook lobbyHook;

    private bool isInGame = false;

    private bool isMatchmaking = false;

    private bool disconnectServer = false;

    private ulong currentMatchID;

    //================================================================================
    // Public Properties
    //================================================================================

    public delegate void BackButtonDelegate();
    // Function to call on Back-Button Press
    public BackButtonDelegate backDelegate;

    //================================================================================
    // Static Properties
    //================================================================================

    // Singleton
    public static CustomLobbyManager lobbyManagerSingleton;

    /// <summary>
    /// Called after user clicks on "Create Game"-Button
    /// </summary>
    public void OnCreateGameClicked() {
        // Server needs to reset colors in use to prevent endless while loop
        CustomLobbyPlayer.ResetColorsInUse();
        string matchName = RandomNameGenerator.generateRoomName();
        ChangeTo(lobbyPanel);
        matchMaker.CreateMatch(
            matchName,
            (uint)maxPlayers,
            true,
            "", "", "", 0, 0,
            OnMatchCreate);

    }

    /// <summary>
    /// Called after user clicks on "Join Game"-Button
    /// </summary>
    public void OnJoinGameClicked() {
        infoPanel.GetComponentInChildren<Text>().text = "Joining Game";
        infoPanel.gameObject.SetActive(true);
        matchMaker.ListMatches(0, 40, "", false, 0, 0, OnMatchList);
    }

    /// <summary>
    /// Finds the first match with at least 1 empty slot. Returns null if none exists.
    /// </summary>
    /// <param name="matchList">List containing all matches</param>
    /// <returns>MatchInfoSnapshot</returns>
    MatchInfoSnapshot FindMatch(List<MatchInfoSnapshot> matchList) {
        for (int i = 0; i < matchList.Count; i++) {
            if (matchList[i].currentSize < matchList[i].maxSize)
                return matchList[i];
        }

        return null;
    }

    void JoinGame(MatchInfoSnapshot match) {
        matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, OnMatchJoined);
    }

    /// <summary>
    /// Switches from one panel to another panel.
    /// </summary>
    /// <param name="newPanel">Panel to display</param>
    public void ChangeTo(RectTransform newPanel) {
        if (currentPanel != null) {
            currentPanel.gameObject.SetActive(false);
        }

        if (newPanel != null) {
            newPanel.gameObject.SetActive(true);
        }

        currentPanel = newPanel;
    }

    IEnumerator CloseInfoPanel(float delay) {
        yield return new WaitForSeconds(delay);

        infoPanel.gameObject.SetActive(false);
    }

    public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList) {
        base.OnMatchList(success, extendedInfo, matchList);

        if (success) {
            if (matchList.Count > 0) {
                MatchInfoSnapshot match = FindMatch(matchList);
                if (match != null)
                    JoinGame(match);
                else {
                    infoPanel.GetComponentInChildren<Text>().text = "No empty slot found";
                    StartCoroutine(CloseInfoPanel(2.0f));
                }
            } else {
                infoPanel.GetComponentInChildren<Text>().text = "No games available";
                StartCoroutine(CloseInfoPanel(2.0f));
            }
        }
    }

    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo) {
        base.OnMatchJoined(success, extendedInfo, matchInfo);
    }

    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo) {
        base.OnMatchCreate(success, extendedInfo, matchInfo);
        currentMatchID = (System.UInt64)matchInfo.networkId;
    }

    public override void OnDestroyMatch(bool success, string extendedInfo) {
        base.OnDestroyMatch(success, extendedInfo);
        if (disconnectServer) {
            StopMatchMaker();
            StopHost();
        }
    }

    public override void OnStartHost() {
        base.OnStartHost();

        backDelegate = StopHostClbk;

        ChangeTo(lobbyPanel);
    }

    /// <summary>
    ///     Called when players clicks on "Back to Menu"
    ///     after the game has ended.
    /// </summary>
    public void GoBackButton() {
        backDelegate();
        isInGame = false;
    }

    /// <summary>
    ///     Stops the Host.
    /// </summary>
    public void StopHostClbk() {
        if (isMatchmaking) {
            matchMaker.DestroyMatch((NetworkID)currentMatchID, 0, OnDestroyMatch);
            disconnectServer = true;
        } else {
            StopHost();
        }

        ChangeTo(mainPanel);
    }

    /// <summary>
    ///     Stops the Client.
    /// </summary>
    public void StopClientClbk() {
        StopClient();

        if (isMatchmaking) {
            StopMatchMaker();
        }

        ChangeTo(mainPanel);
    }

    /// <summary>
    ///     Stops dedicated Server.
    /// </summary>
    public void StopServerClbk() {
        StopServer();
        ChangeTo(mainPanel);
    }

    //================================================================================
    // Server
    //================================================================================

    public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId) {
        GameObject obj = Instantiate(lobbyPlayerPrefab.gameObject) as GameObject;

        CustomLobbyPlayer newPlayer = obj.GetComponent<CustomLobbyPlayer>();
        newPlayer.ToggleJoinButton(numPlayers + 1 >= minPlayers);

        UpdateJoinButtons();

        return obj;
    }

    public override void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId) {
        UpdateJoinButtons();
    }

    public override void OnLobbyServerDisconnect(NetworkConnection conn) {
        UpdateJoinButtons();

    }

    /// <summary>
    ///     Enables Join-Buttons if number of players
    ///     is equal or greater than number of required players.
    /// </summary>
    private void UpdateJoinButtons() {
        for (int i = 0; i < lobbySlots.Length; ++i) {
            CustomLobbyPlayer p = lobbySlots[i] as CustomLobbyPlayer;

            if (p != null) {
                p.ToggleJoinButton(numPlayers + 1 >= minPlayers);
            }
        }
    }

    public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer) {
        lobbyHook.OnLobbyServerSceneLoadedForPlayer(this, lobbyPlayer, gamePlayer);

        mainPanel.gameObject.SetActive(false);
        lobbyPanel.gameObject.SetActive(false);

        return true;
    }

    public override void OnLobbyServerPlayersReady() {
        bool allready = true;
        for (int i = 0; i < lobbySlots.Length; ++i) {
            if (lobbySlots[i] != null)
                allready &= lobbySlots[i].readyToBegin;
        }

        if (allready)
            StartCoroutine(ServerCountdownCoroutine());
    }

    /// <summary>
    ///     Starts Match countdown.
    /// </summary>
    /// <returns></returns>
    public IEnumerator ServerCountdownCoroutine() {
        float remainingTime = prematchCountdown;
        int floorTime = Mathf.FloorToInt(remainingTime);

        while (remainingTime > 0) {
            yield return null;

            remainingTime -= Time.deltaTime;
            int newFloorTime = Mathf.FloorToInt(remainingTime);

            if (newFloorTime != floorTime) {
                floorTime = newFloorTime;

                for (int i = 0; i < lobbySlots.Length; ++i) {
                    if (lobbySlots[i] != null) {
                        (lobbySlots[i] as CustomLobbyPlayer).RpcUpdateCountdown(floorTime);
                    }
                }
            }
        }

        for (int i = 0; i < lobbySlots.Length; ++i) {
            if (lobbySlots[i] != null) {
                (lobbySlots[i] as CustomLobbyPlayer).RpcUpdateCountdown(0);
            }
        }

        ServerChangeScene(playScene);
    }

    //================================================================================
    // Client
    //================================================================================

    public override void OnLobbyClientSceneChanged(NetworkConnection conn) {
        if (networkSceneName.Equals("LobbyScene")) {
            if (isInGame) {
                ChangeTo(lobbyPanel);
                if (isMatchmaking) {
                    if (conn.playerControllers[0].unetView.isServer) {
                        backDelegate = StopHostClbk;
                    } else {
                        backDelegate = StopClientClbk;
                    }
                } else {
                    if (conn.playerControllers[0].unetView.isClient) {
                        backDelegate = StopHostClbk;
                    } else {
                        backDelegate = StopClientClbk;
                    }
                }
            } else {
                ChangeTo(mainPanel);
            }

            isInGame = false;
        } else {
            ChangeTo(null);

            isInGame = true;
        }
    }

    public override void OnClientSceneChanged(NetworkConnection conn) {
        base.OnClientSceneChanged(conn);

        if (networkSceneName.Equals("GameScene"))
            ChangeTo(null);
        else {
            StopMatchMaker();
            if (conn.playerControllers[0].unetView.isServer) {
                StopHost();
            } else {
                StopClient();
            }
            ChangeTo(mainPanel);
        }
            
    }

    public override void OnClientConnect(NetworkConnection conn) {
        base.OnClientConnect(conn);

        if (!NetworkServer.active) {
            backDelegate = StopClientClbk;
            infoPanel.gameObject.SetActive(false);
            ChangeTo(lobbyPanel);
        }
    }


    public override void OnClientDisconnect(NetworkConnection conn) {
        base.OnClientDisconnect(conn);
        ChangeTo(mainPanel);
        StartMatchMaker();
    }

    public override void OnClientError(NetworkConnection conn, int errorCode) {
        ChangeTo(mainPanel);
    }

    //================================================================================
    // Unity
    //================================================================================

    void Start() {
        // Set singleton instance
        lobbyManagerSingleton = this;
        // Keep Lobby-Scene on Scene change
        DontDestroyOnLoad(gameObject);
        currentPanel = mainPanel;
        lobbyHook = GetComponent<PlayerLobbyHook>();
        StartMatchMaker();
    }

}
