using Microsoft.AspNetCore.Mvc;

namespace VanDerHeijden.JsonBodyProviderTest.Controllers;

[Route("api/users")]
public class UsersController : ControllerBase
{
	[HttpPost]
	public IActionResult Create(
		string Name,
		int Age,
		string Email,
		List<string> Roles,
		bool IsActive = true)
	{
		return Ok(new { Name, Age, Email, Roles, IsActive });
	}
}
