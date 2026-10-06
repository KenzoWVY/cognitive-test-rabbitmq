using Avalonia.Threading;
using UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UI.Services;
using UI.ViewModels;

namespace UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IQuestionService _questionService;
    private readonly Queue<QuestionViewModel> _questionBuffer = new();

    [ObservableProperty]
    public partial ObservableObject CurrentPage { get; set; } = null!;

    private int _currentQuestionNumber = 0;
    private readonly int _totalQuestionsRequested = 5;

    public MainViewModel(IQuestionService questionService)
    {
        _questionService = questionService;
        _questionService.OnQuestionReceived += HandleIncomingQuestion;

        CurrentPage = new StartMenuViewModel();

        _ = _questionService.InitializeAsync();
    }

    [RelayCommand]
    private async Task StartTest()
    {
        CurrentPage = new LoadingViewModel();

        await _questionService.RequestQuestionsAsync(_totalQuestionsRequested, Guid.NewGuid());
    }

    private void HandleIncomingQuestion(QuestionResponseMessage response)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var newVm = new QuestionViewModel(response, onComplete: ShowNextQuestion);
            _questionBuffer.Enqueue(newVm);

            if (CurrentPage is LoadingViewModel)
            {
                ShowNextQuestion();
            }
        });
    }

    private void ShowNextQuestion()
    {
        if (_currentQuestionNumber >= _totalQuestionsRequested)
        {
            CurrentPage = new StartMenuViewModel();
            _currentQuestionNumber = 0;
            return;
        }

        if (_questionBuffer.TryDequeue(out var nextQuestion))
        {
            _currentQuestionNumber++;
            nextQuestion.QuestionNumber = _currentQuestionNumber;
            CurrentPage = nextQuestion;
        }
        else
        {
            CurrentPage = new LoadingViewModel();
        }
    }
}