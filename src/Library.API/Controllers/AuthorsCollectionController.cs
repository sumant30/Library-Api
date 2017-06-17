using System;
using System . Collections . Generic;
using System . Linq;
using System . Threading . Tasks;
using Microsoft . AspNetCore . Mvc;
using Library . API . Services;
using Library . API . Models;
using AutoMapper;
using Library . API . Entities;
using Library . API . Helpers;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Library . API . Controllers
{
    [Route ( "api/authorscollection" )]
    public class AuthorsCollectionController : Controller
    {
        ILibraryRepository _repo;

        public AuthorsCollectionController ( ILibraryRepository repo )
        {
            _repo = repo;
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get ( )
        {
            return new string [ ] { "value1" , "value2" };
        }

        // GET api/values/5
        [HttpGet ( "({ids})",Name ="GetAuthorCollection" )]
        public IActionResult Get ([ModelBinder(BinderType =typeof(ArrayModelBinder))] IEnumerable<Guid> ids )
        {
           if(ids == null )
            {
                return BadRequest ( );
            }
            var authorEntities = _repo.GetAuthors(ids);
            if(ids.Count() != authorEntities . Count() )
            {
                return NotFound ( );
            }
            var authorDtos = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok ( authorDtos );
        }

        // POST api/values
        [HttpPost]
        public IActionResult CreateAuthorCollection ( [FromBody]IEnumerable<AuthorForCreationDto> authorsDto)
        {
            if ( authorsDto == null )
            {
                return BadRequest ( );
            }
            var authorsEntity  = Mapper.Map<IEnumerable<Author>>(authorsDto);
            authorsEntity . ToList ( ) . ForEach ( x => { _repo . AddAuthor ( x ); } );
            if ( !_repo . Save ( ) )
            {
                throw new Exception ( "Creating an author collection failed on save." );
            }
            var authorDtos = Mapper.Map<IEnumerable<AuthorDto>>(authorsEntity);
            var authorIds = string.Join(",",authorDtos.Select(x => x.Id));
            return CreatedAtRoute ( "GetAuthorCollection" , new { ids = authorIds } , authorDtos );
        }

        // PUT api/values/5
        [HttpPut ( "{id}" )]
        public void Put ( int id , [FromBody]string value )
        {
        }

        // DELETE api/values/5
        [HttpDelete ( "{id}" )]
        public void Delete ( int id )
        {
        }
    }
}
