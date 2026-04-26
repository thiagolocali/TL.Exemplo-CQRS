using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TL.ExemploCQRS.Application.Commands.Product.Create;
using TL.ExemploCQRS.Application.Commands.Product.Delete;
using TL.ExemploCQRS.Application.Commands.Product.Update;
using TL.ExemploCQRS.Application.Common;
using TL.ExemploCQRS.Application.DTOs.Product;
using TL.ExemploCQRS.Application.Queries.Product.GetAll;
using TL.ExemploCQRS.Application.Queries.Product.GetById;

namespace TL.ExemploCQRS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Lista produtos com paginação e filtros</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductSummaryResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllProductsQuery(page, pageSize, search, isActive);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<ProductSummaryResponse>>.Ok(result));
    }

    /// <summary>Busca produto pelo ID</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<ProductResponse>.Ok(result));
    }

    /// <summary>Cria um novo produto</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity);

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ProductResponse>.Ok(result, "Produto criado com sucesso."));
    }

    /// <summary>Atualiza um produto existente</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<ProductResponse>.Ok(result, "Produto atualizado com sucesso."));
    }

    /// <summary>Remove um produto (soft delete)</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteProductCommand(id);
        await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse.Ok("Produto removido com sucesso."));
    }
}
