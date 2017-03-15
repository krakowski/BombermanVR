using UnityEngine;
using UnityEngine.Networking;

/// <summary>
///     The HUD class manages the HUD (name, lifes, etc.) shown to the player during gameplay.
/// </summary>
public class HUD : NetworkBehaviour {

    //================================================================================
    // Prefab components (Inspector)
    //================================================================================

    [Header("UI References")]
    [Tooltip("GameObject containing HUD Elements")]
    public Transform hudContainer;

    [Tooltip("The Players mesh")]
    public Transform playerModel;

    [Tooltip("Lifebar shown to self")]
    public Transform localLifebar;

    [Tooltip("Lifebar shown to others")]
    public Transform remoteLifebar;

    [Tooltip("Player Camera")]
    public Camera target;

    [Header("HUD Properties")]
    [Tooltip("Speed at which HUD follows players camera")]
    public float followSpeed = 4f;

    //================================================================================
    // Private properties
    //================================================================================

    // Lifebar hearts
    private Heart[] hearts;

    // Lerp values
    private static float heartMinLerp = 40f;
    private static float heartMaxLerp = 41f;

    // Current pulsating hearts index
    private int currentPulsating = 2;

    // Time (for reference)
    private float t = 0.0f;

    //================================================================================
    // Logic
    //================================================================================

    /// <summary>
    ///     Updates visible hearts according to players lifes.
    /// </summary>
    /// <param name="lifes">Players lifes</param>
    public void setLifes(int lifes) {

        // Reset current hearts transform
        if(currentPulsating >= 0) {
            hearts[currentPulsating].setSize(new Vector2(heartMinLerp, heartMinLerp));
        }

        // Show/Hide hearts according to lifes
        for (int i = 0; i < hearts.Length; i++) {
            if (i + 1 <= lifes) {
                hearts[i].setImageVisible(true);
            } else {
                hearts[i].setImageVisible(false);
            }
        }

        // Update current pulsating heart index
        currentPulsating = lifes - 1;
    }

    /// <summary>
    ///     Animates hearts contained inside players lifebar.
    /// </summary>
    private void animateHearts() {

        // Index out of Bounds
        if (currentPulsating < 0)
            return;

        // Lerp heart size
        float size = Mathf.Lerp(heartMinLerp, heartMaxLerp, t);

        // Update current pulsating hearts size
        hearts[currentPulsating].setSize(new Vector2(size, size));
        t += 2f * Time.deltaTime;

        // Switch direction after 1 second
        if (t > 1.0f) {
            float temp = heartMaxLerp;
            heartMaxLerp = heartMinLerp;
            heartMinLerp = temp;
            t = 0.0f;
        }
    }

    /// <summary>
    ///     Updates rotation according to camera rotation.
    /// </summary>
    private void FollowCamera() {
        // Lerp between hud angles and player camera angles
        float angle = Mathf.LerpAngle(hudContainer.localEulerAngles.y, target.transform.localEulerAngles.y, Time.deltaTime * followSpeed);
        hudContainer.localEulerAngles = new Vector3(0, angle, 0);
        // Update player model rotation according to camera rotation
        playerModel.rotation = target.transform.rotation;
    }

    /// <summary>
    ///     Hides the HUD.
    /// </summary>
    public void Hide() {
        hudContainer.gameObject.SetActive(false);
    }

    //================================================================================
    // Start/Update
    //================================================================================

    void Start() {
        // Determine which hearts to manage
        hearts = isLocalPlayer ? localLifebar.GetComponentsInChildren<Heart>() : remoteLifebar.GetComponentsInChildren<Heart>();
    }

    void Update() {
        if (!isLocalPlayer)
            return;

        animateHearts();
        FollowCamera();
    }
}
