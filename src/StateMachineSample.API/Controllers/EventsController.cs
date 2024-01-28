using MassTransit;
using Microsoft.AspNetCore.Mvc;
using StateMachineSample.API.Requests;
using StateMachineSample.Events;

namespace StateMachineSample.API.Controllers;

public class EventsController
{
    private readonly IPublishEndpoint _publishEndpoint;

    public EventsController(
        IPublishEndpoint publishEndpoint
        )
    {
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost("startOrder")]
    public async Task<IActionResult> StartOrder([FromBody] EventCommonRequest request)
    {
        await _publishEndpoint.Publish<StartOrderEvent>(new { request.OrderId });
        return new NoContentResult();
    }
}