using Confluent.Kafka;
using EFlow.Common.Infrastructure;
using EFlow.Common.Messaging.DeadLetter;
using EFlow.Common.Messaging.Factories;
using EFlow.Common.Messaging.Init;
using EFlow.Common.Messaging.Producers;
using EFlow.Common.Messaging.Serialization;
using EFlow.Common.Messaging.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace EFlow.Common.Messaging;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddKafka(IConfiguration configuration, bool useDeadLetterQueue = false)
        {
            services.AddKafkaCore(configuration);
            services.AddKafkaProducer();
            services.AddKafkaConsumer(useDeadLetterQueue);

            return services;
        }

        private IServiceCollection AddKafkaCore(IConfiguration configuration)
        {
            services.AddScoped<TopicInitializer>();
            services.TryAddSingleton<ISystemClock, SystemClock>();

            services
                .AddOptions<KafkaSettings>()
                .Bind(configuration.GetRequiredSection(KafkaSettings.SectionName))
                .Validate(KafkaSettingsValidator.IsValid, "KafkaSettings:DlqRetryDelays count must match KafkaSettings:DlqMaxAttempts.")
                .ValidateOnStart();

            services.Configure<KafkaTopicsSettings>(configuration.GetRequiredSection(KafkaSettings.SectionName));

            services.AddSingleton<IAdminClient>(serviceProvider =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<KafkaSettings>>().Value;

                return new AdminClientBuilder(new AdminClientConfig { BootstrapServers = settings.BootstrapServers })
                    .Build();
            });

            return services;
        }

        private IServiceCollection AddKafkaProducer()
        {
            services.AddScoped(typeof(ICommitLogProducer<,>), typeof(CommitLogProducer<,>));
            services.AddScoped(typeof(ISerializer<>), typeof(DefaultSerializer<>));

            services.AddSingleton<ProducerConfig>(serviceProvider =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<KafkaSettings>>().Value;

                return new ProducerConfig
                {
                    BootstrapServers = settings.BootstrapServers,
                    AllowAutoCreateTopics = false,
                    ReconnectBackoffMs = 1000,
                    MessageSendMaxRetries = settings.MessageSendMaxRetries,
                    MessageTimeoutMs = settings.MessageTimeoutMs
                };
            });

            return services;
        }

        private IServiceCollection AddKafkaConsumer(bool useDeadLetterQueue)
        {
            services.AddScoped<ICommitLogConsumerFactory, CommitLogConsumerFactory>();
            services.AddScoped(typeof(IDeserializer<>), typeof(DefaultSerializer<>));

            if (!useDeadLetterQueue)
                return services;

            services.AddScoped<DeadLetterQueueRetryJob>();
            services.AddHostedService<DeadLetterQueueRetryProcessor>();

            return services;
        }
    }
}
