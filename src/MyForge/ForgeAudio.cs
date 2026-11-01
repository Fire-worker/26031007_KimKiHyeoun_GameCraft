// 게임에서 사용하는 효과음. 파일 음량을 맞춘 뒤 재생 볼륨을 70%로 설정한다.
sealed class ForgeAudio : IDisposable
{
    public const float EffectsVolume = 0.7f;
    private readonly G2AudioSound? _sceneTransition;
    private readonly G2AudioSound? _ambience;
    private readonly G2AudioSound? _attempt;

    public ForgeAudio()
    {
        // 출력 장치가 없는 환경에서도 게임 화면을 확인할 수 있도록 한다.
        if (G2AudioContext.Instance?.Audio == null) return;
        G2AudioContext.Instance.MasteringVoice?.SetVolume(EffectsVolume);
        _sceneTransition = new G2AudioSound("resource/audio/scene_transition.wav");
        _ambience = new G2AudioSound("resource/audio/forge_ambience.wav");
        _attempt = new G2AudioSound("resource/audio/attempt_three_hits.wav");
    }

    public void PlaySceneTransition(bool enterForge)
    {
        StopAll();
        _sceneTransition?.Play();
        if (enterForge) StartAmbience();
    }

    public void StartAmbience()
    {
        if (_ambience?.IsPlaying() == false) _ambience.Play(isLooping: true);
    }

    public void PlayAttempt()
    {
        if (_attempt?.IsPlaying() == false) _attempt.Play();
    }

    public void StopAll()
    {
        _sceneTransition?.Stop();
        _attempt?.Stop();
        _ambience?.Stop();
    }

    public void Dispose()
    {
        StopAll();
        _sceneTransition?.Dispose();
        _attempt?.Dispose();
        _ambience?.Dispose();
    }
}
