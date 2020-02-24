public class FramesAdvantage
{
    public int Frames { get; set; }
    public AdvantageType AdvantageType { get; set; }
}
public enum AdvantageType
{
    Normal,
    Knockdown,
    Launch
    //TODO: launch and knockdown kinds?
}