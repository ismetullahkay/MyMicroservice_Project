using DiscountService.Models;
using DiscountService.Respository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiscountService.Controllers;
[ApiController]
[Route("[controller]")]
public class DiscountController:ControllerBase
{
    private readonly IDiscountRespository _repository;
    
    public DiscountController(IDiscountRespository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreateDiscount(DiscountModel model)
    {
        var response=await _repository.CreateDiscount(model);
        return Ok(response);
    }
}