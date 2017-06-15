﻿using System;
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
        [HttpGet ( "{id}",Name ="GetAuthor" )]
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
        public void Delete ( int id )
        {
        }
    }
}
