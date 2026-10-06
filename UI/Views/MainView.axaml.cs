using Avalonia.Controls;
using Avalonia.Interactivity;
using UI.Views;

namespace UI.Views;

public partial class MainView : Window
{
    public MainView()
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