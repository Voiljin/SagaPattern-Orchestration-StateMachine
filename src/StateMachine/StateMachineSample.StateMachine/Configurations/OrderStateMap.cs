using MassTransit;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace StateMachineSample.StateMachine
{
    public class OrderStateMap :
    SagaClassMap<OrderState>
    {
        protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
        {
            entity.Property(x => x.CurrentState).HasMaxLength(64);
            entity.Property(x => x.OrderStartDate);

            // If using Optimistic concurrency, otherwise remove this property
            entity.Property(x => x.Version).IsRowVersion();
        }
    }
}
