using System;
using System.ComponentModel.DataAnnotations;

namespace ImageGallery.API.Entities
{

    // sle note: EF step 1. define the EF Entities to be mapped into EF generated database tables ((connection string found in Startup.cs)
    public class Image
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        [MaxLength(200)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(50)]
        public string OwnerId { get; set; }
    }
}
