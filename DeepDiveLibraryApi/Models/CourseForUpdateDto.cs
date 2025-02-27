using System.ComponentModel.DataAnnotations;

namespace DeepDiveLibraryApi.Models
{
    public class CourseForUpdateDto : CourseForManipulationDto
    {

        //[Required(ErrorMessage = "You should fill out a title.")]
        //[MaxLength(100, ErrorMessage = "The title shouldn't have more than 100 characters.")]
        //public string Title { get; set; } = string.Empty;

        //[MaxLength(1500, ErrorMessage = "The description shouldn't have more than 1500 characters.")]


        // its not ideal its not make sense implement this member in the class as its already in the base class. it doesnt add anything (duplicate code)
        //public override string Description { get => throw new NotImplementedException(); } 

        [Required(ErrorMessage = "You should fill out a description.")]
        public override string Description
        {
            get => base.Description; set => base.Description = value;
        }
    }
}
