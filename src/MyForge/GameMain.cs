// 교수님 템플릿의 생성 → 갱신 → 렌더 → 해제 순서를 사용한다.
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;

class GameMain : G2AppBase
{
    public override System.Drawing.Size ScreenSize => GameGlobal.ScreenSize;
    public override string GameName => GameGlobal.GameName;

    private enum Scene { Title, Main }
    private enum MenuButton { None, Start, Exit, Back }
    private Scene _scene = Scene.Title;
    private MenuButton _hovered;
    private Form? _window;
    private readonly HashSet<Keys> _heldKeys = new();
    private G2Texture? _titleBackground, _gameBackground, _anvil, _sword, _hammer;
    private G2Font? _displayFont, _headingFont, _bodyFont, _smallFont, _valueFont, _buttonFont;
    private ID2D1SolidColorBrush? _brush;

    private static readonly Rect StartBounds = new(64, 380, 260, 60);
    private static readonly Rect ExitBounds = new(64, 454, 260, 46);
    private static readonly Rect BackBounds = new(32, 570, 144, 44);
    private static readonly Rect EnhanceBounds = new(330, 556, 260, 58);
    private static readonly Color4 Gold = new(0.95f, 0.77f, 0.44f, 1);
    private static readonly Color4 White = new(0.96f, 0.93f, 0.86f, 1);
    private static readonly Color4 Muted = new(0.70f, 0.69f, 0.65f, 1);

    protected override void Initialize()
    {
        _titleBackground = new G2Texture("resource/images/title_background.png");
        _gameBackground = new G2Texture("resource/images/game_background.png");
        _anvil = new G2Texture("resource/images/anvil.png");
        _sword = new G2Texture("resource/images/sword.png");
        _hammer = new G2Texture("resource/images/hammer.png");
        _displayFont = new G2Font("Georgia", 66, FontWeight.Bold);
        _headingFont = new G2Font("Georgia", 25, FontWeight.Bold);
        _bodyFont = new G2Font("Malgun Gothic", 18, FontWeight.Normal);
        _smallFont = new G2Font("Malgun Gothic", 14, FontWeight.Normal);
        _valueFont = new G2Font("Georgia", 34, FontWeight.Bold);
        _buttonFont = new G2Font("Malgun Gothic", 20, FontWeight.Bold,
            textAlignment: TextAlignment.Center, paragraphAlignment: ParagraphAlignment.Center);
        _brush = RenderTarget.CreateSolidColorBrush(new Color4(0, 0, 0, 1));

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
    }

    protected override void Update()
    {
        // UI와 씬 이동만 구현한다. 강화 판정과 수치 변경은 아직 없다.
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
        Text(_headingFont, "30 / 30", new Rect(516, 54, 155, 34), White);
        Fill(new Rect(692, 36, 1, 43), new Color4(0.43f, 0.34f, 0.22f, 1));
        Text(_smallFont, "이번 판 최고 기록", new Rect(728, 31, 180, 24), Muted);
        Text(_headingFont, "+0", new Rect(728, 54, 150, 34), Gold);

        Panel(new Rect(326, 114, 224, 84));
        Text(_smallFont, "현재 강화 단계", new Rect(388, 123, 150, 24), Muted);
        Text(_valueFont, "+0", new Rect(413, 149, 120, 44), Gold);
        _anvil?.Draw(new Rect(252, 280, 352, 240), new Rect(0, 0, 440, 300));
        _sword?.Draw(new Rect(316, 266, 288, 57.6f), new Rect(0, 0, 360, 72));
        _hammer?.Draw(new Rect(476, 184, 144, 176), new Rect(0, 0, 180, 220));

        Panel(new Rect(704, 112, 232, 414));
        Text(_bodyFont, "강화 확률", new Rect(726, 135, 180, 30), Gold);
        RuleLine(726, 180, 188);
        ProbabilityRow("성공", 202, new Color4(0.55f, 0.82f, 0.64f, 1));
        ProbabilityRow("실패", 248, new Color4(0.91f, 0.74f, 0.46f, 1));
        ProbabilityRow("파괴", 294, new Color4(0.90f, 0.49f, 0.42f, 1));
        RuleLine(726, 350, 188);
        Text(_bodyFont, "강화 규칙", new Rect(726, 370, 185, 30), Gold);
        Text(_smallFont, "+0 ~ +4 : 실패 시 유지", new Rect(726, 411, 200, 25), White);
        Text(_smallFont, "+5 이상 : 실패 시 하락", new Rect(726, 442, 200, 25), White);
        Text(_smallFont, "파괴 시 +0부터 다시 도전", new Rect(726, 473, 200, 25), White);

        DrawButton(BackBounds, "시작 화면", MenuButton.Back);
        DrawButton(EnhanceBounds, "강화", MenuButton.None, true, false);
        Text(_smallFont, "최고 목표", new Rect(724, 553, 120, 24), Muted);
        Text(_valueFont, "+10", new Rect(818, 560, 100, 46), Gold);
        Text(_smallFont, "Esc : 시작 화면", new Rect(34, 617, 200, 22), Muted);
    }

    private void ProbabilityRow(string label, float y, Color4 color)
    {
        Fill(new Rect(726, y + 10, 6, 6), color);
        Text(_bodyFont, label, new Rect(744, y, 80, 30), White);
        // 단계별 확률은 아직 미정이다.
        Text(_bodyFont, "— %", new Rect(848, y, 66, 30), color);
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
        float x = e.X / ScaleX, y = e.Y / ScaleY;
        if (_scene == Scene.Title)
        {
            if (Contains(StartBounds, x, y)) return MenuButton.Start;
            if (Contains(ExitBounds, x, y)) return MenuButton.Exit;
        }
        else if (Contains(BackBounds, x, y)) return MenuButton.Back;
        return MenuButton.None;
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        _hovered = HitTest(e);
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
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Alt || e.Control || e.Shift || !_heldKeys.Add(e.KeyCode)) return;
        if (_scene == Scene.Title && e.KeyCode == Keys.Enter) ChangeScene(Scene.Main);
        else if (_scene == Scene.Main && e.KeyCode == Keys.Escape) ChangeScene(Scene.Title);
    }

    private void OnKeyUp(object? sender, KeyEventArgs e) => _heldKeys.Remove(e.KeyCode);
    private void OnDeactivate(object? sender, EventArgs e)
    {
        _heldKeys.Clear();
        ResetPointer(sender, e);
    }

    private void ChangeScene(Scene scene)
    {
        _scene = scene;
        ResetPointer(null, EventArgs.Empty);
    }

    private void ResetPointer(object? sender, EventArgs e)
    {
        _hovered = MenuButton.None;
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
        }
        _titleBackground?.Dispose();
        _gameBackground?.Dispose();
        _anvil?.Dispose();
        _sword?.Dispose();
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
