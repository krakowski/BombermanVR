/// <summary>
///     A HealthPowerUp which can be spawned inside the game.
/// </summary>
public class HealthPowerUp : PowerUp {

	public override void Start () {
        base.Start();
        item = new HealthItem();
	}
}
