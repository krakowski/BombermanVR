using UnityEngine;

/// <summary>
///     Base class for all spawnable PowerUps.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class PowerUp : MonoBehaviour {

    //================================================================================
    // Prefab components
    //================================================================================

    [Range(1, 4)]
    [Tooltip("Speed at which Powerups are rotated")]
    public int rotationSpeed = 1;

    //================================================================================
    // Private/Protected properties
    //================================================================================

    // PowerUpSpawner that spwaned this PowerUp
    private PowerUpSpawner spawner;

    // Item containing PowerUp Logic
    protected Item item;

    //================================================================================
    // Unity
    //================================================================================

    // Let subclasses override/extend method
    public virtual void Start() {
        spawner = GetComponentInParent<PowerUpSpawner>();
    }

    void Update() {
        // Rotate 90 degrees per second/rotationSpeed
        float rotationDelta = Time.deltaTime * 90 * rotationSpeed;
        transform.Rotate(new Vector3(0, rotationDelta, 0));
    }

    private void OnTriggerEnter(Collider other) {
        // PowerUps can only be used on GamePlayers
        if (!other.gameObject.tag.Equals("GamePlayer"))
            return;

        GamePlayer player = other.gameObject.GetComponent<GamePlayer>();
        item.useOn(player);
        // PowerUpSpawners only exist on Server
        if(spawner != null)
            spawner.pickUp();
        Destroy(gameObject);
    }
}
