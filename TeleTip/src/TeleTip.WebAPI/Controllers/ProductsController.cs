using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyProject.Application.Features.Products.Commands.CreateProduct;
using MyProject.Application.Features.Products.Queries.GetAllProducts;
using MyProject.Application.Features.Products.Queries.GetProductById;
using MyProject.Domain.Abstractions;

namespace MyProject.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class ProductsController(ISender sender) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetAllProductsQuery(), cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetProductByIdQuery(id), cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
                : BadRequest(result.Error);
        }
    }
}
