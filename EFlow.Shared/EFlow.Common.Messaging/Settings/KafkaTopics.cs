namespace EFlow.Common.Messaging.Settings;

public static class KafkaTopics
{
    public const string SubmissionSlotCreatedTopic = "eflow-submission-slot-created";
    
    public const string SubmissionSlotUpdatedTopic = "eflow-submission-slot-updated";

    public const string SubmissionSlotDeletedTopic = "eflow-submission-slot-deleted";

    public const string BookingCreatedTopic = "eflow-booking-created";

    public const string BookingCancelledTopic = "eflow-booking-cancelled";
}
