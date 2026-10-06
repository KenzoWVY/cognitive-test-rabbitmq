using System;
using System.Threading.Tasks;
using Contracts;

namespace UI.Services;

public interface IQuestionService
{
    event Action<QuestionResponseMessage>? OnQuestionReceived;

    Task InitializeAsync();

    Task RequestQuestionsAsync(int count, Guid sessionId);
}