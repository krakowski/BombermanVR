using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     A heart shown inside the lifebar.
/// </summary>
public class Heart : MonoBehaviour {

    //================================================================================
    // Private properties
    //================================================================================

    // This hearts RectTransform
    private RectTransform rectTransform;

    // This hearts Image
    private Image image;

    //================================================================================
    // Logic
    //================================================================================

    /// <summary>
    ///     Sets the hearts size to the specified size.
    /// </summary>
    /// <param name="size">New heart size</param>
    public void setSize(Vector2 size) {
        rectTransform.sizeDelta = size;
    }

    /// <summary>
    ///     Hides/Shows this heart.
    /// </summary>
    /// <param name="visibility">true: show heart - false: hide heart</param>
    public void setImageVisible(bool visibility) {
        image.enabled = visibility;
    }

    //================================================================================
    // Unity
    //================================================================================

    void Start () {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
	} 
}
