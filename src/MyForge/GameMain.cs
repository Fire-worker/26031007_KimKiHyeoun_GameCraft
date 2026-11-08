// 교수님 템플릿의 생성 → 갱신 → 렌더 → 해제 순서를 사용한다.
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;

class GameMain : G2AppBase
{
    public override System.Drawing.Size ScreenSize => GameGlobal.ScreenSize;
    public override string GameName => GameGlobal.GameName;

    private enum Scene { Title, Main }
    private enum MenuButton { None, Start, Exit, Back, Hammer, Retry }
    private Scene _scene = Scene.Title;
    private MenuButton _hovered;
    private Form? _window;
    private readonly HashSet<Keys> _heldKeys = new();
    private G2Texture? _titleBackground, _gameBackground, _anvil, _hammer;
    private readonly G2Texture[] _swords = new G2Texture[11];
    private readonly SwordLight[] _lights = new SwordLight[11];
    private readonly G2Texture[] _pieces = new G2Texture[3];
    private readonly ForgeGame _game = new();
    private bool _active = true;
    private G2Font? _displayFont, _headingFont, _bodyFont, _smallFont, _valueFont, _buttonFont;
    private ID2D1SolidColorBrush? _brush;
    private ForgeAudio? _audio;
    private readonly HammerFeedback _hammerFeedback = new();
    private System.Drawing.PointF? _pointer;

    private static readonly Rect StartBounds = new(64, 380, 260, 60);
    private static readonly Rect ExitBounds = new(64, 454, 260, 46);
    private static readonly Rect BackBounds = new(32, 570, 144, 44);
    private static readonly Rect HammerBounds = new(476, 184, 144, 176);
    private static readonly Rect RetryBounds = new(350, 415, 260, 58);
    private static readonly Rect ResultBackBounds = new(350, 490, 260, 46);
    private static readonly Color4 Gold = new(0.95f, 0.77f, 0.44f, 1);
    private static readonly Color4 White = new(0.96f, 0.93f, 0.86f, 1);
    private static readonly Color4 Muted = new(0.70f, 0.69f, 0.65f, 1);

    protected override void Initialize()
    {
        _titleBackground = new G2Texture("resource/images/title_background.png");
        _gameBackground = new G2Texture("resource/images/game_background.png");
        _anvil = new G2Texture("resource/images/anvil.png");
        for (int i=0; i<11; i++)
        {
            string path=$"resource/images/swords/sword_{i:00}.png";
            _swords[i]=new G2Texture(path);
            _lights[i]=new SwordLight(RenderTarget,path);
        }
        _pieces[0]=new G2Texture("resource/images/sword_hilt.png");
        _pieces[1]=new G2Texture("resource/images/sword_middle.png");
        _pieces[2]=new G2Texture("resource/images/sword_tip.png");
        _hammer = new G2Texture("resource/images/hammer.png");
        _displayFont = new G2Font("Georgia", 66, FontWeight.Bold);
        _headingFont = new G2Font("Georgia", 25, FontWeight.Bold);
        _bodyFont = new G2Font("Malgun Gothic", 18, FontWeight.Normal);
        _smallFont = new G2Font("Malgun Gothic", 14, FontWeight.Normal);
        _valueFont = new G2Font("Georgia", 34, FontWeight.Bold);
        _buttonFont = new G2Font("Malgun Gothic", 20, FontWeight.Bold,
            textAlignment: TextAlignment.Center, paragraphAlignment: ParagraphAlignment.Center);
        _brush = RenderTarget.CreateSolidColorBrush(new Color4(0, 0, 0, 1));
        _audio = new ForgeAudio();

        // GameProject와 같이 창 이벤트로 짧은 입력도 받는다.
        _window = Control.FromHandle(RenderTarget.Hwnd) as Form;
        if (_window == null) return;
        _window.MouseMove += OnMouseMove;
        _window.MouseClick += OnMouseClick;
        _window.MouseLeave += ResetPointer;
        _window.Resize += ResetPointer;
        _window.KeyDown += OnKeyDown;
        _window.KeyUp += OnKeyUp;
        _window.Deactivate += OnDeactivate;
        _window.Activated += OnActivated;
    }

