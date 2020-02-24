public class ButtonInput
{
    public bool LP { get; set; }
    public bool RP { get; set; }
    public bool LK { get; set; }
    public bool RK { get; set; }
    
    public bool WhenHit { get; set; }
    public bool Cancel { get; set; }

    public void SetButton(char d)
    {
        switch (d)
        {
            case '1': LP = true; break;
            case '2': RP = true; break;
            case '3': LK = true; break;
            case '4': RK = true; break;
            default: throw new System.ArgumentException($"Cound't parse button input '{d}'");
        }
    }
}