﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DeepDiveLibraryApi.Entities
{
    public class Course
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(1500)]
        public string? Description { get; set; }

        [ForeignKey("AuthorId")]
        public Author Author { get; set; } = null!;

        public Guid AuthorId { get; set; }

        public Course(string title)
        {
            Title = title;
        }
    }
}
