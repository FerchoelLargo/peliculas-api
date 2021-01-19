using AutoMapper;
using back_end.DTOs;
using back_end.Entidades;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.Utilidades
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles(GeometryFactory geometryFactory)
        {
            CreateMap<Genero, GeneroDTO>().ReverseMap();
            CreateMap<GeneroCreacionDTO, Genero>();

            CreateMap<Actor, ActorDTO>().ReverseMap();
            CreateMap<ActorCreacionDTO, Actor>().ForMember(x => x.Foto, options => options.Ignore());

            CreateMap<CineCreacionDTO, Cine>().
                ForMember(cine => cine.Ubicacion,
                    opciones => opciones.MapFrom(
                        cineCreacionDTO => geometryFactory.CreatePoint(new Coordinate(cineCreacionDTO.Longitud, cineCreacionDTO.Latitud))
                    )
                );
            CreateMap<Cine, CineDTO>()
                .ForMember(cineDTO => cineDTO.Latitud, cines => cines.MapFrom(cine => cine.Ubicacion.Y))
                .ForMember(cineDTO => cineDTO.Longitud, cines => cines.MapFrom(cine => cine.Ubicacion.X));

            CreateMap<PeliculaCreacionDTO, Pelicula>()
                .ForMember(pelicula => pelicula.Poster, opciones => opciones.Ignore())
                .ForMember(pelicula => pelicula.PeliculasGeneros, opciones => opciones.MapFrom(MapearPeliculasGeneros))
                .ForMember(pelicula => pelicula.PeliculasCines, opciones => opciones.MapFrom(MapearPeliculasCines))
                .ForMember(pelicula => pelicula.PeliculasActores, opciones => opciones.MapFrom(MapearPeliculasActores));

            CreateMap<Pelicula, PeliculaDTO>()
                .ForMember(peliculaDTO => peliculaDTO.Generos, options => options.MapFrom(MapearPeliculasGeneros))
                .ForMember(peliculaDTO => peliculaDTO.Cines, options => options.MapFrom(MapearPeliculasCines))
                .ForMember(peliculaDTO => peliculaDTO.Actores, options => options.MapFrom(MapearPeliculasActores));
            //CreateMap< desde , hacia>()
        }

        /*Funciones de mapeo Pelicula --> PeliculaDTO */

        private List<CineDTO> MapearPeliculasCines(Pelicula pelicula, PeliculaDTO peliculaDTO)
        {
            var result = new List<CineDTO>();
            if(pelicula.PeliculasCines != null)
            {
                foreach(var peliculaCine in pelicula.PeliculasCines)
                {
                    result.Add(new CineDTO()
                    {
                        Id = peliculaCine.CineId,
                        Nombre = peliculaCine.Cine.Nombre,
                        Latitud = peliculaCine.Cine.Ubicacion.Y ,
                        Longitud = peliculaCine.Cine.Ubicacion.X,
                    });
                }
            }
            return result;
        }

        private List<PeliculaActorDTO> MapearPeliculasActores(Pelicula pelicula, PeliculaDTO peliculaDTO)
        {
            var result = new List<PeliculaActorDTO>();
            if(pelicula.PeliculasActores != null)
            {
                foreach(var peliculaActor in pelicula.PeliculasActores)
                {
                    result.Add(new PeliculaActorDTO()
                    {
                        Id = peliculaActor.ActorId,
                        Nombre = peliculaActor.Actor.Nombre,
                        Foto = peliculaActor.Actor.Foto,
                        Personaje = peliculaActor.Personaje,
                        Orden = peliculaActor.Orden,
                    }) ;
                }
            }
            return result;
        }

        private List<GeneroDTO> MapearPeliculasGeneros(Pelicula pelicula, PeliculaDTO peliculaDTO)
        {
            var result = new List<GeneroDTO>();
            if(pelicula.PeliculasGeneros != null)
            {
                foreach(var peliculaGenero in pelicula.PeliculasGeneros)
                {
                    result.Add(new GeneroDTO() { Id = peliculaGenero.GeneroId, Nombre = peliculaGenero.Genero.Nombre });
                }
            }
            return result;
        }

        /*Funciones de mapeo PeliculaCreacionDTO --> Pelicula*/

        private List<PeliculasActores> MapearPeliculasActores(PeliculaCreacionDTO peliculaCreacionDTO, Pelicula pelicula)
        {
            var resultado = new List<PeliculasActores>();
            if (peliculaCreacionDTO.GenerosIds == null)
            {
                return resultado;
            }
            foreach (var actor in peliculaCreacionDTO.Actores )
            {
                resultado.Add(new PeliculasActores() { ActorId = actor.Id, Personaje = actor.Personaje });
            }
            return resultado;
        }

        private List<PeliculasGeneros> MapearPeliculasGeneros(PeliculaCreacionDTO peliculaCreacionDTO,Pelicula pelicula)
        {
            var resultado = new List<PeliculasGeneros>();
            if( peliculaCreacionDTO.GenerosIds == null)
            {
                return resultado;
            }
            foreach(var id in peliculaCreacionDTO.GenerosIds)
            {
                resultado.Add(new PeliculasGeneros() { GeneroId = id });
            }
            return resultado;
        }

        private List<PeliculasCines> MapearPeliculasCines(PeliculaCreacionDTO peliculaCreacionDTO, Pelicula pelicula)
        {
            var resultado = new List<PeliculasCines>();
            if (peliculaCreacionDTO.CinesIds == null)
            {
                return resultado;
            }
            foreach (var id in peliculaCreacionDTO.CinesIds)
            { 
                resultado.Add(new PeliculasCines() { CineId = id });
            }
            return resultado;
        }


    }
}
