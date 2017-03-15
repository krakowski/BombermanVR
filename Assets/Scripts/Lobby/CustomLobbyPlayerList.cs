using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     The CustomLobbyPlayerList class manages all active players inside a specific lobby.
///     Players can be added and removed from here.
/// </summary>
[RequireComponent(typeof(HorizontalLayoutGroup))]
public class CustomLobbyPlayerList : MonoBehaviour {

    //================================================================================
    // Private properties
    //================================================================================

    private List<CustomLobbyPlayer> players = new List<CustomLobbyPlayer>();

    //================================================================================
    // Logic
    //================================================================================

    /// <summary>
    ///     Adds a CustomLobbyPlayer to the list and displays
    ///     on the Lobby GUI.
    /// </summary>
    /// <param name="player">CustomLobbyPlayer that should be added</param>
    public void AddPlayer(CustomLobbyPlayer player) {
        if (players.Contains(player))
            return;

        players.Add(player);

        player.transform.SetParent(transform, false);
    }

    /// <summary>
    ///     Removes a specific CustomLobbyPlayer from the list.
    /// </summary>
    /// <param name="player">Player to remove from list</param>
    public void RemovePlayer(CustomLobbyPlayer player) {
        players.Remove(player);
    }
}
