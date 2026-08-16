using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ProductoQueryDto
    {
        public string? Nombre { get; set; }

        public decimal? PrecioMaximo { get; set; }

        [Range(1, int.MaxValue)]
        public int Pagina { get; set; } = 1;

        [Range(1, 100)]
        public int TamanioPagina { get; set; } = 10;
    }
}