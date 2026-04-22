using System.Windows.Forms;
using GenICamCameraCapture.UI;

namespace GenICamCameraCapture;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Application.ThreadException += (_, e) =>
            MessageBox.Show($"UI线程异常:\n{e.Exception.Message}\n\n{e.Exception.StackTrace}",
                "未处理异常", MessageBoxButtons.OK, MessageBoxIcon.Error);

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                MessageBox.Show($"致命异常:\n{ex.Message}\n\n{ex.StackTrace}",
                    "未处理异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            MessageBox.Show($"后台任务异常:\n{e.Exception.InnerException?.Message}",
                "未处理异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            e.SetObserved();
        };

        Application.Run(new MainForm());
    }
}
