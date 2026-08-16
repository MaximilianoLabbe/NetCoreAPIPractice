using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CreateProductoDto
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        [StringLength(500)]
        public string Descripcion { get; set; } = string.Empty;
        [Range(0.01, double.MaxValue)]
        public decimal Precio { get; set; }
        [Range(0, int.MaxValue)]
        public int Stock { get; set; } = 0;
    }
}
