using App.Domain;
using App.Service.BllUow;
using App.Service.Dto;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.ApiController;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly IAppBll _bll;

    public CategoryController(IAppBll bll)
    {
        _bll = bll;
    }

    [HttpPost]
    public async Task<ActionResult<Category>> CreateCategory(
        [FromBody] CategoryCreateDto dto)
    {
        await _bll.CategoryService.CreateAsync(dto);
        return Created();
    }
}