using UnityEngine;

/// <summary>
///     The InteractiveObject class can be attached to all GameObjects which should
///     be only interactable from within a certain distance.
/// </summary>
public class InteractiveObject : MonoBehaviour {

    [Header("Properties")]
    [Tooltip("Maxiumum distance at which the player may interact with the obejct")]
    [Range(0, 5)]
    public float interactionDistance = 2.0f;

}
