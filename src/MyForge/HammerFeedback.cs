// 강화 판정 없이 망치 버튼의 확대와 3회 타격 연출만 관리한다.
sealed class HammerFeedback
{
    public const float HoverScale = 1.15f;
    public const double HitInterval = 1.55;
    public const double Duration = 4.3;
    private double _elapsed;

    public bool IsStriking { get; private set; }
    public float Scale { get; private set; } = 1;

    public bool TryStart()
    {
        if (IsStriking) return false;
        _elapsed = 0;
        IsStriking = true;
        Scale = 1;
        return true;
    }

    public void Update(double deltaTime, bool hovered)
    {
        double delta = Math.Max(0, deltaTime);
        if (IsStriking)
        {
            _elapsed += delta;
            if (_elapsed >= Duration) IsStriking = false;
        }
        float target = hovered && !IsStriking ? HoverScale : 1;
        Scale += (target - Scale) * (float)(1 - Math.Exp(-18 * delta));
    }

    // 음원의 타격 시작(0초, 1.55초, 3.1초)에 맞춰 내려갔다가 돌아온다.
    public float StrikeOffset
    {
        get
        {
            if (!IsStriking) return 0;
            double phase = _elapsed % HitInterval;
            return phase < 0.3 ? 42 * (float)Math.Pow(1 - phase / 0.3, 2) : 0;
        }
    }

    public void Reset()
    {
        IsStriking = false;
        _elapsed = 0;
        Scale = 1;
    }
}
