enum ForgePhase { Ready, Striking, Charging, Revealing, Finished }
enum ForgeResult { None, Success, Failure, Destroyed }
readonly record struct ForgeChance(int Success, int Failure, int Destroyed);

// 확률과 한 판의 상태를 화면·음향과 분리한다.
sealed class ForgeGame
{
    public const int MaxAttempts = 30;
    public const double ChargeDuration = 1.25;
    public const double RevealDuration = 1.65;
    private static readonly ForgeChance[] Chances =
    [new(95,5,0), new(90,10,0), new(85,15,0), new(80,20,0), new(75,25,0),
     new(65,30,5), new(55,35,10), new(45,40,15), new(35,45,20), new(25,50,25)];
    public int Level { get; private set; }
    public int PreviousLevel { get; private set; }
    public int Highest { get; private set; }
    public int Remaining { get; private set; } = MaxAttempts;
    public ForgePhase Phase { get; private set; }
    public ForgeResult Result { get; private set; }
    public double Elapsed { get; private set; }
    public ForgeChance Chance => Chances[Math.Min(Level,9)];
    public bool CanAttempt => Phase == ForgePhase.Ready && Remaining > 0 && Level < 10;
    public bool TryStart()
    {
        if (!CanAttempt) return false;
        Remaining--;
        PreviousLevel = Level;
        Result = ForgeResult.None;
        Phase = ForgePhase.Striking;
        Elapsed = 0;
        return true;
    }
    // true는 이번 프레임에 결과가 확정되었음을 뜻한다.
    public bool Update(double delta, Func<int> roll)
    {
        if (Phase is ForgePhase.Ready or ForgePhase.Finished) return false;
        Elapsed += Math.Max(0,delta);
        if (Phase == ForgePhase.Striking && Elapsed >= HammerFeedback.Duration)
        { Phase = ForgePhase.Charging; Elapsed = 0; }
        else if (Phase == ForgePhase.Charging && Elapsed >= ChargeDuration)
        {
            var chance = Chance;
            int value = roll();
            if (value < 0 || value >= 100) throw new ArgumentOutOfRangeException(nameof(roll));
            Result = value < chance.Success ? ForgeResult.Success
                : value < chance.Success + chance.Failure ? ForgeResult.Failure : ForgeResult.Destroyed;
            Level = Result switch
            {
                ForgeResult.Success => Level + 1,
                ForgeResult.Failure => Level >= 5 ? Level - 1 : Level,
                _ => 0
            };
            Highest = Math.Max(Highest, Level);
            Phase = ForgePhase.Revealing; Elapsed = 0;
            return true;
        }
        else if (Phase == ForgePhase.Revealing && Elapsed >= RevealDuration)
        { Phase = Remaining == 0 || Level == 10 ? ForgePhase.Finished : ForgePhase.Ready; Elapsed = 0; }
        return false;
    }
    public void RestartStrike()
    {
        if (Phase == ForgePhase.Striking) Elapsed = 0;
    }
    public void Reset()
    {
        Level = PreviousLevel = Highest = 0;
        Remaining = MaxAttempts;
        Phase = ForgePhase.Ready;
        Result = ForgeResult.None;
        Elapsed = 0;
    }
}
