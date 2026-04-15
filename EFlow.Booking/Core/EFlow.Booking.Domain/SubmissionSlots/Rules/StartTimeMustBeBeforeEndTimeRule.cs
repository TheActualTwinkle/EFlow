using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Rules;

/// <inheritdoc />
public sealed class StartTimeMustBeBeforeEndTimeRule : IBusinessRule
{
    private readonly DateTime _startTime;
    private readonly DateTime _endTime;

    internal StartTimeMustBeBeforeEndTimeRule(DateTime startTime, DateTime endTime)
    {
        _startTime = startTime;
        _endTime = endTime;
    }

    public string Message =>
        $"Submission slot start time ({_startTime}) must be before end time ({_endTime}).";

    public bool IsBroken() =>
        _startTime >= _endTime;
}