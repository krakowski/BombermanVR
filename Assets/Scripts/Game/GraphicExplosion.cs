using UnityEngine;

/// <summary>
///     The GraphicExplosion class generates a volumetric explosion
///     based on a custom shader.
/// </summary>
public class GraphicExplosion : MonoBehaviour {

    //================================================================================
    // Prefab Components (Inspector)
    //================================================================================

    [Header("Properties")]
    [Tooltip("Duration for one loop")]
    public float loopduration;

    [Tooltip("Explosion life time")]
    public float lifeTime = 3.0f;

    [Tooltip("Maximum Explosion size")]
    public Vector3 maxSize = new Vector3(2.0f, 2.0f, 2.0f);
    [Space(10)]

    [Header("References")]
    [Tooltip("ExplosionModel")]
    public Transform explosionModel;

    //================================================================================
    // Prefab Components (Inspector)
    //================================================================================

    // Ramp position (0 -> 1)
    private float ramptime=0;

    // Alpha value (1 -> 0)
    private float alphatime=1;

    // Time Explosion was created
    private float time;

    // Hide deprecated renderer
    private new Renderer renderer;

    //================================================================================
    // Logic
    //================================================================================

    private void Start() {
        renderer = explosionModel.GetComponent<Renderer>();
        time = Time.time;
    }

    void Update () {

        // Increase Explosion size
        explosionModel.localScale = Vector3.Lerp(explosionModel.localScale, maxSize, Time.deltaTime);

        // Move further on ramp
        ramptime += Time.deltaTime*2;

        // Reduce alpha value
        alphatime -= Time.deltaTime;
        
        // Modify rgb		
        float r = Mathf.Sin((Time.time / loopduration) * (2 * Mathf.PI)) * 0.5f + 0.25f;
        float g = Mathf.Sin((Time.time / loopduration + 0.33333333f) * 2 * Mathf.PI) * 0.5f + 0.25f;
        float b = Mathf.Sin((Time.time / loopduration + 0.66666667f) * 2 * Mathf.PI) * 0.5f + 0.25f;
        float correction = 1 / (r + g + b);
        r *= correction;
        g *= correction;
        b *= correction;

        // Change Material Properties
        renderer.material.SetVector("_ChannelFactor", new Vector4(r,g,b,0));
        renderer.material.SetVector("_Range", new Vector4(ramptime,0,0,0));
        renderer.material.SetFloat("_ClipRange", alphatime);

        if (Time.time > time + lifeTime)
            NetworkPoolManager.instance.Destroy(gameObject);
	}

    private void OnEnable() {
        // Reset to Initial State
        time = Time.time;
        explosionModel.localScale = new Vector3(.1f, .1f, .1f);
        ramptime = 0;
        alphatime = 1;
    }
}
