using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace GenICamCameraCapture.UI;

/// <summary>
/// 3圈同心圆焊点选择控件。
/// 每圈 72 个槽位，共 216 个（外圈 0-71 / 中圈 72-143 / 内圈 144-215）。
/// 状态：NotPresent（灰色，不可点）/ Unselected（橙色）/ Selected（绿色）。
/// </summary>
public sealed class JointCirclePanel : Panel
{
    public enum JointState { NotPresent, Unselected, Selected }

    public const int SlotsPerRing = 72;
    public const int RingCount    = 3;
    public const int TotalSlots   = SlotsPerRing * RingCount; // 216

    // 三圈半径比例（相对于有效半径）
    private static readonly float[] RingRadFrac = { 0.88f, 0.60f, 0.34f };

    private readonly JointState[] _states = new JointState[TotalSlots];
    private float _nr;   // 当前节点半径（OnPaint 时计算）

    private readonly ToolTip _tip = new() { InitialDelay = 300, ShowAlways = true };

    /// <summary>节点被点击后触发，参数为 0-based slot 索引。</summary>
    public event EventHandler<int>? JointToggled;

    public JointCirclePanel()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer, true);
        BackColor   = Color.FromArgb(22, 24, 32);
        MinimumSize = new Size(240, 240);
        MouseMove  += OnMouseMoveTooltip;
    }

    // ── 公开 API ──────────────────────────────────────────────

    /// <summary>
    /// 传入检测到的焊点 slot ID（1-based，1..216）。
    /// 不在列表中的槽位 → NotPresent；列表中原为 NotPresent 的槽位默认变为 Selected。
    /// </summary>
    public void SetPresentJoints(IEnumerable<int> presentIds)
    {
        var set = new HashSet<int>(presentIds);
        for (int i = 0; i < TotalSlots; i++)
        {
            bool present = set.Contains(i + 1);
            if (!present)
                _states[i] = JointState.NotPresent;
            else if (_states[i] == JointState.NotPresent)
                _states[i] = JointState.Selected;
        }
        Invalidate();
    }

    public void SelectAll()
    {
        for (int i = 0; i < TotalSlots; i++)
            if (_states[i] != JointState.NotPresent)
                _states[i] = JointState.Selected;
        Invalidate();
    }

    public void SelectNone()
    {
        for (int i = 0; i < TotalSlots; i++)
            if (_states[i] != JointState.NotPresent)
                _states[i] = JointState.Unselected;
        Invalidate();
    }

    public JointState GetState(int zeroBasedSlot) =>
        (uint)zeroBasedSlot < TotalSlots ? _states[zeroBasedSlot] : JointState.NotPresent;

    public void SetSlotSelected(int zeroBasedSlot, bool selected)
    {
        if ((uint)zeroBasedSlot >= TotalSlots) return;
        if (_states[zeroBasedSlot] == JointState.NotPresent) return;
        _states[zeroBasedSlot] = selected ? JointState.Selected : JointState.Unselected;
        Invalidate();
    }

    public IEnumerable<int> GetSelectedIds()
    {
        for (int i = 0; i < TotalSlots; i++)
            if (_states[i] == JointState.Selected)
                yield return i + 1;
    }

    public int SelectedCount => Array.FindAll(_states, s => s == JointState.Selected).Length;
    public int PresentCount  => Array.FindAll(_states, s => s != JointState.NotPresent).Length;

    // slot → (ring 0-2, posInRing 0-71)
    private static (int Ring, int Pos) SlotToRingPos(int slot) =>
        (slot / SlotsPerRing, slot % SlotsPerRing);

    // ── 几何计算 ──────────────────────────────────────────────

    private RectangleF DrawArea()
    {
        const float pad = 20f;
        float size = Math.Min(Width, Height) - pad * 2f;
        if (size < 60f) return RectangleF.Empty;
        return new RectangleF((Width - size) / 2f, (Height - size) / 2f, size, size);
    }

    private PointF NodeCenter(int slot, RectangleF area)
    {
        var (ring, pos) = SlotToRingPos(slot);
        float cx  = area.X + area.Width  / 2f;
        float cy  = area.Y + area.Height / 2f;
        float er  = Math.Min(area.Width, area.Height) / 2f * RingRadFrac[ring];
        float ang = -MathF.PI / 2f + 2f * MathF.PI * pos / SlotsPerRing;
        return new PointF(cx + er * MathF.Cos(ang), cy + er * MathF.Sin(ang));
    }

    // ── 绘制 ─────────────────────────────────────────────────

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode     = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;

        var   area = DrawArea();
        if (area.IsEmpty) return;

        float er = Math.Min(area.Width, area.Height) / 2f;
        float cx = area.X + area.Width  / 2f;
        float cy = area.Y + area.Height / 2f;

        // 节点半径：外圈间距决定上限，各圈统一大小
        // 外圈周长 / 72 / 2.4 ≈ 合适半径
        float outerR  = er * RingRadFrac[0];
        float spacing = 2f * MathF.PI * outerR / SlotsPerRing;
        _nr = Math.Clamp(spacing / 2.5f, 3.5f, 9f);

        // 圈圈引导线
        using var rp = new Pen(Color.FromArgb(40, 255, 255, 255), 0.6f);
        foreach (float rf in RingRadFrac)
        {
            float r = er * rf;
            g.DrawEllipse(rp, cx - r, cy - r, r * 2, r * 2);
        }

        // 中心十字
        float arm = er * 0.07f;
        using var cp = new Pen(Color.FromArgb(80, 255, 220, 0), 1f);
        g.DrawLine(cp, cx - arm, cy, cx + arm, cy);
        g.DrawLine(cp, cx, cy - arm, cx, cy + arm);

        // 节点（只画彩点，不写数字）
        for (int i = 0; i < TotalSlots; i++)
        {
            var   state = _states[i];
            var   pt    = NodeCenter(i, area);
            Color fill, edge;
            float pw;

            switch (state)
            {
                case JointState.Selected:
                    fill = Color.FromArgb(220, 38, 200, 68);
                    edge = Color.FromArgb(120, 255, 130);
                    pw   = 1.2f;
                    break;
                case JointState.Unselected:
                    fill = Color.FromArgb(210, 215, 95, 20);
                    edge = Color.FromArgb(255, 148, 50);
                    pw   = 1.2f;
                    break;
                default: // NotPresent
                    fill = Color.FromArgb(35, 88, 92, 108);
                    edge = Color.FromArgb(50, 108, 112, 130);
                    pw   = 0.4f;
                    break;
            }

            using var fb = new SolidBrush(fill);
            using var ep = new Pen(edge, pw);
            g.FillEllipse(fb, pt.X - _nr, pt.Y - _nr, _nr * 2, _nr * 2);
            g.DrawEllipse(ep, pt.X - _nr, pt.Y - _nr, _nr * 2, _nr * 2);
        }

        // 圈序号标签（右侧外围）
        DrawRingLabel(g, area, 0, "外圈");
        DrawRingLabel(g, area, 1, "中圈");
        DrawRingLabel(g, area, 2, "内圈");
    }

    private void DrawRingLabel(Graphics g, RectangleF area, int ring, string text)
    {
        float er   = Math.Min(area.Width, area.Height) / 2f;
        float cx   = area.X + area.Width  / 2f;
        float cy   = area.Y + area.Height / 2f;
        float r    = er * RingRadFrac[ring];
        // 放在3点钟方向
        float lx   = cx + r + 4f;
        float ly   = cy - 7f;

        using var font = new Font("微软雅黑", 7f);
        using var brush = new SolidBrush(Color.FromArgb(100, 160, 180));
        g.DrawString(text, font, brush, lx, ly);
    }

    // ── 点击 ─────────────────────────────────────────────────

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button != MouseButtons.Left) return;

        int slot = HitTest(e.Location);
        if (slot < 0) return;

        _states[slot] = _states[slot] == JointState.Selected
            ? JointState.Unselected
            : JointState.Selected;
        Invalidate();
        JointToggled?.Invoke(this, slot);
    }

    private void OnMouseMoveTooltip(object? sender, MouseEventArgs e)
    {
        int slot = HitTest(e.Location);
        if (slot < 0)
        {
            _tip.SetToolTip(this, string.Empty);
            return;
        }
        var (ring, pos) = SlotToRingPos(slot);
        string ringName = ring == 0 ? "外圈" : ring == 1 ? "中圈" : "内圈";
        string state = _states[slot] switch
        {
            JointState.Selected   => "已选中",
            JointState.Unselected => "未选中",
            _                     => "不存在"
        };
        _tip.SetToolTip(this, $"{ringName} 第 {pos + 1} 位  [{state}]");
    }

    // 返回命中的 slot 索引（0-based），未命中返回 -1
    private int HitTest(Point pt)
    {
        var area = DrawArea();
        if (area.IsEmpty) return -1;

        float hitR2 = (_nr + 5f) * (_nr + 5f);
        for (int i = 0; i < TotalSlots; i++)
        {
            if (_states[i] == JointState.NotPresent) continue;
            var   c  = NodeCenter(i, area);
            float dx = pt.X - c.X, dy = pt.Y - c.Y;
            if (dx * dx + dy * dy <= hitR2) return i;
        }
        return -1;
    }
}
