using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Contracts;

namespace UI.ViewModels;

public partial class QuestionViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedQuestionNumber))]
    public partial int QuestionNumber { get; set; }

    [ObservableProperty]
    public partial string QuestionText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int SelectedIndex { get; set; } = -1;

    public string FormattedQuestionNumber => $"Question {QuestionNumber}";

    public ObservableCollection<string> Options { get; } = new();

    private readonly int _correctIndex;
    private readonly Action _onComplete;

    public QuestionViewModel(QuestionResponseMessage response, Action onComplete)
    {
        QuestionText = response.QuestionText;
        _correctIndex = response.CorrectIndex;
        _onComplete = onComplete;

        foreach (var opt in response.Options) Options.Add(opt);
    }

    [RelayCommand]
    private void Submit()
    {
        if (SelectedIndex == -1) return;

        bool isCorrect = (SelectedIndex == _correctIndex);
        _onComplete.Invoke();
    }
}