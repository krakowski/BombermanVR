using UnityEngine;
using System.Collections;
using UnityEditor;

/// <summary>
///     The MapEditor class updates the map whenever a change happens to it.
/// </summary>
[CustomEditor (typeof (MapManager))]
public class MapEditor : Editor {

    /// <summary>
    ///     Gets called every time a change happens inside the Inspector.
    /// </summary>
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        MapManager creator = (MapManager) target;
        creator.createMap();
    }

}
