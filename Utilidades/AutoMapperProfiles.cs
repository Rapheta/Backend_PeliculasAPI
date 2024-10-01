using AutoMapper;
using NetTopologySuite.Geometries;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;

namespace PeliculasAPI.Utilidades
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles(GeometryFactory geometryFactory)
        {
            ConfigurarMapeoActores();
            ConfigurarMapeoGeneros();
            ConfigurarMapeoCines(geometryFactory);
            ConfigurarMapeoPeliculas();
        }

        private void ConfigurarMapeoActores()
        {
            CreateMap<ActorCreacionDTO, Actor>()
                .ForMember(x => x.Foto, opciones => opciones.Ignore());
            CreateMap<Actor, ActorDTO>();
            CreateMap<Actor, PeliculaActorDTO>();
        }

        private void ConfigurarMapeoGeneros()
        {
            CreateMap<GeneroCreacionDTO, Genero>();
            CreateMap<Genero,GeneroDTO>();
        }

        private void ConfigurarMapeoCines(GeometryFactory geometryFactory)
        {
            CreateMap<CineCreacionDTO, Cine>()
                .ForMember(x => x.Ubicacion, cineDTO => cineDTO.MapFrom(x => geometryFactory.CreatePoint(new Coordinate(x.Longitud, x.Latitud))));

            CreateMap<Cine, CineDTO>()
                .ForMember(x => x.Latitud, cine => cine.MapFrom(x => x.Ubicacion.Y))
                .ForMember(x => x.Longitud, cine => cine.MapFrom(x => x.Ubicacion.X));
        }

        private void ConfigurarMapeoPeliculas()
        {
            CreateMap<PeliculaCreacionDTO, Pelicula>()
                .ForMember(x => x.Poster, opciones => opciones.Ignore())
                .ForMember(x => x.PeliculasGeneros, dto => 
                    dto.MapFrom(p => p.GenerosIds!.Select(id => new PeliculaGenero { GeneroId = id }))
                )
                .ForMember(x => x.PeliculasCines, dto =>
                    dto.MapFrom(p => p.CinesIds!.Select(id => new PeliculaCine { CineId = id }))
                )
                .ForMember(p => p.PeliculasActores, dto =>
                    dto.MapFrom(p => p.Actores!.Select(actor => new PeliculaActor { ActorId = actor.Id, Personaje = actor.Personaje }))
                );

            CreateMap<Pelicula, PeliculaDTO>();
        }
    }
}
