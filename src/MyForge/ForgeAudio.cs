// 게임에서 사용하는 효과음. 파일 음량을 맞춘 뒤 재생 볼륨을 70%로 설정한다.
sealed class ForgeAudio : IDisposable
{
    public const float EffectsVolume = 0.7f;
    private readonly G2AudioSound? _sceneTransition;
    private readonly G2AudioSound? _ambience;
    private readonly G2AudioSound? _attempt;
    private readonly G2AudioSound? _success, _failure;

    public ForgeAudio()
    {
        // 출력 장치가 없는 환경에서도 게임 화면을 확인할 수 있도록 한다.
        if (G2AudioContext.Instance?.Audio == null) return;
        G2AudioContext.Instance.MasteringVoice?.SetVolume(EffectsVolume);
        _sceneTransition = new G2AudioSound("resource/audio/scene_transition.wav");
        _ambience = new G2AudioSound("resource/audio/forge_ambience.wav");
        _attempt = new G2AudioSound("resource/audio/attempt_three_hits.wav");
        _success = new G2AudioSound("resource/audio/success.wav");
        _failure = new G2AudioSound("resource/audio/failure.wav");
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
        // 다음 타격을 눌러도 성공음 뒤에 이어지는 보상음은 끝까지 들려준다.
        if (_attempt?.IsPlaying() == false) _attempt.Play();
    }

    public void PlayResult(ForgeResult result)
    {
        _attempt?.Stop(); _success?.Stop(); _failure?.Stop();
        if (result == ForgeResult.Success) _success?.Play();
        else _failure?.Play();
    }

    public void StopAll()
    {
        _sceneTransition?.Stop();
        _attempt?.Stop();
        _ambience?.Stop();
        _success?.Stop(); _failure?.Stop();
    }

    public void Dispose()
    {
        StopAll();
        _sceneTransition?.Dispose();
        _attempt?.Dispose();
        _ambience?.Dispose();
        _success?.Dispose(); _failure?.Dispose();
    }
}
