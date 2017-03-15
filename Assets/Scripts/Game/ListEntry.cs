using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     An entry inside the LeaderBoard shown at the end of a game.
/// </summary>
public class ListEntry : MonoBehaviour {

    //================================================================================
    // Prefab components (Inspector)
    //================================================================================

    [Header("UI References")]
    [Tooltip("Player Rank")]
    public Text playerRank;

    [Tooltip("Player Name")]
    public Text playerNameText;

    [Tooltip("Player Time")]
    public Text playerTime;

    //================================================================================
    // Logic
    //================================================================================

    public void setRank(int pos) {
        playerRank.text = "" + pos;
    }

    public void setPlayerName(string name) {
        playerNameText.text = name;
    }

    public void setPlayerTime(float time) {
        playerTime.text = "" + time;
    }

}
