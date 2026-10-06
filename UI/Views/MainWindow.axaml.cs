using Avalonia.Controls;
using Avalonia.Interactivity;
using UI.Views;

namespace UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ExitButton_onClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void StartButton_onClick(object sender, RoutedEventArgs e)
    {
        this.Content = new QuestionView();
    }

    public void QuitCommand()
    {
        Close();
    }
}