    protected override void Update()
    {
        if (_scene != Scene.Main || !_active) return;
        _hammerFeedback.Update(DeltaTime, _hovered == MenuButton.Hammer);
        if (_game.Update(DeltaTime, () => Random.Shared.Next(100))) _audio?.PlayResult(_game.Result);
        RefreshPointer();
    }

    protected override void Render()
    {
        if (_scene == Scene.Title) DrawTitle();
        else DrawMain();
    }

    private void DrawTitle()
    {
        _titleBackground?.Draw();
        // 메뉴 쪽만 어둡게 하여 배경 위에서도 글씨가 읽히도록 한다.
        for (int i = 0; i < 24; i++)
            Fill(new Rect(i * 24, 0, 24, 640), new Color4(0.015f, 0.025f, 0.05f, 0.52f * (1 - i / 24f)));
        Text(_smallFont, "무기 강화 시뮬레이션", new Rect(66, 140, 330, 28), Gold);
        Text(_displayFont, "MY FORGE", new Rect(60, 178, 540, 90), White);
        Fill(new Rect(66, 278, 68, 3), Gold);
        Text(_bodyFont, "30번의 기회, +10을 향한 도전.", new Rect(66, 306, 390, 32), White);
        DrawButton(StartBounds, "게임 시작", MenuButton.Start, true);
        DrawButton(ExitBounds, "종료", MenuButton.Exit);
        Text(_smallFont, "마우스로 선택  ·  Enter로 시작", new Rect(66, 524, 360, 28), Muted);
    }

    private void DrawMain()
    {
        // 추후 글자를 UI 이미지 렌더를 통해 구현한다.
        _gameBackground?.Draw();
        Panel(new Rect(24, 20, 912, 74));
        Text(_headingFont, "MY FORGE", new Rect(44, 30, 240, 35), Gold);
        Text(_smallFont, "대장간", new Rect(46, 65, 200, 22), Muted);
        Text(_smallFont, "남은 시도", new Rect(516, 31, 130, 24), Muted);
        Text(_headingFont, $"{_game.Remaining} / 30", new Rect(516, 54, 155, 34), White);
        Fill(new Rect(692, 36, 1, 43), new Color4(0.43f, 0.34f, 0.22f, 1));
        Text(_smallFont, "이번 판 최고 기록", new Rect(728, 31, 180, 24), Muted);
        Text(_headingFont, $"+{_game.Highest}", new Rect(728, 54, 150, 34), Gold);

        Panel(new Rect(326, 114, 224, 84));
        Text(_smallFont, "현재 강화 단계", new Rect(388, 123, 150, 24), Muted);
        Text(_valueFont, $"+{_game.Level}", new Rect(413, 149, 120, 44), Gold);
        _anvil?.Draw(new Rect(252, 280, 352, 240), new Rect(0, 0, 440, 300));
        DrawSword();
        float scale = _hammerFeedback.Scale;
        _hammer?.Draw(new Rect(
            HammerBounds.X - HammerBounds.Width * (scale - 1) / 2,
            HammerBounds.Y - HammerBounds.Height * (scale - 1) / 2 + _hammerFeedback.StrikeOffset,
            HammerBounds.Width * scale, HammerBounds.Height * scale), new Rect(0, 0, 180, 220));

        Panel(new Rect(704, 112, 232, 414));
        Text(_bodyFont, "강화 확률", new Rect(726, 135, 180, 30), Gold);
        RuleLine(726, 180, 188);
        ProbabilityRow("성공", _game.Chance.Success, 202, new Color4(0.55f, 0.82f, 0.64f, 1));
        ProbabilityRow("실패", _game.Chance.Failure, 248, new Color4(0.91f, 0.74f, 0.46f, 1));
        ProbabilityRow("파괴", _game.Chance.Destroyed, 294, new Color4(0.90f, 0.49f, 0.42f, 1));
        RuleLine(726, 350, 188);
        Text(_bodyFont, "강화 규칙", new Rect(726, 370, 185, 30), Gold);
        Text(_smallFont, "+0 ~ +4 : 실패 시 유지", new Rect(726, 411, 200, 25), White);
        Text(_smallFont, "+5 이상 : 실패 시 하락", new Rect(726, 442, 200, 25), White);
        Text(_smallFont, "파괴 시 +0부터 다시 도전", new Rect(726, 473, 200, 25), White);

        DrawButton(BackBounds, "시작 화면", MenuButton.Back);
        string status = _game.Phase switch
        {
            ForgePhase.Striking => "두드리는 중...",
            ForgePhase.Charging => "빛이 모이고 있습니다...",
            ForgePhase.Revealing => _game.Result switch
            { ForgeResult.Success => "강화 성공!", ForgeResult.Failure => "강화 실패", _ => "무기 파괴" },
            ForgePhase.Finished => "도전 종료",
            _ => "망치를 눌러 강화"
        };
        Text(_buttonFont, status, new Rect(270, 540, 410, 42), Gold);
        string detail = _game.Phase == ForgePhase.Revealing
            ? _game.Result switch
            { ForgeResult.Success => $"+{_game.PreviousLevel} → +{_game.Level}",
              ForgeResult.Failure => _game.Level == _game.PreviousLevel ? "강화 단계가 유지되었습니다" : $"+{_game.PreviousLevel} → +{_game.Level}",
              _ => "+0 검으로 다시 도전합니다" }
            : "망치 클릭 : 강화  ·  최대 30회 도전";
        Text(_smallFont, detail, new Rect(300, 590, 380, 25), White);
        Text(_smallFont, "최고 목표", new Rect(724, 553, 120, 24), Muted);
        Text(_valueFont, "+10", new Rect(818, 560, 100, 46), Gold);
        Text(_smallFont, "Esc : 시작 화면", new Rect(34, 617, 200, 22), Muted);
        if (_game.Phase == ForgePhase.Finished) DrawGameOver();
    }

