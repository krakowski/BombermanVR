using UnityEngine;
using UnityEngine.Networking;

/// <summary>
///     The Health class manages a specific players health.
/// </summary>
public class Health : NetworkBehaviour {

    //================================================================================
    // Network properties
    //================================================================================

    [HideInInspector]
    [SyncVar(hook = "OnHealthChanged")]
    public int currentHealth = maxHealth;

    //================================================================================
    // Prefab components (Inspector)
    //================================================================================

    [Header("UI References")]
    [Tooltip("Player HUD")]
    public HUD hud;

    //================================================================================
    // Private properties
    //================================================================================

    // Player max health
    private const int maxHealth = 3;

    //================================================================================
    // Logic
    //================================================================================

    /// <summary>
    ///     Indicates wether a Player is dead.
    /// </summary>
    /// <returns>true: Player is dead - false: Player is alive</returns>
    public bool isDead() {
        return currentHealth == 0;
    }

    /// <summary>
    ///     Applies specified amount of damage to the Player.
    /// </summary>
    /// <param name="amount">Amount of damage</param>
    public void damage(int amount) {
        // Only Server is allowed to damage players
        if (!isServer)
            return;

        currentHealth -= amount;

        if(currentHealth < 0) {
            currentHealth = 0;
        }
    }

    /// <summary>
    ///     Adds specified amount of lifes to the Players lifes.
    /// </summary>
    /// <param name="amount">Amount of lifes</param>
    public void heal(int amount) {
        // Only Server is allowed to heal players
        if (!isServer)
            return;

        if (currentHealth + amount > maxHealth)
            currentHealth = maxHealth;
        else
            currentHealth += amount;
    }

    //================================================================================
    // SyncVar Hooks
    //================================================================================

    /// <summary>
    ///     Called whenever Server changes currenHealth.
    /// </summary>
    /// <param name="newHealth">Players new health value</param>
    public void OnHealthChanged(int newHealth) {
        currentHealth = newHealth;
        hud.setLifes(currentHealth);
    }
}
