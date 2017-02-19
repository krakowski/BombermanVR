/// <summary>
///     A SpeedPowerUp which can be spawned inside the game.
/// </summary>
public class SpeedPowerUp : PowerUp {

    public override void Start() {
        base.Start();
        item = new SpeedItem();
    }
}
