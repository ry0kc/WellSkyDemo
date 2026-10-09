using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReferralDemo.Features.Referrals;
using ReferralDemo.Rules;

namespace ReferralDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReferralsController : ControllerBase
{
    // Note: this controller depends on IMediator, NOT on DbContext,
    private readonly IMediator _mediator;

    public ReferralsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("search")]
    public async Task<IActionResult> SearchPatients([FromQuery] string? name, [FromQuery] string? status)
    {
        var result = await _mediator.Send(new SearchPatientsQuery { NameContains = name, Status = status });
        return Ok(result);
    }

    [HttpGet("{patientId}/status")]
    public async Task<IActionResult> GetReferralStatus(string patientId)
    {
        var result = await _mediator.Send(new GetReferralStatusQuery { PatientId = patientId });
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    [HttpPost("schedule-visit")]
    public async Task<IActionResult> ScheduleVisit([FromBody] ScheduleVisitCommand command)
    {
        var visitId = await _mediator.Send(command);
        return Ok(new { visitId });
    }

    [HttpGet("{patientId}/summary")]
    public async Task<IActionResult> GetReferralSummary(string patientId)
    {
        var result = await _mediator.Send(new GetReferralSummaryQuery { PatientId = patientId });
        return result is null ? NotFound() : Ok(result);
    }   
}