    private void DrawSword()
    {
        float progress = (float)Math.Clamp(_game.Elapsed / ForgeGame.ChargeDuration,0,1);
        float scale = _game.Phase == ForgePhase.Charging ? 1 + .28f * progress : 1;
        if (_game.Phase == ForgePhase.Revealing)
            scale += .28f * Math.Max(0,1-(float)_game.Elapsed*4);
        float x=460-144*scale, y=294.8f-28.8f*scale;
        if (_game.Phase == ForgePhase.Revealing && _game.Result == ForgeResult.Failure)
            x += MathF.Sin((float)_game.Elapsed*45)*9*MathF.Exp(-(float)_game.Elapsed*3);
        var bounds=new Rect(x,y,288*scale,57.6f*scale);
        if (_game.Phase == ForgePhase.Revealing && _game.Result == ForgeResult.Destroyed && _game.Elapsed < 1)
        {
            float t=(float)_game.Elapsed;
            for(int i=0;i<3;i++)
                _pieces[i].Draw(new Rect(320+i*92+(i-1)*80*t,280-90*t+160*t*t,89.6f,i==0?48:22.4f),
                    new Rect(0,0,i==1?104:112,i==0?60:28),1-t);
        }
        else _swords[_game.Level].Draw(bounds,new Rect(0,0,360,72));
        if (_game.Phase == ForgePhase.Charging)
            _lights[_game.Level].Draw(RenderTarget,bounds,Math.Min(1,progress*1.8f));
        if (_game.Phase == ForgePhase.Revealing && _game.Result == ForgeResult.Success)
        {
            float t=(float)_game.Elapsed;
            _lights[_game.Level].Draw(RenderTarget,bounds,Math.Max(0,1-t*2));
            _brush!.Color=new Color4(1,.83f,.35f,Math.Max(0,1-t/1.3f));
            for(int i=0;i<12;i++)
            {
                float a=i*MathF.PI/6, r=28+100*t;
                float px=460+MathF.Cos(a)*r, py=295+MathF.Sin(a)*r*.55f;
                RenderTarget.DrawLine(new System.Numerics.Vector2(px,py),
                    new System.Numerics.Vector2(px+MathF.Cos(a)*9,py+MathF.Sin(a)*9),_brush,2);
            }
        }
    }

