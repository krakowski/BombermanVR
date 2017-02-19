using UnityEngine;

/// <summary>
///     A crate which can be placed inside the map.
/// </summary>
public class Crate : MonoBehaviour, Explodable {


    //================================================================================
    // Prefab components (Inspector)
    //================================================================================

    [Header("Prefabs")]
    [Tooltip("Explosion Effect")]
    public ParticleSystem explosionEffect;

    //================================================================================
    // Logic
    //================================================================================

    public void explode() {
        // Remove Crate
        Destroy(gameObject);
        // Instantiate Explosion particles
        Destroy(Instantiate(explosionEffect.gameObject, gameObject.transform.position + Vector3.up, Quaternion.Euler(-90, 0, 0)) as GameObject, explosionEffect.main.startLifetime.constant);
    }
}
