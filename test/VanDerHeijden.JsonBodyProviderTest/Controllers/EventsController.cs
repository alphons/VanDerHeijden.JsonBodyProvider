using Microsoft.AspNetCore.Mvc;

namespace VanDerHeijden.JsonBodyProviderTest.Controllers;

[Route("api")]
public class EventsController : ControllerBase
{
	[HttpPost("events")]
	public IActionResult TrackEvent(
		string EventName,
		string UserId,
		DateTime Timestamp,
		int Value,
		Dictionary<string, string> Properties,
		bool IsConversion,
		Guid? CampaignId)
	{
		return Ok(new { EventName, UserId, Timestamp, Value, Properties, IsConversion, CampaignId });
	}
}
