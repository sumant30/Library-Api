using System;
using System . Collections . Generic;
using System . ComponentModel . DataAnnotations;
using System . Linq;
using System . Threading . Tasks;

namespace Library.API.Models
{
    public class BookForUpdateDto: BookForManipulationDto
    {
        [Required(ErrorMessage ="Description cannot be empty.")]
        public override string Description
        {
            get
            {
                return base . Description;
            }

            set
            {
                base . Description = value;
            }
        }
    }
}
