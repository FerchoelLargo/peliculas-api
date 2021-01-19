using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.DTOs
{
    public class PaginacionDTO
    {
        public int Pagina { get; set; } = 1;

        private int itemsPorPagina = 10;
        private readonly int maxItemsPorPagina = 50;
        public int ItemsPorPagina {
            get
            {
                return itemsPorPagina;
            }
            set
            {
                itemsPorPagina = value > maxItemsPorPagina ? maxItemsPorPagina : value;
            }
        }
    }
}
