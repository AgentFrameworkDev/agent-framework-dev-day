namespace WorkflowLab.HumanInTheLoop;

/// <summary>
/// Request sent to supervisor for review.
/// </summary>
public sealed record SupervisorReviewRequest(
    string TicketId,
    string Category,
    string Priority,
    string DraftResponse
);

/// <summary>
/// Supervisor's decision after reviewing the draft.
/// </summary>
public sealed record SupervisorDecision(
    ReviewAction Action,
    string? ModifiedResponse,
    string? Notes
);

/// <summary>
/// Actions a supervisor can take.
/// </summary>
public enum ReviewAction
{
    Approve,
    Edit,
    Escalate
}
