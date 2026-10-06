namespace Contracts
{
    public record QuestionResponseMessage
    {
        public Guid SessionId { get; init; }
        public Guid RequestId { get; init; }

        public string QuestionText { get; init; } = string.Empty;

        public List<string> Options { get; init; } = [];

        public int CorrectIndex { get; init; }

        public bool IsError { get; init; }
    }
}
