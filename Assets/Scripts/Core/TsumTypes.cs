namespace PONPONLemon.Core
{
    public enum TsumType
    {
        Type1 = 0,
        Type2 = 1,
        Type3 = 2,
        Type4 = 3,
        Type5 = 4
    }
    
    public enum TsumState
    {
        Idle,
        Selected,
        Dropping,
        Popping
    }
    
    public enum GameState
    {
        Ready,
        Countdown,
        Playing,
        Paused,
        Fever,
        Result
    }
}