    private void DrawGameOver()
    {
        Fill(new Rect(0,0,960,640),new Color4(0,0,0,.72f));
        Panel(new Rect(240,160,480,400));
        Text(_buttonFont,_game.Level==10?"+10 달성!":"도전 종료",new Rect(280,195,400,45),Gold);
        Text(_buttonFont,$"이번 판 최고 기록  +{_game.Highest}",new Rect(280,270,400,45),White);
        Text(_buttonFont,$"사용한 시도  {30-_game.Remaining} / 30",new Rect(280,335,400,35),Muted);
        DrawButton(RetryBounds,"다시 도전",MenuButton.Retry,true);
        DrawButton(ResultBackBounds,"시작 화면",MenuButton.Back);
    }

    private void ProbabilityRow(string label, int percent, float y, Color4 color)
    {
        Fill(new Rect(726, y + 10, 6, 6), color);
        Text(_bodyFont, label, new Rect(744, y, 80, 30), White);
        Text(_bodyFont, $"{percent} %", new Rect(840, y, 76, 30), color);
    }

    private void DrawButton(Rect bounds, string label, MenuButton button, bool primary = false, bool enabled = true)
    {
        bool hover = enabled && _hovered == button;
        Color4 edge = enabled ? (hover ? White : Gold) : new Color4(0.39f, 0.37f, 0.33f, 1);
        Color4 background = !enabled ? new Color4(0.14f, 0.13f, 0.12f, 0.96f)
            : primary ? new Color4(hover ? 0.70f : 0.55f, hover ? 0.43f : 0.31f, 0.12f, 0.98f)
            : new Color4(0.055f, 0.07f, 0.09f, hover ? 0.98f : 0.88f);
        Fill(bounds, edge);
        Fill(new Rect(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2), background);
        Text(_buttonFont, label, bounds, enabled ? White : Muted);
    }

    private void Panel(Rect bounds)
    {
        Fill(bounds, new Color4(0.36f, 0.28f, 0.17f, 0.94f));
        Fill(new Rect(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2),
            new Color4(0.045f, 0.04f, 0.035f, 0.91f));
    }

    private void RuleLine(float x, float y, float width) =>
        Fill(new Rect(x, y, width, 1), new Color4(0.38f, 0.30f, 0.20f, 1));

    private void Fill(Rect bounds, Color4 color)
    {
        _brush!.Color = color;
        RenderTarget.FillRectangle(bounds, _brush);
    }

    private static void Text(G2Font? font, string text, Rect bounds, Color4 color) => font?.DrawText(text, bounds, color);
    private static bool Contains(Rect bounds, float x, float y) =>
        x >= bounds.X && x < bounds.X + bounds.Width && y >= bounds.Y && y < bounds.Y + bounds.Height;

    private MenuButton HitTest(MouseEventArgs e)
    {
        if (ScaleX <= 0 || ScaleY <= 0) return MenuButton.None;
        float x = e.X / ScaleX, y = e.Y / ScaleY;
        return HitTest(x, y);
    }

    private MenuButton HitTest(float x, float y)
    {
        if (_scene == Scene.Title)
        {
            if (Contains(StartBounds, x, y)) return MenuButton.Start;
            if (Contains(ExitBounds, x, y)) return MenuButton.Exit;
        }
        else
        {
            if (_game.Phase == ForgePhase.Finished)
            {
                if (Contains(RetryBounds,x,y)) return MenuButton.Retry;
                if (Contains(ResultBackBounds,x,y)) return MenuButton.Back;
                return MenuButton.None;
            }
            if (Contains(BackBounds, x, y)) return MenuButton.Back;
            // 확대 중에도 판정 영역을 고정하여 가장자리에서 깜빡이지 않게 한다.
            var bounds = new Rect(HammerBounds.X - 11, HammerBounds.Y - 14,
                HammerBounds.Width + 22, HammerBounds.Height + 28);
            if (_game.CanAttempt && Contains(bounds, x, y)) return MenuButton.Hammer;
        }
        return MenuButton.None;
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (ScaleX <= 0 || ScaleY <= 0) return;
        _pointer = new System.Drawing.PointF(e.X / ScaleX, e.Y / ScaleY);
        RefreshPointer();
    }

