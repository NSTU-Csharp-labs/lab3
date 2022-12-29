using System;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;

namespace lab3.Controls.MainWindow;

public static class UIMessageBox
{
    public static async void Show(string title, string message)
    {
        var messageBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(
            new MessageBoxStandardParams
            {
                ContentTitle = title,
                ContentMessage = message
            });
        await messageBoxStandardWindow.Show();
    }

    public static async void ShowUnhandledError(Exception ex)
    {
        var messageBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(
            new MessageBoxStandardParams
            {
                ContentTitle = "Ошибка",
                ContentMessage = "Необработанная ошибка, пожалуйста, обратитесь в поддержку\n" + ex.Message
            });
        await messageBoxStandardWindow.Show();
    }

    public static async void ShowError(Exception ex)
    {
        var messageBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(
            new MessageBoxStandardParams
            {
                ContentTitle = "Ошибка",
                ContentMessage = ex.Message
            });
        await messageBoxStandardWindow.Show();
    }

    public static async void ShowError(string message)
    {
        var messageBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(
            new MessageBoxStandardParams
            {
                ContentTitle = "Ошибка",
                ContentMessage = message
            });
        await messageBoxStandardWindow.Show();
    }
}