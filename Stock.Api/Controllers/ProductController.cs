using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Stock.Api.DTOs;
using Stock.Api.Extensions;
using Stock.AppService.Services;
using Stock.Model.Entities;

namespace Stock.Api.Controllers
{
 [Produces("application/json")]
 [Route("api/product")]
 [ApiController]

 public class ProductController: ControllerBase
 {
     private ProductService service;
     private readonly IMapper mapper;

     public ProductController(ProductService service, IMapper mapper)
     {
         this.service=service;
         this.mapper=mapper;
     }

     /// <sumary>
     /// Permite crear una nueva instancias
     /// </sumary>
     /// <param name="value">Una nueva instancias</param>
     [HttpPost]
     public ActionResult Post([FromBody] ProductDTO value)
     {
         TryValidateModel(value);
         
        try
        {
            var product= this.mapper.Map<Product>(value);
            this.service.Create(product);
            value.Id=product.Id;
            return Ok(new {Success=true, Message="", data=value});
        }
        catch
        {
            return Ok(new {Success=false, Message="The name is already in use"});
        }
     }

     /// <sumary>
     /// Permite recuperar todas las instancias
     /// </sumary>
     /// <returns>Una coleccion de instancias</returns>
     [HttpGet]
     public ActionResult<IEnumerable<ProductDTO>> Get()
     {
         try
         {
             var result= this.service.GetAll();
             return this.mapper.Map<IEnumerable<ProductDTO>>(result).ToList();
         }
         catch(Exception)
         {
             return StatusCode(500);
         }
     }

     /// <sumary>
     /// Permite recuperar una isntancia mediante un identificador
     /// </sumary>
     /// <param name="id">Identificador de la instacia</param>
     /// <return>Una instancia</return>
     [HttpGet("{id}")]
     public ActionResult<ProductDTO> Get(string id)
     {
         try
         {
             var result= this.service.Get(id);
             return this.mapper.Map<ProductDTO>(result);
         }
         catch(Exception)
         {
             return StatusCode(500);
         }
     }
     
     /// <sumary>
     /// Permite borrar una instancia
     /// </sumary>
     /// <param name="id">Identificador de la instacia</param>
     [HttpDelete("{id}")]
     public ActionResult Delete(string id)
     {
         try
         {
             var product= this.service.Get(id);
             this.service.Delete(product);
             return Ok(new {Success= true, Message="", data=id});
         }
         catch
         {
             return Ok(new {Success=false, Message="", data=id});
         }
     }

     /// <sumary>
     /// Permite editar un instancia
     /// </sumary>
     /// <param name="id">Identificador de la instancia a editar</param>
     /// <param name="value">Instancia con los nuevos datos</param>
     [HttpPut("{id}")]
     public void Put(string id, [FromBody] ProductDTO value)
     {
         var product= this.service.Get(id);
         TryValidateModel(value);
         this.mapper.Map<ProductDTO, Product>(value, product);
         this.service.Update(product);
     }

     /// <sumary>
     /// Permite recuperar todas las instancias que cumplan con un filtro
     /// </sumary>
     /// <param name="model">Valores del filtro a utilizar</param>
     /// <return>Una coleccion de instancias</return>
     [HttpPost("search")]
     public ActionResult Search([FromBody] ProductSearchDTO model)
     {
         Expression<Func<Product, bool>> filter= x => !string.IsNullOrWhiteSpace(x.Id);

         if(!string.IsNullOrWhiteSpace(model.Name))
         {
             filter= filter.AndOrCustom(
                 x => x.Name.ToUpper().Contains(model.Name.ToUpper()),
                 model.Condition.Equals(ActionDto.AND));
         }

         if(!string.IsNullOrWhiteSpace(model.ProductTypeId))
         {
             filter= filter.AndOrCustom(
                 x => x.ProductType.Id.ToUpper().Contains(model.ProductTypeId.ToUpper()),
                 model.Condition.Equals(ActionDto.AND));
         }
         var product= this.service.Search(filter);
         return Ok(product);
         
     }
 }
    
}