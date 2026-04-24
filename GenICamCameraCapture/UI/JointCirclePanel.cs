using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace GenICamCameraCapture.UI;

/// <summary>
/// 72 焊点三圈同心圆选择控件。
/// 槽位 0-35 → 外圈(36个)，36-59 → 中圈(24个)，60-71 → 内圈(12个)。
/// 状态：NotPresent（灰色，不可点）/ Unselected（橙色）/ Selected（绿色）。
/// </summary>
public sealed class JointCirclePanel : Panel
{
    public enum JointState { NotPresent, Unselected, Selected }

    private static readonly (int Count, float RadFrac)[] Rings =
    {
        (36, 0.82f),
        (24, 0.56f),
        (12, 0.30f),
    };

    public const int TotalSlots = 72;

    private readonly JointState[] _states = new JointState[TotalSlots];
    private float _nr; // 当前节点半径，OnPaint 时计算

    /// <summary>节点被点击后触发，参数为 0-based slot 索引。</summary>
    public event EventHandler<int>? JointToggled;

    public JointCirclePanel()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.FromArgb(22, 24, 32);
        MinimumSize = new Size(200, 200);
    }

    // ── 公开 API ──────────────────────────────────────────────

    /// <summary>
    /// 传入检测到的焊点 ID（1-based）。
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

    public JointState GetState(int zeroBasedSlot) => _states[zeroBasedSlot];

    public IEnumerable<int> GetSelectedIds()
    {
        for (int i = 0; i < TotalSlots; i++)
            if (_states[i] == JointState.Selected)
                yield return i + 1;
    }

    public int SelectedCount => Array.FindAll(_states, s => s == JointState.Selected).Length;
    public int PresentCount  => Array.FindAll(_states, s => s != JointState.NotPresent).Length;

    /// <summary>直接设置单个存在槽位的选中状态（对 NotPresent 槽位无效）。</summary>
    public void SetSlotSelected(int zeroBasedSlot, bool selected)
    {
        if ((uint)zeroBasedSlot >= TotalSlots) return;
        if (_states[zeroBasedSlot] == JointState.NotPresent) return;
        _states[zeroBasedSlot] = selected ? JointState.Selected : JointState.Unselected;
        Invalidate();
    }

    // ── 内部计算 ──────────────────────────────────────────────

    private static (int Ring, int Pos) SlotToRingPos(int slot)
    {
        int cum = 0;
        for (int r = 0; r < Rings.Length; r++)
        {
            if (slot < cum + Rings[r].Count) return (r, slot - cum);
            cum += Rings[r].Count;
        }
        return (Rings.Length - 1, slot - (TotalSlots - Rings[^1].Count));
    }

    private PointF NodeCenter(int slot, RectangleF area)
    {
        var (ring, pos) = SlotToRingPos(slot);
        float cx  = area.X + area.Width  / 2f;
        float cy  = area.Y + area.Height / 2f;
        float er  = Math.Min(area.Width, area.Height) / 2f * Rings[ring].RadFrac;
        float ang = -MathF.PI / 2f + 2f * MathF.PI * pos / Rings[ring].Count;
        return new PointF(cx + er * MathF.Cos(ang), cy + er * MathF.Sin(ang));
    }

    private RectangleF DrawArea()
    {
        const float pad = 18f;
        float size = Math.Min(Width, Height) - pad * 2f;
        if (size < 40f) return RectangleF.Empty;
        return new RectangleF((Width - size) / 2f, (Height - size) / 2f, size, size);
    }

    // ── 绘制 ─────────────────────────────────────────────────

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode     = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;

        var area = DrawArea();
        if (area.IsEmpty) return;

        float er = Math.Min(area.Width, area.Height) / 2f;
        _nr = MathF.Max(6.5f, er * 0.048f);

        float cx = area.X + area.Width  / 2f;
        float cy = area.Y + area.Height / 2f;

        // 三圈引导线
        using var rp = new Pen(Color.FromArgb(45, 255, 255, 255), 0.8f);
        foreach (var (_, rf) in Rings)
        {
            float r = er * rf;
            g.DrawEllipse(rp, cx - r, cy - r, r * 2, r * 2);
        }

        // 中心十字
        float arm = er * 0.07f;
        using var cp = new Pen(Color.FromArgb(90, 255, 220, 0), 1f);
        g.DrawLine(cp, cx - arm, cy, cx + arm, cy);
        g.DrawLine(cp, cx, cy - arm, cx, cy + arm);

        // 节点
        float fs = MathF.Max(4.5f, _nr * 0.70f);
        using var fnt = new Font("Arial", fs, FontStyle.Bold);

        for (int i = 0; i < TotalSlots; i++)
        {
            var   state = _states[i];
            var   pt    = NodeCenter(i, area);
            Color fill, edge;
            float pw;

            switch (state)
            {
                case JointState.Selected:
                    fill = Color.FromArgb(210, 38, 195, 68);
                    edge = Color.FromArgb(110, 255, 130);
                    pw   = 1.5f;
                    break;
                case JointState.Unselected:
                    fill = Color.FromArgb(200, 215, 95, 20);
                    edge = Color.FromArgb(255, 148, 50);
                    pw   = 1.5f;
                    break;
                default: // NotPresent
                    fill = Color.FromArgb(42, 88, 92, 108);
                    edge = Color.FromArgb(55, 108, 112, 130);
                    pw   = 0.5f;
                    break;
            }

            using var fb = new SolidBrush(fill);
            using var ep = new Pen(edge, pw);
            g.FillEllipse(fb, pt.X - _nr, pt.Y - _nr, _nr * 2, _nr * 2);
            g.DrawEllipse(ep, pt.X - _nr, pt.Y - _nr, _nr * 2, _nr * 2);

            if (state != JointState.NotPresent)
            {
                string lbl = (i + 1).ToString();
                using var lb = new SolidBrush(
                    state == JointState.Selected
                        ? Color.White
                        : Color.FromArgb(255, 215, 175));
                var sz = g.MeasureString(lbl, fnt);
                g.DrawString(lbl, fnt, lb,
                    pt.X - sz.Width  / 2f + 0.3f,
                    pt.Y - sz.Height / 2f + 0.3f);
            }
        }
    }

    // ── 点击 ─────────────────────────────────────────────────

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button != MouseButtons.Left) return;

        var area = DrawArea();
        if (area.IsEmpty) return;

        float hitR2 = (_nr + 4f) * (_nr + 4f);
        for (int i = 0; i < TotalSlots; i++)
        {
            if (_states[i] == JointState.NotPresent) continue;
            var   pt = NodeCenter(i, area);
            float dx = e.X - pt.X, dy = e.Y - pt.Y;
            if (dx * dx + dy * dy <= hitR2)
            {
                _states[i] = _states[i] == JointState.Selected
                    ? JointState.Unselected
                    : JointState.Selected;
                Invalidate();
                JointToggled?.Invoke(this, i);
                return;
            }
        }
    }
}
