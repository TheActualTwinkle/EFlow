namespace EFlow.Common.Messaging.Settings;

public static class KafkaSettingsValidator
{
    public static bool IsValid(KafkaSettings settings) =>
        !string.IsNullOrWhiteSpace(settings.DeadLetterConsumerGroup) &&
        settings.DlqMaxAttempts > 0 &&
        settings.DlqRetryDelays.Count == settings.DlqMaxAttempts;
}
