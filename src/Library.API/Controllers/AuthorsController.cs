using System;
using System . Collections . Generic;
using System . Linq;
using System . Threading . Tasks;
using Microsoft . AspNetCore . Mvc;
using Library . API . Services;
using AutoMapper;
using Library . API . Models;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Library . API . Controllers
{
    [Route ( "api/authors" )]
    public class AuthorsController : Controller
    {
        ILibraryRepository _repo;

        public AuthorsController ( ILibraryRepository repo )
        {
            _repo = repo;
        }

        // GET: api/values
        [HttpGet]
        public IActionResult Authors ( )
        {
            var authors = _repo.GetAuthors();
            var authorsDto = Mapper.Map<IEnumerable<AuthorDto>>(authors);
            return Ok ( authorsDto );
        }

        // GET api/values/5
        [HttpGet ( "{id}" )]
        public IActionResult Author ( Guid id )
        {
            var author = _repo.GetAuthor(id);
            if ( author == null )
                return NotFound ( );
            else
                return Ok ( Mapper . Map<AuthorDto> ( author ) );
        }

        // POST api/values
        [HttpPost]
        public void Post ( [FromBody]string value )
        {
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
