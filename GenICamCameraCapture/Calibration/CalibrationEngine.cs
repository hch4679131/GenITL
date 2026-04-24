using GenICamCameraCapture.Models;

namespace GenICamCameraCapture.Calibration;

/// <summary>
/// 9点仿射标定引擎。
/// 建立像素坐标 → 机器人TCP坐标 的仿射变换（6参数最小二乘）：
///   robot_x = a·px + b·py + c
///   robot_y = d·px + e·py + f
/// 至少需要 3 个有效标定点；9点过定方程组用法方程求解。
/// </summary>
public static class CalibrationEngine
{
    // ── 公开 API ─────────────────────────────────────────────

    /// <summary>
    /// 从标定点集合计算 2×3 仿射矩阵。
    /// 返回 null 表示标定点不足或矩阵奇异。
    /// </summary>
    public static float[,]? Compute(IEnumerable<CalibrationPoint> pts)
    {
        var valid = pts.Where(p => p.IsMarked).ToArray();
        if (valid.Length < 3) return null;

        var data = valid
            .Select(p => (px: (double)p.PixelX, py: (double)p.PixelY,
                          rx: (double)p.RobotX,  ry: (double)p.RobotY))
            .ToArray();

        double[]? cx = SolveLeastSquares(data.Select(d => (d.px, d.py)).ToArray(),
                                          data.Select(d => d.rx).ToArray());
        double[]? cy = SolveLeastSquares(data.Select(d => (d.px, d.py)).ToArray(),
                                          data.Select(d => d.ry).ToArray());
        if (cx == null || cy == null) return null;

        var m = new float[2, 3];
        m[0, 0] = (float)cx[0]; m[0, 1] = (float)cx[1]; m[0, 2] = (float)cx[2];
        m[1, 0] = (float)cy[0]; m[1, 1] = (float)cy[1]; m[1, 2] = (float)cy[2];
        return m;
    }

    /// <summary>像素坐标 → 机器人坐标。</summary>
    public static (float x, float y) PixelToRobot(float[,] m, float px, float py) =>
        (m[0, 0] * px + m[0, 1] * py + m[0, 2],
         m[1, 0] * px + m[1, 1] * py + m[1, 2]);

    /// <summary>计算 RMS 重投影误差（mm）。</summary>
    public static float ComputeRms(float[,] m, IEnumerable<CalibrationPoint> pts)
    {
        var valid = pts.Where(p => p.IsMarked).ToArray();
        if (valid.Length == 0) return float.NaN;
        double sum = 0;
        foreach (var p in valid)
        {
            var (ex, ey) = PixelToRobot(m, p.PixelX, p.PixelY);
            sum += (ex - p.RobotX) * (ex - p.RobotX) + (ey - p.RobotY) * (ey - p.RobotY);
        }
        return (float)Math.Sqrt(sum / valid.Length);
    }

    /// <summary>将标定矩阵序列化为 JSON 字符串（便于保存到文件）。</summary>
    public static string MatrixToJson(float[,] m) =>
        $"{{\"m00\":{m[0,0]:F6},\"m01\":{m[0,1]:F6},\"m02\":{m[0,2]:F6}," +
        $"\"m10\":{m[1,0]:F6},\"m11\":{m[1,1]:F6},\"m12\":{m[1,2]:F6}}}";

    /// <summary>从 JSON 字符串反序列化标定矩阵。</summary>
    public static float[,]? MatrixFromJson(string json)
    {
        try
        {
            float Get(string key)
            {
                int i = json.IndexOf($"\"{key}\":", StringComparison.Ordinal) + key.Length + 3;
                int j = json.IndexOfAny(new[] { ',', '}' }, i);
                return float.Parse(json[i..j]);
            }
            var m = new float[2, 3];
            m[0, 0] = Get("m00"); m[0, 1] = Get("m01"); m[0, 2] = Get("m02");
            m[1, 0] = Get("m10"); m[1, 1] = Get("m11"); m[1, 2] = Get("m12");
            return m;
        }
        catch { return null; }
    }

    // ── 最小二乘：求 a,b,c 使 Σ(a·px+b·py+c - val)² 最小 ─────

    // 法方程 (A^T A) x = A^T b，A 为 n×3，解 3×3 系统
    private static double[]? SolveLeastSquares(
        (double px, double py)[] coords, double[] vals)
    {
        int n = coords.Length;
        // A^T A (3×3) 和 A^T b (3×1)
        double[,] AtA = new double[3, 3];
        double[]  Atb = new double[3];

        foreach (var ((px, py), v) in coords.Zip(vals))
        {
            double[] row = { px, py, 1.0 };
            for (int i = 0; i < 3; i++)
            {
                Atb[i] += row[i] * v;
                for (int j = 0; j < 3; j++)
                    AtA[i, j] += row[i] * row[j];
            }
        }
        return Solve3x3(AtA, Atb);
    }

    // Gauss 消元（带列主元）求解 3×3 线性方程组
    private static double[]? Solve3x3(double[,] A, double[] b)
    {
        const int N = 3;
        double[,] aug = new double[N, N + 1];
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++) aug[i, j] = A[i, j];
            aug[i, N] = b[i];
        }

        for (int col = 0; col < N; col++)
        {
            // 选主元
            int pivot = col;
            for (int r = col + 1; r < N; r++)
                if (Math.Abs(aug[r, col]) > Math.Abs(aug[pivot, col])) pivot = r;
            for (int k = 0; k <= N; k++) (aug[col, k], aug[pivot, k]) = (aug[pivot, k], aug[col, k]);

            if (Math.Abs(aug[col, col]) < 1e-12) return null; // 奇异

            // 消元
            for (int r = 0; r < N; r++)
            {
                if (r == col) continue;
                double f = aug[r, col] / aug[col, col];
                for (int k = col; k <= N; k++) aug[r, k] -= f * aug[col, k];
            }
        }

        return Enumerable.Range(0, N).Select(i => aug[i, N] / aug[i, i]).ToArray();
    }
}
