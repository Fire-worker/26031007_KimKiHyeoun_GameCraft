// 게임에서 사용하는 효과음. 파일 음량을 맞춘 뒤 재생 볼륨을 70%로 설정한다.
sealed class ForgeAudio : IDisposable
{
    public const float EffectsVolume = 0.7f;
    private readonly G2AudioSound? _sceneTransition;

    public ForgeAudio()
    {
        // 출력 장치가 없는 환경에서도 게임 화면을 확인할 수 있도록 한다.
        if (G2AudioContext.Instance?.Audio == null) return;
        G2AudioContext.Instance.MasteringVoice?.SetVolume(EffectsVolume);
        _sceneTransition = new G2AudioSound("resource/audio/scene_transition.wav");
    }

    public void PlaySceneTransition()
    {
        StopAll();
        _sceneTransition?.Play();
    }

    public void StopAll()
    {
        _sceneTransition?.Stop();
    }

    public void Dispose()
    {
        StopAll();
        _sceneTransition?.Dispose();
    }
}
