using AutoMapper;
using back_end.DTOs;
using back_end.Entidades;
using back_end.Utilidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/peliculas")]
    public class PeliculasController : ControllerBase
    {
        private readonly ApplicationDBContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenador;
        private readonly ILogger<PeliculasController> logger;
        private readonly string contenedor = "peliculas";

        public PeliculasController(ApplicationDBContext context, IMapper mapper, IAlmacenadorArchivos almacenador, ILogger<PeliculasController> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenador = almacenador;
            this.logger = logger;
        }

        [HttpGet("{Id:int}")]
        public async Task<ActionResult<PeliculaDTO>> Get (int Id)
        {
            var pelicula = await context.Peliculas
                .Include(pelicula => pelicula.PeliculasGeneros).ThenInclude(peliculaGenero => peliculaGenero.Genero)
                .Include(pelicula => pelicula.PeliculasCines ).ThenInclude(peliculaCine => peliculaCine.Cine)
                .Include(pelicula => pelicula.PeliculasActores).ThenInclude(peliculaActor => peliculaActor.Actor)
                .FirstOrDefaultAsync(pelicula => pelicula.Id == Id );
            if(pelicula == null)
            {
                return NotFound();
            }
            var dto = mapper.Map<PeliculaDTO>(pelicula);
            dto.Actores = dto.Actores.OrderBy(actor => actor.Orden).ToList();
            return dto;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] PeliculaCreacionDTO peliculaCreacionDTO)
        {
            var pelicula = mapper.Map<Pelicula>(peliculaCreacionDTO);
            if (peliculaCreacionDTO.Poster != null)
            {
                pelicula.Poster = await almacenador.GuardarArchivo(contenedor, peliculaCreacionDTO.Poster);
            }
            EscribirOrdenActores(pelicula);

            context.Peliculas.Add(pelicula);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("LandingData")]
        public async Task<ActionResult<LandingDataDTO>> LandingData()
        {
            int top = 6;
            DateTime hoy = DateTime.Today;
            var proximosEstrenos = await context.Peliculas
                .Where(pelicula => pelicula.FechaLanzamiento > hoy)
                .OrderBy(pelicula => pelicula.FechaLanzamiento)
                .Take(top)
                .ToListAsync();

            var enCines = await context.Peliculas
                .Where(pelicula => pelicula.EnCines)
                .OrderBy(pelicula => pelicula.FechaLanzamiento)
                .Take(top)
                .ToListAsync();

            var result = new LandingDataDTO();
            result.EnCines = mapper.Map<List<PeliculaDTO>>(enCines);
            result.ProximosEstrenos = mapper.Map<List<PeliculaDTO>>(proximosEstrenos);
            return result;
        }

        private void EscribirOrdenActores(Pelicula pelicula)
        {
            if(pelicula.PeliculasActores != null)
            {
                for(int i = 0; i< pelicula.PeliculasActores.Count; i++)
                {
                    pelicula.PeliculasActores[i].Orden = i;
                }
            }
        }

        [HttpGet("PostGet")]
        public async Task<ActionResult<PeliculasPostGetDTO>> PostGet()
        {
            var cines = await context.Cines.ToListAsync();
            var generos = await context.Generos.ToListAsync();

            var cinesDTO = mapper.Map<List<CineDTO>>(cines);
            var generosDTO = mapper.Map<List<GeneroDTO>>(generos);

            return new PeliculasPostGetDTO() { Cines = cinesDTO, Generos = generosDTO };
        }


        [HttpGet("PutGet/{Id:int}")]
        public async Task<ActionResult<PeliculasPutGetTO>> PutGet(int Id)
        {
            var peliculaActionResult = await Get(Id);
            if (peliculaActionResult.Result is NotFoundResult) return NotFound();
            
            var pelicula = peliculaActionResult.Value;
            
            var generosSeleccionadosIds = pelicula.Generos.Select(genero => genero.Id);
            var generosNoSeleccionados = await context.Generos
                .Where(genero => !generosSeleccionadosIds.Contains(genero.Id))
                .ToListAsync();

            var cinesSeleccionadosIds = pelicula.Cines.Select(cine => cine.Id);
            var cinesNoSeleccionados = await context.Cines
                .Where(cine => !cinesSeleccionadosIds.Contains(cine.Id))
                .ToListAsync();

            var respuesta = new PeliculasPutGetTO();
            respuesta.CinesSeleccionados = pelicula.Cines;
            respuesta.CinesNoSeleccionados = mapper.Map<List<CineDTO>>(cinesNoSeleccionados);
            respuesta.GenerosSeleccionados = pelicula.Generos;
            respuesta.GenerosNoSeleccionados = mapper.Map<List<GeneroDTO>>(generosNoSeleccionados);
            respuesta.Actores = pelicula.Actores;
            respuesta.Pelicula = pelicula;
            return respuesta;
        }


        [HttpPut("{Id:int}")]
        public async Task<ActionResult<Pelicula>> Put(int Id, [FromForm] PeliculaCreacionDTO peliculaCreacionDTO)
        {
            var pelicula = await context.Peliculas
                .Include(pelicula => pelicula.PeliculasActores)
                .Include(pelicula => pelicula.PeliculasGeneros)
                .Include(pelicula => pelicula.PeliculasCines)
                .FirstOrDefaultAsync(pelicula => pelicula.Id == Id);
            if(pelicula == null)
            {
                return NotFound();
            }

            pelicula = mapper.Map(peliculaCreacionDTO,pelicula);
            if(peliculaCreacionDTO.Poster != null)
            {
                pelicula.Poster = await almacenador.EditararArchivo(contenedor, peliculaCreacionDTO.Poster, pelicula.Poster);
            }
            EscribirOrdenActores(pelicula);
            //await context.SaveChangesAsync();
            return pelicula;
            return NoContent();
        }

    }
}
