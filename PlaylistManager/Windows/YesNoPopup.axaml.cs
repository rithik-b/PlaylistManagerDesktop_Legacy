using System;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PlaylistManager.Models;

namespace PlaylistManager.Windows;

public partial class YesNoPopup : Window
{
    private readonly SemaphoreSlim openSemaphore;
    
    public YesNoPopup()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
            this.AttachDevTools();
#endif
        openSemaphore = new SemaphoreSlim(0, 1);
    }
    
    private YesNoPopupModel? viewModel;
    private YesNoPopupModel? ViewModel
    {
        get => viewModel;
        set
        {
            viewModel = value;
            DataContext = value;
        }
    }

    public async void ShowPopup(Window parent, YesNoPopupModel viewModel)
    {
        ViewModel = viewModel;
        var mainWindow = parent as MainWindow;
        if (mainWindow != null)
        {
            mainWindow.viewModel.ModalShown = true;
        }
        _ = ShowDialog(parent);
        await openSemaphore.WaitAsync();
        if (mainWindow != null)
        {
            mainWindow.viewModel.ModalShown = false;
        }
        Hide();
    }

    private void YesButtonClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.yesButtonAction?.Invoke();
        openSemaphore.Release();
    }


    private void NoButtonClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.noButtonAction?.Invoke();
        openSemaphore.Release();
    }
}

public class YesNoPopupModel : ViewModelBase
{
    private string Message { get; }
    private string YesButtonText { get; }
    private string NoButtonText { get; }
    public readonly Action? yesButtonAction;
    public readonly Action? noButtonAction;
    
    public YesNoPopupModel(string message, string yesButtonText = "Yes", string noButtonText = "No",
        Action? yesButtonAction = null, Action? noButtonAction = null)
    {
        Message = message;
        YesButtonText = yesButtonText;
        NoButtonText = noButtonText;
        this.yesButtonAction = yesButtonAction;
        this.noButtonAction = noButtonAction;
    }
}