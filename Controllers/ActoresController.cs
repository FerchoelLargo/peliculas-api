using AutoMapper;
using back_end.DTOs;
using back_end.Entidades;
using back_end.Utilidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Controllers
{
    [Route("api/actores")]
    [ApiController]
    public class ActoresController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenador;
        private readonly string contenedor = "actores";

        public ActoresController(ApplicationDBContext context, IMapper mapper, IAlmacenadorArchivos almacenador)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenador = almacenador;
        }

        [HttpGet]
        public async Task<ActionResult<List<ActorDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = context.Actores.AsQueryable();
            await HttpContext.InsertarDatosPaginacionHeader(queryable);
            var actores = await queryable.OrderBy(x => x.Nombre).Paginar(paginacionDTO).ToListAsync();
            return mapper.Map<List<ActorDTO>>(actores);
        }

        [HttpGet("{Id:int}")]
        public async Task<ActionResult<ActorDTO>> Get(int Id)
        {
            var actor = await context.Actores.FirstOrDefaultAsync(item => item.Id == Id);
            if (actor == null)
            {
                return NotFound();
            }
            return mapper.Map<ActorDTO>(actor);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] ActorCreacionDTO actorCreacionDTO)
        {
            var actor = mapper.Map<Actor>(actorCreacionDTO);
            if( actorCreacionDTO.Foto != null)
            {
                actor.Foto = await almacenador.GuardarArchivo(contenedor, actorCreacionDTO.Foto);
            }
            context.Actores.Add(actor);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{Id:int}")]
        public async Task<ActionResult> Put(int Id, [FromForm] ActorCreacionDTO actorCreacionDTO)
        {
            var actor = await context.Actores.FirstOrDefaultAsync(item => item.Id == Id);
            if (actor == null)
            {
                return NotFound();
            }
            actor = mapper.Map(actorCreacionDTO, actor);
            if(actorCreacionDTO.Foto != null)
            {
                actor.Foto = await almacenador.EditararArchivo(contenedor, actorCreacionDTO.Foto, actor.Foto);
            }
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{Id:int}")]
        public async Task<ActionResult> Delete(int Id)
        {

            var actor = await context.Actores.FirstOrDefaultAsync(item => item.Id == Id);
            if (actor == null)
            {
                return NoContent();
            }
            await almacenador.BorrarArchivo(actor.Foto, contenedor);
            context.Actores.Remove(actor);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("obtenerPorNombre")]
        public async Task<ActionResult<List<PeliculaActorDTO>>> ObtenerPorNombre([FromHeader] string toSearch)
        {
            if (string.IsNullOrWhiteSpace(toSearch))
            {
                return new List<PeliculaActorDTO>();
            }
            toSearch = toSearch.ToLower();
            return await context.Actores
                .Where(actor => actor.Nombre.ToLower().Contains(toSearch))
                .Select(actor => new PeliculaActorDTO() { Nombre = actor.Nombre, Foto = actor.Foto, Id = actor.Id })
                .Take(5)
                .ToListAsync();
        }

    }
}
