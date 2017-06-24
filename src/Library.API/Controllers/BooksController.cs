using System;
using System . Collections . Generic;
using System . Linq;
using System . Threading . Tasks;
using Microsoft . AspNetCore . Mvc;
using Library . API . Services;
using AutoMapper;
using Library . API . Models;
using Library . API . Entities;
using Microsoft . AspNetCore . JsonPatch;
using Library . API . Helpers;
using Microsoft . Extensions . Logging;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Library . API . Controllers
{
    [Route ( "api/authors/{authorId}/books" )]
    public class BooksController : Controller
    {
        ILibraryRepository _repo;
        ILogger<BooksController> _logger;
        IUrlHelper _urlHelper;

        public BooksController ( ILibraryRepository repo , ILogger<BooksController> logger , IUrlHelper urlHelper )
        {
            _repo = repo;
            _logger = logger;
            _urlHelper = urlHelper;
        }

        // GET: api/values
        [HttpGet("",Name ="GetBooksForAuthor")]
        public IActionResult GetBooksForAuthor ( Guid authorId )
        {
            if ( !_repo . AuthorExists ( authorId ) )
            {
                return NotFound ( );
            }
            var books = _repo.GetBooksForAuthor(authorId);
            var booktoReturn =   Mapper . Map<IEnumerable<BookDto>> ( books ) 
                . Select ( x => 
                {
                    x = CreateLinksForBook ( x );
                    return x;
                } ) ;
            var wrapper  = new LinkCollectionResourceWrapperDto<BookDto>(booktoReturn);
            return Ok ( CreateLinksForBooks ( wrapper ) );
        }

        // GET api/values/5
        [HttpGet ( "{id}" , Name = "GetBookForAuthor" )]
        public IActionResult GetBookForAuthor ( Guid authorId , Guid id )
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
            return Ok ( CreateLinksForBook ( Mapper . Map<BookDto> ( book ) ) );
        }

        // POST api/values
        [HttpPost (Name ="CreateBookForAuthor" )]
        public IActionResult Post ( Guid authorId , [FromBody]BookForCreationDto book )
        {
            if ( book == null )
            {
                return BadRequest ( );
            }
            if ( book . Title == book . Description )
            {
                ModelState . AddModelError ( nameof ( BookForCreationDto ) , "Title & Description should be different." );
            }
            if ( !ModelState . IsValid )
            {
                return new UnprocessableEntityObjectResult ( ModelState );
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
                new { authorId = authorId , id = bookEntity . Id } , CreateLinksForBook ( bookToReturn ) );
        }

        // PUT api/values/5
        [HttpPut ( "{id}" , Name = "UpdateBookForAuthor" )]
        public IActionResult UpdateBookForAuthor ( Guid authorId , Guid id , [FromBody]BookForUpdateDto book )
        {
            if ( book == null )
            {
                return BadRequest ( );
            }
            if ( book . Title == book . Description )
            {
                ModelState . AddModelError ( nameof ( BookForUpdateDto ) , "Title & Description should be different." );
            }
            if ( !ModelState . IsValid )
            {
                return new UnprocessableEntityObjectResult ( ModelState );
            }
            if ( !_repo . AuthorExists ( authorId ) )
            {
                return NotFound ( );
            }
            var bookEntity  = _repo.GetBookForAuthor(authorId,id);
            if ( book == null )
            {
                var bookToInsert = Mapper.Map<Book>(book);
                bookToInsert . Id = id;
                _repo . AddBookForAuthor ( authorId , bookToInsert );
                if ( !_repo . Save ( ) )
                {
                    throw new Exception ( $"An exception occured when trying to insert book {id} for author {authorId}" );
                }
                var bookToReturn = Mapper.Map<BookDto>(bookToInsert);
                return CreatedAtRoute ( "GetBookForAuthor" ,
                    new { authorId = authorId , id = bookToReturn . Id } , bookToReturn );

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
        [HttpDelete ( "{id}" , Name = "DeleteBookForAuthor" )]
        public IActionResult DeleteBookForAuthor ( Guid authorId , Guid id )
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
            _logger . LogInformation ( 100 , $"Deleted book {id} for author {authorId}" );
            return NoContent ( );
        }

        [HttpPatch ( "{id}" , Name = "PartiallyUpdateBookForAuthor" )]
        public IActionResult PartiallyUpdateBookForAuthor ( Guid authorId , Guid id , [FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc )
        {
            if ( patchDoc == null )
            {
                return BadRequest ( );
            }
            if ( !_repo . AuthorExists ( authorId ) )
            {
                return NotFound ( );
            }
            var bookEntity  = _repo.GetBookForAuthor(authorId,id);
            if ( bookEntity == null )
            {
                //Upserting with patch
                var bookDto = new BookForUpdateDto();
                patchDoc . ApplyTo ( bookDto , ModelState );

                if ( bookDto . Title == bookDto . Description )
                {
                    ModelState . AddModelError ( nameof ( BookForUpdateDto ) , "Title & Description should be different." );
                }
                TryValidateModel ( bookDto );
                if ( !ModelState . IsValid )
                {
                    return new UnprocessableEntityObjectResult ( ModelState );
                }

                var bookToAdd = Mapper.Map<Book>(bookDto);
                bookToAdd . Id = id;
                _repo . AddBookForAuthor ( authorId , bookToAdd );
                if ( !_repo . Save ( ) )
                {
                    throw new Exception ( $"An exception occured when trying to upsert book {id} for author {authorId}" );
                }
                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute ( "GetBookForAuthor" ,
                    new { authorId = authorId , id = bookToReturn . Id } , bookToReturn );
            }

            var bookToPatch = Mapper.Map<BookForUpdateDto>(bookEntity);
            patchDoc . ApplyTo ( bookToPatch , ModelState );

            if ( bookToPatch . Title == bookToPatch . Description )
            {
                ModelState . AddModelError ( nameof ( BookForUpdateDto ) , "Title & Description should be different." );
            }
            TryValidateModel ( bookToPatch );
            if ( !ModelState . IsValid )
            {
                return new UnprocessableEntityObjectResult ( ModelState );
            }

            Mapper . Map ( bookToPatch , bookEntity );

            _repo . UpdateBookForAuthor ( bookEntity );

            if ( !_repo . Save ( ) )
            {
                throw new Exception ( $"An exception occured when trying to patch book {id} for author {authorId}" );
            }
            return NoContent ( );
        }

        private BookDto CreateLinksForBook ( BookDto bookDto )
        {
            bookDto . Links . Add ( new LinkDto ( _urlHelper . RouteUrl ( "GetBookForAuthor" , new { id = bookDto . Id } ) , "self" , "GET" ) );
            bookDto . Links . Add ( new LinkDto ( _urlHelper . RouteUrl ( "UpdateBookForAuthor" , new { id = bookDto . Id } ) , "update_book" , "PUT" ) );
            bookDto . Links . Add ( new LinkDto ( _urlHelper . RouteUrl ( "DeleteBookForAuthor" , new { id = bookDto . Id } ) , "delete_book" , "DELETE" ) );
            bookDto . Links . Add ( new LinkDto ( _urlHelper . RouteUrl ( "DeleteBookForAuthor" , new { id = bookDto . Id } ) , "partially_update_book" , "PATCH" ) );

            return bookDto;
        }

        private LinkCollectionResourceWrapperDto<BookDto> CreateLinksForBooks( LinkCollectionResourceWrapperDto<BookDto> booksWrapper )
        {

            booksWrapper . Links . Add ( new LinkDto ( _urlHelper . RouteUrl ( "GetBooksForAuthor" , new { } ) , "self" , "GET" ) );

            return booksWrapper;
        }
    }
}
