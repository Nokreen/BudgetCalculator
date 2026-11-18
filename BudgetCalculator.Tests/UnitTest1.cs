using Microsoft.AspNetCore.Mvc;
using BudgetCalculator.Api;
using Xunit;

namespace BudgetCalculator.Tests;

public class BudgetControllerTests
{
    // Позитивный сценарий 
    [Fact]
    public void Calculate_HappyPath_Returns200AndCorrectTotal()
    {
        var controller = new BudgetController();
        var request = new BudgetRequest { DailyCost = 100m, Days = 5, IncludeInsurance = false };

        var result = controller.Calculate(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(500m, GetTotalAmount(okResult));
    }

    // Проверка бага со страховкой
    [Fact]
    public void Calculate_WithInsurance_AddsFivePercent()
    {
        var controller = new BudgetController();
        var request = new BudgetRequest { DailyCost = 100m, Days = 5, IncludeInsurance = true };

        var result = controller.Calculate(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(525m, GetTotalAmount(okResult));
    }

    // Негативный сценарий 
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Calculate_NonPositiveDays_Returns400BadRequest(int days)
    {
        var controller = new BudgetController();
        var request = new BudgetRequest { DailyCost = 100m, Days = days, IncludeInsurance = false };

        var result = controller.Calculate(request);

        // Проверяем, что вернулся именно BadRequest 
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    //  Негативный сценарий 
    [Fact]
    public void Calculate_ZeroDailyCost_Returns200AndZero()
    {
        var controller = new BudgetController();
        var request = new BudgetRequest { DailyCost = 0m, Days = 5, IncludeInsurance = false };

        var result = controller.Calculate(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(0m, GetTotalAmount(okResult));
    }

    // Проверка округления 
    [Fact]
    public void Calculate_FractionalDailyCost_CalculatesToKopecks()
    {
        var controller = new BudgetController();
        // 100.50 * 3 = 301.50
        var request = new BudgetRequest { DailyCost = 100.50m, Days = 3, IncludeInsurance = false };

        var result = controller.Calculate(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(301.50m, GetTotalAmount(okResult));
    }

    // Доп  метод, чтобы не дублировать код извлечения суммы
    private static decimal GetTotalAmount(OkObjectResult okResult)
    {
        var value = okResult.Value!;
        var prop = value.GetType().GetProperty("TotalAmount")!;
        return (decimal)prop.GetValue(value)!;
    }
}