using System;
using System . Collections . Generic;
using System . ComponentModel . DataAnnotations;
using System . Linq;
using System . Threading . Tasks;

namespace Library.API.Models
{
    public abstract class BookForManipulationDto
    {
        [Required ( ErrorMessage = "Title should not be empty." )]
        [MaxLength ( 100 , ErrorMessage = "Title should not exceed 100 characters." )]
        public string Title { get; set; }

        [MaxLength ( 500 , ErrorMessage = "Description should not exceed 500 characters." )]
        public virtual string Description { get; set; }
    }
}
