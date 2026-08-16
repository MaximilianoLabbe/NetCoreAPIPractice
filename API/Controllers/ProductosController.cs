using API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;
using API.DTOs;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<PagedResultDto<ProductoDto>>> GetProductos([FromQuery] ProductoQueryDto filtros)
        {

            var query = _context.Productos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtros.Nombre))
            {
                query = query.Where(p => p.Nombre.Contains(filtros.Nombre));
            }

            if (filtros.PrecioMaximo.HasValue)
            {
                query = query.Where(p => p.Precio <= filtros.PrecioMaximo.Value);
            }

            var totalRegistros = await query.CountAsync();

            var productos = await query
                .OrderBy(p => p.Id)
                .Skip((filtros.Pagina - 1) * filtros.TamanioPagina)
                .Take(filtros.TamanioPagina)
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    FechaCreacion = p.FechaCreacion
                })
                .ToListAsync();

            return Ok(new PagedResultDto<ProductoDto>
            {
                TotalRegistros = totalRegistros,
                Pagina = filtros.Pagina,
                TamanioPagina = filtros.TamanioPagina,
                TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)filtros.TamanioPagina),
                Datos = productos
            });
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id)
        {
            var producto = await _context.Productos
                .Where(p => p.Id == id)
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    FechaCreacion = p.FechaCreacion
                })
                .FirstOrDefaultAsync();

            if (producto == null)
            {
                return NotFound();
            }

            return Ok(producto);
        }

        [HttpPost]
        public async Task<ActionResult<ProductoDto>> CreateProducto(CreateProductoDto dto)
        {
            var producto = new Producto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                Stock = dto.Stock
            };

            _context.Productos.Add(producto);

            await _context.SaveChangesAsync();

            var productoDto = new ProductoDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Stock = producto.Stock,
                FechaCreacion = producto.FechaCreacion
            };

            return CreatedAtAction(
                nameof(GetProducto),
                new { id = producto.Id },
                productoDto
            );
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProducto(int id, UpdateProductoDto dto)
        {
            var productoExistente = await _context.Productos.FindAsync(id);

            if (productoExistente == null)
            {
                return NotFound();
            }

            productoExistente.Nombre = dto.Nombre;
            productoExistente.Descripcion = dto.Descripcion;
            productoExistente.Precio = dto.Precio;
            productoExistente.Stock = dto.Stock;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound();
            }

            _context.Productos.Remove(producto);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}