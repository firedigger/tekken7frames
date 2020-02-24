using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Move
{
    public IEnumerable<HitLevel> HitLevels { get; set; }
    public IEnumerable<Command> Commands { get; set; }
    public Character Character { get; set; }
    public int Damage { get; set; }
    public int StartUpFrame { get; set; }
    public FramesAdvantage BlockFrame { get; set; }
    public FramesAdvantage HitFrame { get; set; }
    public FramesAdvantage CounterHitFrame { get; set; }
    public MoveProperties MoveProperties { get; set; }
    public string Notes { get; set; }
    public string Stance { get; set; }
}