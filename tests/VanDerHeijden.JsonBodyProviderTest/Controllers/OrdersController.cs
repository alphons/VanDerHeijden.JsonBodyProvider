using Microsoft.AspNetCore.Mvc;

namespace VanDerHeijden.JsonBodyProviderTest.Controllers;

[Route("api")]
public class OrdersController : ControllerBase
{
	[HttpPost("orders")]
	public IActionResult PlaceOrder(
		string OrderNumber,
		DateOnly OrderDate,
		decimal TotalAmount,
		List<string> ProductCodes,
		Guid CustomerId,
		string? Remark = null)
	{
		return Ok(new { OrderNumber, OrderDate, TotalAmount, ProductCodes, CustomerId, Remark });
	}
}
