using System.Text.Json.Serialization;

[JsonConverter(typeof(AsRuntimeTypeConverter<Move>))]
public class Move
{
    public Character Character { get; set; }
    public int Damage { get; set; }
    public int StartUpFrame { get; set; }
    public FramesAdvantage BlockFrame { get; set; }
    public FramesAdvantage HitFrame { get; set; }
    public FramesAdvantage CounterHitFrame { get; set; }
    public MoveProperties MoveProperties { get; set; }
    public string Notes { get; set; }
}