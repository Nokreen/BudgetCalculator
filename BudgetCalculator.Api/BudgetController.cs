using Microsoft.AspNetCore.Mvc;

namespace BudgetCalculator.Api;

[ApiController]
[Route("api/budget")]
public class BudgetController : ControllerBase
{
    [HttpPost("calculate")]
    public IActionResult Calculate([FromBody] BudgetRequest request)
    {
        if (request.Days <= 0) return BadRequest("Количество дней должно быть больше нуля.");

        var total = request.DailyCost * request.Days;

        // Баг: страховка должна стоить 5%, а сейчас умножается на 5 (500%)
        if (request.IncludeInsurance)
        {
            total *= 1.05m;
        }

        return Ok(new { TotalAmount = total });
    }
}