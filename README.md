# VanDerHeijden.JsonBodyProvider

**ASP.NET Core custom value provider**  
Allows you to **bind multiple simple parameters directly from a JSON request body** — without using `[FromBody]` and without wrapper DTO classes.

**Main purpose**  
Call controller actions like this:

```csharp
[HttpPost]
public IActionResult CreateUser(
    string Name,
    int Age,
    List<string> Roles,
    Guid TenantId,
    DateOnly StartDate,
    bool IsActive,
    string? Comment = null)
{
    // ← all parameters automatically come from JSON body
}
```

## Most important facts first

**Works only without** `[ApiController]` attribute  
`[ApiController]` breaks this completely → **remove it** from the controller

## Installation

```bash
dotnet add package JsonBodyProvider
```

## Recommended minimal startup configuration

```csharp
builder.Services
    .AddControllers()                       // ← no [ApiController] !
    .AddJsonBodyProvider(CorrectLists: true);  // ← enables normal list + string behaviour
```

## Good real-world examples

### Example 1 – Classic user creation

```csharp
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
        // ...
    }
}
```

```json
POST /api/users
Content-Type: application/json

{
  "name": "Lisa van Dijk",
  "age": 34,
  "email": "lisa@example.com",
  "roles": ["editor", "reviewer"],
  "isActive": true
}
```

### Example 2 – Order with items

```csharp
[HttpPost("orders")]
public IActionResult PlaceOrder(
    string OrderNumber,
    DateOnly OrderDate,
    decimal TotalAmount,
    List<string> ProductCodes,
    Guid CustomerId,
    string? Remark = null)
{
    // ...
}
```

```json
{
  "orderNumber": "ORD-2025-78412",
  "orderDate": "2025-06-18",
  "totalAmount": 1249.95,
  "productCodes": ["PROD-A42", "PROD-B17", "PROD-C09"],
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "remark": ""
}
```

### Example 3 – Very flat analytics event

```csharp
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
    // ...
}
```

```json
{
  "eventName": "purchase_completed",
  "userId": "usr_9k3j2p8x",
  "timestamp": "2025-06-18T14:22:09Z",
  "value": 1,
  "properties": {
    "category": "electronics",
    "source": "newsletter"
  },
  "isConversion": true,
  "campaignId": null
}
```

### Example 4 – Very minimal update

```csharp
[HttpPatch("items/{id}")]
public IActionResult PatchItem(
    Guid id,
    string? Title,
    string? Description,
    bool? Active,
    int? Priority)
{
    // only the fields that are sent will be bound
}
```

```json
{
  "title": "Updated task name",
  "priority": 3
}
```

## What you get for free

- Multiple parameters from **one** JSON body
- No need for `[FromBody]` on every parameter
- No wrapper DTO needed
- Empty strings stay empty strings
- Lists behave nicely (no random fallback to default(T) on parse errors)
- Headers and cookies are also available as parameters

## Golden rule (repeat until it hurts)

**Do NOT use [ApiController] on controllers where you want to use this package**

If you really need `[ApiController]` → you cannot use this style of multi-parameter binding

## License

MIT
