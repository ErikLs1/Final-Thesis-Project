using App.Service;
using App.Service.BllUow;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.ApiController;


[ApiController]
public class ProductController : ControllerBase
{
    private readonly IAppBll _bll;

    public ProductController(IAppBll bll)
    {
        _bll = bll;
    }
}