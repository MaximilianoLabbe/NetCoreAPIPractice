namespace API.DTOs
{
    public class PagedResultDto<T>
    {
        public int TotalRegistros { get; set; }

        public int Pagina { get; set; }

        public int TamanioPagina { get; set; }

        public int TotalPaginas { get; set; }

        public IEnumerable<T> Datos { get; set; } = [];
    }
}