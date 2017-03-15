using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     The CountdownPanel class manages the content which is shown
///     to the user before a match starts.
/// </summary>
public class CountdownPanel : MonoBehaviour {

    //================================================================================
    // Prefab components (Inspector)
    //================================================================================

    [Header("UI References")]
    [Tooltip("Text that displays the countdown")]
    public Text countDownText;

    //================================================================================
    // Logic
    //================================================================================

    /// <summary>
    ///     Sets the message shown to the user.
    /// </summary>
    /// <param name="message">Message to be shown</param>
    public void SetMessage(string message) {
        countDownText.text = message;
    }
}
