namespace Contracts;

public record QuestionRequestMessage
{
    public Guid SessionId { get; init; }
    public Guid RequestId { get; init; }

    public string Topic { get; init; } = string.Empty;

    public string Difficulty { get; init; } = string.Empty;

    public int QuestionAmount { get; init; }
}
