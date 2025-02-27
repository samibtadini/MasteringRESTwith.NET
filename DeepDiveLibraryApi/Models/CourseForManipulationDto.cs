using DeepDiveLibraryApi.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace DeepDiveLibraryApi.Models
{
    [CourseTitleMustBeDifferentFromDescriptionAttribute]
    public abstract class CourseForManipulationDto //: IValidatableObject
    {
        [Required(ErrorMessage = "You should fill out a title.")]
        [MaxLength(100, ErrorMessage = "The title shouldn't have more than 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1500, ErrorMessage = "The description shouldn't have more than 1500 characters.")]

        // virtual is great better than abstract if you have an implementation in the base class we do want to ovveride it if nesseccary
        public virtual string Description { get; set; } = string.Empty;


        // lvl up our validation 

        // note: this will not execute or on other world we dont see the message stating that the description should be differnet 
        // from the title. the reason for this is that the validate method is not executed when one of the data valid annotation already 
        // resulted in the object being invalid

        //-----------------------------------------------
        //public IEnumerable<ValidationResult> Validate(
        //ValidationContext validationContext)
        //{
        //    if (Title == Description)
        //    {
        //        // yield return makes sure that the validationRes.. is immediately returnd after which code execution will continue
        //        yield return new ValidationResult( // obj to provide error mesages and related properties 
        //        "The provided description should be different from the title.",
        //        new[] { "Course" });
        //    }
        //}
        //-----------------------------------------------
    }
}
