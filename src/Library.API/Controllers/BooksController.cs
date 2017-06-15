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
    [Route ( "api/authors/{authorId}/books" )]
    public class BooksController : Controller
    {
        ILibraryRepository _repo;

        public BooksController ( ILibraryRepository repo )
        {
            _repo = repo;
        }

        // GET: api/values
        [HttpGet]
        public IActionResult GetBooks ( Guid authorId )
        {
            if ( !_repo . AuthorExists ( authorId ) )
            {
                return NotFound ( );
            }
            var books = _repo.GetBooksForAuthor(authorId);
            return Ok ( Mapper . Map < IEnumerable<BookDto> > ( books ) );
        }

        // GET api/values/5
        [HttpGet ( "{id}" )]
        public string Get ( int id )
        {
            return "value";
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
