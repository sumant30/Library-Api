using System;
using System . Collections . Generic;
using System . Linq;
using System . Threading . Tasks;
using Microsoft . AspNetCore . Mvc;
using Library . API . Services;
using AutoMapper;
using Library . API . Models;
using Library . API . Entities;

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
            return Ok ( Mapper . Map<IEnumerable<BookDto>> ( books ) );
        }

        // GET api/values/5
        [HttpGet ( "{id}" , Name = "GetBookForAuthor" )]
        public IActionResult Get ( Guid authorId , Guid id )
        {
            if ( !_repo . AuthorExists ( authorId ) )
            {
                return NotFound ( );
            }
            var book = _repo.GetBookForAuthor(authorId,id);
            if ( book == null )
            {
                return NotFound ( );
            }
            return Ok ( Mapper . Map<BookDto> ( book ) );
        }

        // POST api/values
        [HttpPost ( )]
        public IActionResult Post ( Guid authorId , [FromBody]BookForCreationDto book )
        {
            if ( book == null )
            {
                return BadRequest ( );
            }
            if ( !_repo . AuthorExists ( authorId ) )
            {
                return BadRequest ( );
            }

            var bookEntity = Mapper.Map<Book>(book);

            _repo . AddBookForAuthor ( authorId , bookEntity );
            if ( !_repo . Save ( ) )
            {
                throw new Exception ( $"Creating book failed for {authorId}" );
            }
            var bookToReturn = Mapper.Map<BookDto>(bookEntity);
            return CreatedAtRoute ( "GetBookForAuthor" ,
                new { authorId = authorId , id = bookEntity . Id } , bookToReturn );
        }

        // PUT api/values/5
        [HttpPut ( "{id}" )]
        public IActionResult Put ( Guid authorId , Guid id , [FromBody]BookForUpdateDto book )
        {
            if ( book == null )
            {
                return BadRequest ( );
            }
            if ( !_repo . AuthorExists ( authorId ) )
            {
                return NotFound ( );
            }
            var bookEntity  = _repo.GetBookForAuthor(authorId,id);
            if ( book == null )
            {
                return NotFound ( );
            }
            Mapper . Map ( book , bookEntity );
            _repo . UpdateBookForAuthor ( bookEntity );
            if ( !_repo . Save ( ) )
            {
                throw new Exception ( $"An exception occured when trying to update book {id} for author {authorId}" );
            }
            return NoContent ( );
        }

        // DELETE api/values/5
        [HttpDelete ( "{id}" )]
        public IActionResult Delete ( Guid authorId , Guid id )
        {
            if ( !_repo . AuthorExists ( authorId ) )
            {
                return NotFound ( );
            }
            var book  = _repo.GetBookForAuthor(authorId,id);
            if ( book == null )
            {
                return NotFound ( );
            }
            _repo . DeleteBook ( book );
            if ( !_repo . Save ( ) )
            {
                throw new Exception ( $"An error when deleting book {id} for author {authorId}" );
            }
            return NoContent ( );
        }
    }
}
