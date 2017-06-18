using System;
using System . Collections . Generic;
using System . Linq;
using System . Threading . Tasks;
using Microsoft . AspNetCore . Mvc;
using Library . API . Services;
using AutoMapper;
using Library . API . Models;
using Library . API . Entities;
using Microsoft . AspNetCore . Http;
using Library . API . Helpers;

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
        public IActionResult Authors ( AuthorResourceParameters authorResourceParameters )
        {
            
            var authors = _repo.GetAuthors(authorResourceParameters);
            var authorsDto = Mapper.Map<IEnumerable<AuthorDto>>(authors);
            return Ok ( authorsDto );
        }

        // GET api/values/5
        [HttpGet ( "{id}" , Name = "GetAuthor" )]
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
        public IActionResult CreateAuthor ( [FromBody]AuthorForCreationDto author )
        {
            if ( author == null )
            {
                return BadRequest ( );
            }
            var authorEntity  = Mapper.Map<Author>(author);
            _repo . AddAuthor ( authorEntity );
            if ( !_repo . Save ( ) )
            {
                throw new Exception ( "Creating an author failed on save." );
            }
            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);
            return CreatedAtRoute ( "GetAuthor" , new { id = authorToReturn . Id } , authorToReturn );
        }

        // PUT api/values/5
        [HttpPut ( "{id}" )]
        public void Put ( int id , [FromBody]string value )
        {
        }

        // DELETE api/values/5
        [HttpDelete ( "{id}" )]
        public IActionResult Delete ( Guid id )
        {
            var author = _repo.GetAuthor(id);
            if ( author == null )
            {
                return NotFound ( );
            }
            _repo . DeleteAuthor ( author );
            if ( !_repo . Save ( ) )
            {
                throw new Exception ( $"An error occured when trying to delete author {id}" );
            }
            return NoContent ( );
        }

        [HttpPost ( "{id}" )]
        public IActionResult BlockAuthorCreation ( Guid id )
        {
            if ( _repo . AuthorExists ( id ) )
            {
                return new StatusCodeResult ( StatusCodes . Status409Conflict );
            }
            return NotFound ( );
        }
    }
}
