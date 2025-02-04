﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Servicios;

namespace PeliculasAPI.Controllers
{
    [Route("api/peliculas")]
    [ApiController]
    public class PeliculasController : CustomBaseController
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private const string cacheTag = "peliculas";
        private readonly string contenedor = "peliculas";

        public PeliculasController(ApplicationDbContext context, IMapper mapper, IOutputCacheStore outputCacheStore, IAlmacenadorArchivos almacenadorArchivos) : base(context, mapper, outputCacheStore, cacheTag)
        {
            this.context = context;
            this.mapper = mapper;
            this.outputCacheStore = outputCacheStore;
            this.almacenadorArchivos = almacenadorArchivos;
        }

        [HttpGet("PostGet")]
        public async Task<ActionResult<PeliculasPostGetDTO>> PostGet()
        {
            var cines = await context.Cines.ProjectTo<CineDTO>(mapper.ConfigurationProvider).ToListAsync();
            var generos = await context.Generos.ProjectTo<GeneroDTO>(mapper.ConfigurationProvider).ToListAsync();

            return new PeliculasPostGetDTO
            {
                Cines = cines,
                Generos = generos
            };
        }

        [HttpGet("{id:int}", Name = "ObtenerPeliculaPorId")]
        //[OutputCache(Tags = [cacheTag])]
        public Task<IActionResult> Get(int id)
        {
            //return await Get<Cine, CineDTO>(id);

            throw new NotImplementedException();
        }

        [HttpGet("landing")]
        public async Task<ActionResult<LandingPageDTO>> Get()
        {
            var top = 6;
            var hoy = DateTime.Today;

            var proximosEstrenos = await context.Peliculas
                                    .Where(p => p.FechaLanzamiento > hoy)
                                    .OrderBy(p => p.FechaLanzamiento)
                                    .Take(top)
                                    .ProjectTo<PeliculaDTO>(mapper.ConfigurationProvider)
                                    .ToListAsync();

            var enCines = await context.Peliculas
                                    .Where(p => p.PeliculasCines.Select(pc => pc.PeliculaId).Contains(p.Id))
                                    .OrderBy(p => p.FechaLanzamiento)
                                    .Take(top)
                                    .ProjectTo<PeliculaDTO>(mapper.ConfigurationProvider)
                                    .ToListAsync();

            return new LandingPageDTO
            {
                ProximosEstrenos = proximosEstrenos,
                EnCines = enCines
            };
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] PeliculaCreacionDTO peliculaCreacionDTO)
        {
            var pelicula = mapper.Map<Pelicula>(peliculaCreacionDTO);

            if(peliculaCreacionDTO.Poster is not null)
            {
                var url = await almacenadorArchivos.Almacenar(contenedor, peliculaCreacionDTO.Poster);
                pelicula.Poster = url;
            }

            AsignarOrdenActores(pelicula);

            context.Add(pelicula);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cacheTag, default);

            var peliculaDTO = mapper.Map<PeliculaDTO>(pelicula);
            return CreatedAtRoute("ObtenerPeliculaPorId", new { id = pelicula.Id }, peliculaDTO);
        }

        private void AsignarOrdenActores(Pelicula pelicula)
        {
            if(pelicula.PeliculasActores is not null)
            {
                for (int i = 0; i < pelicula.PeliculasActores.Count; i++)
                {
                    pelicula.PeliculasActores[i].Orden = i;
                }
            }
        }
    }
}
