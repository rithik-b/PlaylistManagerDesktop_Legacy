using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PlaylistManager.Models;

namespace PlaylistManager.Windows;

public partial class YesNoPopup : Window
{
    private readonly Button yesButton;
    private readonly SemaphoreSlim openSemaphore;
    
    public YesNoPopup()
    {
        AvaloniaXamlLoader.Load(this);
        yesButton = this.Find<Button>("YesButton");
        openSemaphore = new SemaphoreSlim(0, 1);
#if DEBUG
            this.AttachDevTools();
#endif
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
        if (ViewModel != null)
        {
            return;
        }
        
        ViewModel = viewModel;
        var mainWindow = parent as MainWindow;
        if (mainWindow != null)
        {
            mainWindow.viewModel.ModalShown = true;
        }
        _ = ShowDialog(parent);
        yesButton.Focus();
        await openSemaphore.WaitAsync();
        if (mainWindow != null)
        {
            mainWindow.viewModel.ModalShown = false;
        }
        ViewModel = null;
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
    private bool ShowNoButton { get; }
    public readonly Action? yesButtonAction;
    public readonly Action? noButtonAction;
    
    public YesNoPopupModel(string message, string yesButtonText = "Yes", string noButtonText = "No",
        bool showNoButton = true, Action? yesButtonAction = null, Action? noButtonAction = null)
    {
        Message = message;
        YesButtonText = yesButtonText;
        NoButtonText = noButtonText;
        ShowNoButton = showNoButton;
        this.yesButtonAction = yesButtonAction;
        this.noButtonAction = noButtonAction;
    }
}