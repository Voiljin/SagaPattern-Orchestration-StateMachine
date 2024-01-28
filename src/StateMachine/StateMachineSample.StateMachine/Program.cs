using MassTransit;
using Microsoft.EntityFrameworkCore;
using StateMachineSample.StateMachine;
using StateMachineSample.StateMachine.Settings;
using System.Reflection;

Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder =>
    {
        var configurationBuilder = new ConfigurationBuilder();
        var configuration = configurationBuilder.AddEnvironmentVariables().AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json")
            .Build();

        builder.Sources.Clear();
        builder.AddConfiguration(configuration);
    })
    .ConfigureServices((hostContext, services) =>
    {
        var messageBrokerQueueSettings = hostContext.Configuration.GetSection("MessageBroker:QueueSettings").Get<MessageBrokerQueueSettings>();

        services.AddMassTransit(x =>
        {
            x.AddSagaStateMachine<OrderStateMachine, OrderState>().EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Pessimistic; 

            r.AddDbContext<DbContext, OrderStateDbContext>((provider, builder) =>
            {
                //builder.UseSqlServer("Server=localhost,1433;Database=SagaDB;User Id=sa;Password=Password06!;TrustServerCertificate=True;", m =>
                //{
                //    m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                //    m.MigrationsHistoryTable($"__{nameof(OrderStateDbContext)}");
                //});

                builder.UseSqlServer("Server=DESKTOP-PES153H;Database=SagaDB;Integrated Security=True;TrustServerCertificate=True;", m =>
                {
                    m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                    m.MigrationsHistoryTable($"__{nameof(OrderStateDbContext)}");
                });
            });
        });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(messageBrokerQueueSettings.HostName, messageBrokerQueueSettings.VirtualHost, h =>
                {
                    h.Username(messageBrokerQueueSettings.UserName);
                    h.Password(messageBrokerQueueSettings.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });



    }).Build().Run();