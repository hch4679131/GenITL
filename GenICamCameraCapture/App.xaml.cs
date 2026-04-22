using System.Windows;
using System.Windows.Threading;

namespace GenICamCameraCapture;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"UI 线程异常:\n{e.Exception.Message}\n\n{e.Exception.StackTrace}",
            "未处理异常", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            MessageBox.Show($"致命异常:\n{ex.Message}\n\n{ex.StackTrace}",
                "未处理异常", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        MessageBox.Show($"后台任务异常:\n{e.Exception.InnerException?.Message}\n\n{e.Exception.InnerException?.StackTrace}",
            "未处理异常", MessageBoxButton.OK, MessageBoxImage.Error);
        e.SetObserved();
    }
}
