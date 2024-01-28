using MassTransit;
using StateMachineSample.API.Consumers.Base;
using StateMachineSample.Events;

namespace StateMachineSample.API.Consumers
{
    public class StartOrderFaultEventConsumer : ConsumerBase<Fault<StartOrderEvent>>
    {
        protected override Task ConsumeInternal(ConsumeContext<Fault<StartOrderEvent>> context)
        {
            Console.WriteLine("Start Order Process Faulted.");

            return Task.CompletedTask;
        }
    }
}
