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

        private ITypeHelperService _typeHelper;

        public AuthorsController ( ILibraryRepository repo , IUrlHelper urlHelper , IPropertyMappingService propertyService , ITypeHelperService typeHelper )
        {
            _repo = repo;
            _urlHelper = urlHelper;
            _propertyService = propertyService;
            _typeHelper = typeHelper;
        }

        // GET: api/values
        [HttpGet ( Name = "GetAuthors" )]
        public IActionResult Authors ( AuthorResourceParameters authorResourceParameters,[FromHeader(Name ="Accept")] string mediaType )
        {
            if ( !_propertyService . ValidMappingExistsFor<AuthorDto , Author> ( authorResourceParameters . OrderBy ) )
            {
                return BadRequest ( );
            }

            if ( !_typeHelper . TypeHasProperties<AuthorDto> ( authorResourceParameters . Fields ) )
            {
                return BadRequest ( );
            }

            var authors = _repo.GetAuthors(authorResourceParameters);

            if ( mediaType.Equals("application/vnd.marvin.hateoas+json" ))
            {

                var paginationMetaData = new
                {
                    totalCount = authors.TotalCount,
                    pageSize = authors.PageSize,
                    currentPage = authors.CurrentPage,
                    totalPages = authors.TotalPages,

                };

                Response . Headers . Add ( "X-Pagination" , JsonConvert . SerializeObject ( paginationMetaData ) );

                var authorsDto = Mapper.Map<IEnumerable<AuthorDto>>(authors);
                var links  = CreateLinksForAuthors(authorResourceParameters,authors.HasNext,authors.HasPrevious);
                var shapedData = authorsDto.ShapeData(authorResourceParameters.Fields);
                var shapedAuthorWithLinks = shapedData.Select(x =>
            {
                var authorAsDictionary = x as IDictionary<string,object>;
                var authorLinks = CreateLinksForAuthor((Guid)authorAsDictionary["Id"],authorResourceParameters.Fields);
                authorAsDictionary.Add("links",authorLinks);

                return authorAsDictionary;

            });

                var linkedCollectionResource = new
                {
                    value = shapedAuthorWithLinks,
                    links = links
                };

                return Ok ( linkedCollectionResource );
            }
            else
            {
                var previousPageLink = authors.HasPrevious ? CreateAuthorResourceUri(authorResourceParameters,ResourceTypeUri.PreviousPage):null;
                var nextPageLink = authors.HasNext ? CreateAuthorResourceUri(authorResourceParameters,ResourceTypeUri.NextPage):null;

                var paginationMetaData = new
                {
                    previousPageLink=previousPageLink,
                    nextPageLink=nextPageLink,
                    totalCount = authors.TotalCount,
                    pageSize = authors.PageSize,
                    currentPage = authors.CurrentPage,
                    totalPages = authors.TotalPages,

                };

                Response . Headers . Add ( "X-Pagination" , JsonConvert . SerializeObject ( paginationMetaData ) );

                return Ok ( authors . ShapeData ( authorResourceParameters . Fields ) );

            }
        }

        private string CreateAuthorResourceUri ( AuthorResourceParameters authorResourceParameters , ResourceTypeUri type )
        {
            switch ( type )
            {
                case ResourceTypeUri . PreviousPage:
                    return _urlHelper . Link ( "GetAuthors" ,
                        new
                        {
                            fields = authorResourceParameters . Fields ,
                            orderBy = authorResourceParameters . OrderBy ,
                            searchQuery = authorResourceParameters . SearchQuery ,
                            genre = authorResourceParameters . Genre ,
                            pageNumber = authorResourceParameters . PageNumber - 1 ,
                            pageSize = authorResourceParameters . PageSize
                        } );
                case ResourceTypeUri . NextPage:
                    return _urlHelper . Link ( "GetAuthors" ,
                        new
                        {
                            fields = authorResourceParameters . Fields ,
                            orderBy = authorResourceParameters . OrderBy ,
                            searchQuery = authorResourceParameters . SearchQuery ,
                            genre = authorResourceParameters . Genre ,
                            pageNumber = authorResourceParameters . PageNumber + 1 ,
                            pageSize = authorResourceParameters . PageSize
                        } );
                case ResourceTypeUri . Current:
                default:
                    return _urlHelper . Link ( "GetAuthors" ,
                        new
                        {
                            fields = authorResourceParameters . Fields ,
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
        public IActionResult Author ( Guid id , [FromQuery] string fields )
        {
            if ( !_typeHelper . TypeHasProperties<AuthorDto> ( fields ) )
            {
                return BadRequest ( );
            }

            var author = _repo.GetAuthor(id);
            if ( author == null )
                return NotFound ( );
            var links = CreateLinksForAuthor(id,fields);
            var linkedResourceToReturn = Mapper . Map<AuthorDto> ( author . ShapeData ( fields ) ) as IDictionary<string,object>;
            linkedResourceToReturn . Add ( "links" , links );
            return Ok ( linkedResourceToReturn );
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
            var links = CreateLinksForAuthor(authorToReturn.Id,null);
            var linkedResourceToReturn = authorToReturn.ShapeData(null) as IDictionary<string,object>;
            linkedResourceToReturn . Add ( "links" , links );
            return CreatedAtRoute ( "GetAuthor" , new { id = linkedResourceToReturn [ "Id" ] } , linkedResourceToReturn );
        }

        // PUT api/values/5
        [HttpPut ( "{id}" )]
        public void Put ( int id , [FromBody]string value )
        {
        }

        // DELETE api/values/5
        [HttpDelete ( "{id}" , Name = "DeleteAuthor" )]
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

        private IEnumerable<LinkDto> CreateLinksForAuthor ( Guid id , string fields )
        {
            var links = new List<LinkDto>();
            if ( string . IsNullOrWhiteSpace ( fields ) )
            {
                links . Add ( new LinkDto ( _urlHelper . Link ( "GetAuthor" , new { id = id } ) , "self" , "GET" ) );
            }
            else
            {
                links . Add ( new LinkDto ( _urlHelper . Link ( "GetAuthor" , new { id = id , fields = fields } ) , "self" , "GET" ) );
            }

            links . Add ( new LinkDto ( _urlHelper . Link ( "DeleteAuthor" , new { id = id } ) , "delete_book" , "Delete" ) );
            links . Add ( new LinkDto ( _urlHelper . Link ( "CreateBookForAuthor" , new { authorId = id } ) , "create_book_for_author" , "POST" ) );
            links . Add ( new LinkDto ( _urlHelper . Link ( "GetBooksForAuthor" , new { authorId = id } ) , "books" , "GET" ) );

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAuthors ( AuthorResourceParameters paramters , bool hasNext , bool hasPrevious )
        {
            var links = new List<LinkDto>();

            links . Add ( new LinkDto ( CreateAuthorResourceUri ( paramters , ResourceTypeUri . Current ) , "self" , "GET" ) );

            if ( hasNext )
            {
                links . Add ( new LinkDto ( CreateAuthorResourceUri ( paramters , ResourceTypeUri . NextPage ) , "nextPage" , "GET" ) );
            }

            if ( hasPrevious )
            {
                links . Add ( new LinkDto ( CreateAuthorResourceUri ( paramters , ResourceTypeUri . PreviousPage ) , "previousPage" , "GET" ) );
            }


            return links;
        }
    }
}
