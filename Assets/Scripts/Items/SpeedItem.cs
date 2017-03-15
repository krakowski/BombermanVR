/// <summary>
///     An item that speeds up a player.
/// </summary>
public class SpeedItem : Item {

    public void useOn(GamePlayer player) {
        // SpeedUp for 10 seconds
        player.speedUp(10f);
    }

}
