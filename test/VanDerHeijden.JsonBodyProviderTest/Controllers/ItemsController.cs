using Microsoft.AspNetCore.Mvc;

namespace VanDerHeijden.JsonBodyProviderTest.Controllers;

[Route("api")]
public class ItemsController : ControllerBase
{
	[HttpPatch("items/{id}")]
	public IActionResult PatchItem(
		Guid id,
		string? Title,
		string? Description,
		bool? Active,
		int? Priority)
	{
		return Ok(new { id, Title, Description, Active, Priority });
	}
}