    private void RefreshPointer()
    {
        _hovered = _pointer is { } p ? HitTest(p.X, p.Y) : MenuButton.None;
        if (_window != null) _window.Cursor = _hovered == MenuButton.None ? Cursors.Default : Cursors.Hand;
    }

    private void OnMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        switch (HitTest(e))
        {
            case MenuButton.Start: ChangeScene(Scene.Main); break;
            case MenuButton.Back: ChangeScene(Scene.Title); break;
            case MenuButton.Exit: Close(); break;
            case MenuButton.Retry: StartRound(); break;
            case MenuButton.Hammer:
                if (_game.TryStart()) { _hammerFeedback.TryStart(); _audio?.PlayAttempt(); }
                RefreshPointer();
                break;
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Alt || e.Control || e.Shift || !_heldKeys.Add(e.KeyCode)) return;
        if (_scene == Scene.Title && e.KeyCode == Keys.Enter) ChangeScene(Scene.Main);
        else if (_scene == Scene.Main && _game.Phase == ForgePhase.Finished && e.KeyCode == Keys.Enter) StartRound();
        else if (_scene == Scene.Main && e.KeyCode == Keys.Escape) ChangeScene(Scene.Title);
    }

    private void OnKeyUp(object? sender, KeyEventArgs e) => _heldKeys.Remove(e.KeyCode);
    private void OnDeactivate(object? sender, EventArgs e)
    {
        _heldKeys.Clear();
        _active = false;
        _audio?.StopAll();
        ResetPointer(sender, e);
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        _active = true;
        if (_scene == Scene.Main && _game.Phase == ForgePhase.Striking)
        {
            _game.RestartStrike(); _hammerFeedback.Reset(); _hammerFeedback.TryStart();
            _audio?.PlayAttempt();
        }
        if (_scene == Scene.Main) _audio?.StartAmbience();
    }

    private void ChangeScene(Scene scene)
    {
        _scene = scene;
        if (scene == Scene.Main) _game.Reset();
        _hammerFeedback.Reset();
        _audio?.PlaySceneTransition(scene == Scene.Main);
        ResetPointer(null, EventArgs.Empty);
    }

    private void StartRound()
    {
        _game.Reset(); _hammerFeedback.Reset();
        _audio?.PlaySceneTransition(true);
        ResetPointer(null,EventArgs.Empty);
    }

    private void ResetPointer(object? sender, EventArgs e)
    {
        _hovered = MenuButton.None;
        _pointer = null;
        if (_window != null) _window.Cursor = Cursors.Default;
    }

    public override void Dispose()
    {
        if (_window != null)
        {
            _window.MouseMove -= OnMouseMove;
            _window.MouseClick -= OnMouseClick;
            _window.MouseLeave -= ResetPointer;
            _window.Resize -= ResetPointer;
            _window.KeyDown -= OnKeyDown;
            _window.KeyUp -= OnKeyUp;
            _window.Deactivate -= OnDeactivate;
            _window.Activated -= OnActivated;
        }
        _audio?.Dispose();
        _titleBackground?.Dispose();
        _gameBackground?.Dispose();
        _anvil?.Dispose();
        foreach(var sword in _swords) sword?.Dispose();
        foreach(var light in _lights) light?.Dispose();
        foreach(var piece in _pieces) piece?.Dispose();
        _hammer?.Dispose();
        _displayFont?.Dispose();
        _headingFont?.Dispose();
        _bodyFont?.Dispose();
        _smallFont?.Dispose();
        _valueFont?.Dispose();
        _buttonFont?.Dispose();
        _brush?.Dispose();
        base.Dispose();
    }
}
