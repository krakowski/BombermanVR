/// <summary>
///     An item that heals a player.
/// </summary>
public class HealthItem : Item {

    public void useOn(GamePlayer player) {
        player.heal(1);
    }

}
