using MassTransit;
using StateMachineSample.API.Consumers.Base;
using StateMachineSample.Events.Responses;
using StateMachineSample.Events;

namespace StateMachineSample.API.Consumers
{
    public class StartOrderEventConsumer : ConsumerBase<StartOrderEvent>
    {
        protected override Task ConsumeInternal(ConsumeContext<StartOrderEvent> context)
        {
            Console.WriteLine("Order Process Started.");

            context.Respond(new StartOrderEventDto
            {
                OrderId = context.Message.OrderId
            });

            return Task.CompletedTask;
        }
    }
}
