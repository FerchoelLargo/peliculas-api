using AutoMapper;
using back_end.DTOs;
using back_end.Entidades;
using back_end.Filtros;
using back_end.Utilidades;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Controllers
{
    [Route("api/generos")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class GenerosController:ControllerBase
    {
        private readonly ILogger<GenerosController> logger;
        private readonly ApplicationDBContext context;
        private readonly IMapper mapper;

        public GenerosController(ILogger<GenerosController> logger , ApplicationDBContext context, IMapper mapper ){
            this.logger = logger;
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<GeneroDTO>>> Get( [FromQuery] PaginacionDTO paginacionDTO )
        {
            var queryable = context.Generos.AsQueryable();
            await HttpContext.InsertarDatosPaginacionHeader(queryable);
            var generos = await queryable.OrderBy(x => x.Nombre).Paginar(paginacionDTO).ToListAsync();
            return mapper.Map<List<GeneroDTO>>(generos);
        }

        [HttpGet("{Id:int}")]
        public async Task<ActionResult<GeneroDTO>> Get(int Id)
        {
            var genero = await context.Generos.FirstOrDefaultAsync(item => item.Id == Id);
            if(genero == null)
            {
                return NotFound();
            }
            return mapper.Map<GeneroDTO>(genero);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] GeneroCreacionDTO generoCreacionDTO)
        {
            Genero genero = mapper.Map<Genero>(generoCreacionDTO);
            context.Generos.Add(genero);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{Id:int}")]
        public async Task<ActionResult> Put(int Id, GeneroCreacionDTO generoCreacionDTO)
        {
            var genero = await context.Generos.FirstOrDefaultAsync(item => item.Id == Id);
            if (genero == null)
            {
                return NotFound();
            }
            genero = mapper.Map(generoCreacionDTO,genero);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{Id:int}")]
        public async Task<ActionResult> Delete(int Id)
        {
            var existe = await context.Generos.AnyAsync(item => item.Id == Id);
            if (!existe)
            {
                return NoContent();
            }
            context.Generos.Remove(new Genero() { Id = Id });
            await context.SaveChangesAsync();
            return NoContent();
        }

    }
}
