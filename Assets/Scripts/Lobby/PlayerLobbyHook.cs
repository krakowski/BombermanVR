using UnityEngine;
using UnityEngine.Networking;

/// <summary>
///     The PlayerLobbyHook class is used to transfer all information (player name, player color)
///     from the lobby to the GamePlayer class.
/// </summary>
public class PlayerLobbyHook : MonoBehaviour {

    public void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer) {
        GamePlayer player = gamePlayer.GetComponent<GamePlayer>();
        CustomLobbyPlayer lobby = lobbyPlayer.GetComponent<CustomLobbyPlayer>();

        // Transfer Lobby Information to Game
        player.playerName = lobby.playerName;
        player.playerColorIndex = lobby.playerColorIndex;
    }

}
