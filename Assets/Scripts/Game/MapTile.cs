using UnityEngine;

/// <summary>
///     An interactable tile on the map.
/// </summary>
class MapTile : MonoBehaviour, IGvrGazeResponder {

    //================================================================================
    // Prefab components (Inspector)
    //================================================================================

    [Header("Prefabs")]
    [Tooltip("Material applied on Enter")]
    public Material activeMaterial;

    [Tooltip("Material applied on Exit")]
    public Material inactiveMaterial;

    [Tooltip("The tiles renderer")]
    public new Renderer renderer;

    
    /// <summary>
    ///     Called whenever Players reticle enters this tile.
    /// </summary>
    public void OnGazeEnter() {
        renderer.material = activeMaterial;
    }

    /// <summary>
    ///     Called whenever Players reticle exits this tile.
    /// </summary>
    public void OnGazeExit() {
        renderer.material = inactiveMaterial;
    }

    public void OnGazeTrigger() {
    }

    //================================================================================
    // Prefab components (Inspector)
    //================================================================================

    void Start() {
        renderer = GetComponent<Renderer>();
    }
}
