using MassTransit;
using StateMachineSample.Events;

namespace StateMachineSample.StateMachine;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        #region EventsDefinitions

        Event(() => StartOrderEvent);
        Event(() => StartOrderFaultEvent,
            x => x.CorrelateById(context => context.InitiatorId.Value));

        Event(() => OrderProcessInitializationEvent);
        Event(() => OrderProcessInitializationFaultEvent,
            x => x.CorrelateById(context => context.InitiatorId ?? context.Message.Message.OrderId));

        Event(() => CheckProductStockEvent);
        Event(() => CheckProductStockFaultEvent,
            x => x.CorrelateById(context => context.InitiatorId ?? context.Message.Message.OrderId));

        Event(() => TakePaymentEvent);
        Event(() => TakePaymentEventFaultEvent,
            x => x.CorrelateById(context => context.InitiatorId ?? context.Message.Message.OrderId));

        Event(() => CreateOrderEvent);
        Event(() => CreateOrderFaultEvent,
            x => x.CorrelateById(context => context.InitiatorId ?? context.Message.Message.OrderId));

        Event(() => OrderProcessFailedEvent);

        #endregion

        InstanceState(x => x.CurrentState);

        #region Flow

        During(Initial,
            When(StartOrderEvent)
                .Then(x => x.Saga.OrderStartDate = DateTime.Now)
                .TransitionTo(StartOrderState)
                .Then(context => context.Publish<OrderProcessInitializationEvent>(new { OrderId = context.Message.OrderId })));

        During(StartOrderState,                                                                                                     // => Mevcut State bu durumdayken
            When(OrderProcessInitializationEvent)                                                                                   // => Bu Event gelirse
                .TransitionTo(OrderProcessInitializedState)                                                                         // => State'i buna güncelle
                .Then(context => context.Publish<CheckProductStockEvent>(new { OrderId = context.Message.OrderId })));

        During(OrderProcessInitializedState,
            When(CheckProductStockEvent)
            .TransitionTo(CheckProductStockState)
            .Then(context => context.Publish<TakePaymentEvent>(new { OrderId = context.Message.OrderId })));

        During(CheckProductStockState,
            When(TakePaymentEvent)
                .TransitionTo(TakePaymentState)
                .Then(context => context.Publish<CreateOrderEvent>(new { OrderId = context.Message.OrderId })));

        During(TakePaymentState,
            When(CreateOrderEvent)
                .TransitionTo(CreateOrderState));

        #endregion


        #region Fault-Companse State

        DuringAny(When(CreateOrderFaultEvent)
            .TransitionTo(CreateOrderFaultedState)
            .Then(context => context.Publish<Fault<TakePaymentEvent>>(new { context.Message })));

        DuringAny(When(TakePaymentEventFaultEvent)
            .TransitionTo(TakePaymentFaultedState)
            .Then(context => context.Publish<Fault<CheckProductStockEvent>>(new { context.Message })));

        DuringAny(When(CheckProductStockFaultEvent)
            .TransitionTo(CheckProductStockFaultedState)
            .Then(context => context.Publish<Fault<OrderProcessInitializationEvent>>(new { context.Message })));

        DuringAny(When(OrderProcessInitializationFaultEvent)
            .TransitionTo(OrderProcessInitializedFaultedState)
            .Then(context => context.Publish<Fault<StartOrderEvent>>(new { context.Message })));

        DuringAny(When(StartOrderFaultEvent)
            .TransitionTo(StartOrderFaultedState)
            .Then(context => context.Publish<OrderProcessFailedEvent>(new { OrderId = context.Saga.CorrelationId })));

        DuringAny(When(OrderProcessFailedEvent)
            .TransitionTo(OrderProcessFailedState));

        #endregion
    }

    #region Events

    public Event<StartOrderEvent> StartOrderEvent { get; }
    public Event<Fault<StartOrderEvent>> StartOrderFaultEvent { get; }

    public Event<OrderProcessInitializationEvent> OrderProcessInitializationEvent { get; }
    public Event<Fault<OrderProcessInitializationEvent>> OrderProcessInitializationFaultEvent { get; }

    public Event<CheckProductStockEvent> CheckProductStockEvent { get; }
    public Event<Fault<CheckProductStockEvent>> CheckProductStockFaultEvent { get; }

    public Event<TakePaymentEvent> TakePaymentEvent { get; }
    public Event<Fault<TakePaymentEvent>> TakePaymentEventFaultEvent { get; }

    public Event<CreateOrderEvent> CreateOrderEvent { get; }
    public Event<Fault<CreateOrderEvent>> CreateOrderFaultEvent { get; }

    public Event<OrderProcessFailedEvent> OrderProcessFailedEvent { get; }

    #endregion


    #region States

    public State StartOrderState { get; }
    public State StartOrderFaultedState { get; }

    public State OrderProcessInitializedState { get; }
    public State OrderProcessInitializedFaultedState { get; }

    public State CheckProductStockState { get; }
    public State CheckProductStockFaultedState { get; }

    public State TakePaymentState { get; }
    public State TakePaymentFaultedState { get; }

    public State CreateOrderState { get; }
    public State CreateOrderFaultedState { get; }

    public State OrderProcessFailedState { get; }

    #endregion















    private void TestDebug<T>(BehaviorContext<OrderState, T> context) where T : class
    {
        if (context is BehaviorContext<OrderState, OrderProcessInitializationEvent>)
        {
            context.Publish<CheckProductStockEvent>(new { OrderId = context.Saga.CorrelationId });
        }
        else if (context is BehaviorContext<OrderState, CheckProductStockEvent>)
        {
            context.Publish<TakePaymentEvent>(new { OrderId = context.Saga.CorrelationId });
        }
        else if (context is BehaviorContext<OrderState, TakePaymentEvent>)
        {
            context.Publish<CreateOrderEvent>(new { OrderId = context.Saga.CorrelationId });
        }
    }
}