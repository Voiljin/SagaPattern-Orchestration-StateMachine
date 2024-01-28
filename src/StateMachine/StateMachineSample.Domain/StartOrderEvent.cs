using MassTransit;
using System.Runtime.CompilerServices;

namespace StateMachineSample.Events
{
    public class StartOrderEvent
    {
        public Guid OrderId { get; set; }

        [ModuleInitializer]
        internal static void Init()
        {
            GlobalTopology.Send.UseCorrelationId<StartOrderEvent>(x => x.OrderId);
        }
    }
}
