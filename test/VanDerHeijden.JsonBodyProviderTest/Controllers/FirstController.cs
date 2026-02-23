using Microsoft.AspNetCore.Mvc;

namespace JsonBodyProviderTest.Controllers;

[Route("[controller]")]
public class FirstController : ControllerBase
{
	[HttpPost("Test")]
	public IActionResult Test(
		string UserId,
		DateTime Timestamp,
		int Value,
		byte Bite,
		double Weight,
		decimal Price,
		float Height,
		List<int> IntList,
		Dictionary<string, string> Properties,
		bool isConversion,
		Guid? campaignId,
		char FirstChar,
		long Ernie,
		short Man,
		DateOnly Dater,
		TimeOnly Timer)
	{
		return Ok(new
		{
			UserId,
			Timestamp,
			Value,
			Bite,
			Weight,
			Price,
			Height,
			IntList,
			Properties,
			isConversion,
			campaignId,
			FirstChar,
			Ernie,
			Man,
			Dater,
			Timer
		});
	}
}
