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
using Newtonsoft . Json;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Library . API . Controllers
{
    [Route ( "api/authors" )]
    public class AuthorsController : Controller
    {
        ILibraryRepository _repo;

        IUrlHelper _urlHelper;

        private IPropertyMappingService _propertyService;

        public AuthorsController ( ILibraryRepository repo , IUrlHelper urlHelper , IPropertyMappingService propertyService )
        {
            _repo = repo;
            _urlHelper = urlHelper;
            _propertyService = propertyService;
        }

        // GET: api/values
        [HttpGet ( Name = "GetAuthors" )]
        public IActionResult Authors ( AuthorResourceParameters authorResourceParameters )
        {
            if ( !_propertyService . ValidMappingExistsFor<AuthorDto , Author> ( authorResourceParameters . OrderBy ) )
            {
                return BadRequest ( );
            }

            var authors = _repo.GetAuthors(authorResourceParameters);

            var previousPageLink = authors.HasPrevious ? CreateAuthorResourceUri(authorResourceParameters,ResourceTypeUri.PreviousPage):null;
            var nextPageLink = authors.HasNext ? CreateAuthorResourceUri(authorResourceParameters,ResourceTypeUri.NextPage):null;

            var paginationMetaData = new
            {
                totalCount = authors.TotalCount,
                pageSize = authors.PageSize,
                currentPage = authors.CurrentPage,
                totalPages = authors.TotalPages,
                previousPageLink = previousPageLink,
                nextPageLink=nextPageLink
            };

            Response . Headers . Add ( "X-Pagination" , JsonConvert.SerializeObject(paginationMetaData ));
            var authorsDto = Mapper.Map<IEnumerable<AuthorDto>>(authors);
            return Ok ( authorsDto );
        }

        private string CreateAuthorResourceUri ( AuthorResourceParameters authorResourceParameters , ResourceTypeUri type )
        {
            switch ( type )
            {
                case ResourceTypeUri . PreviousPage:
                    return _urlHelper . Link ( "GetAuthors" ,
                        new
                        {
                            orderBy = authorResourceParameters.OrderBy,
                            searchQuery = authorResourceParameters.SearchQuery,
                            genre = authorResourceParameters.Genre,
                            pageNumber = authorResourceParameters . PageNumber - 1 ,
                            pageSize = authorResourceParameters . PageSize
                        } );
                case ResourceTypeUri . NextPage:
                    return _urlHelper . Link ( "GetAuthors" ,
                        new
                        {
                            orderBy = authorResourceParameters . OrderBy ,
                            searchQuery = authorResourceParameters . SearchQuery ,
                            genre = authorResourceParameters . Genre ,
                            pageNumber = authorResourceParameters . PageNumber + 1 ,
                            pageSize = authorResourceParameters . PageSize
                        } );
                default:
                    return _urlHelper . Link ( "GetAuthors" ,
                        new
                        {
                            orderBy = authorResourceParameters . OrderBy ,
                            searchQuery = authorResourceParameters . SearchQuery ,
                            genre = authorResourceParameters . Genre ,
                            pageNumber = authorResourceParameters . PageNumber ,
                            pageSize = authorResourceParameters . PageSize
                        } );
            }
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
