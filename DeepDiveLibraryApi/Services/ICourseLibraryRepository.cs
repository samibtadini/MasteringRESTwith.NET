using DeepDiveLibraryApi.Entities;
using DeepDiveLibraryApi.Helpers;
using DeepDiveLibraryApi.ResourceParameters;

namespace DeepDiveLibraryApi.Services
{
    public interface ICourseLibraryRepository
    {
        Task<IEnumerable<Course>> GetCoursesAsync(Guid authorId);
        Task<Course> GetCourseAsync(Guid authorId, Guid courseId);
        void AddCourse(Guid authorId, Course course);
        void UpdateCourse(Course course);
        void DeleteCourse(Course course);
        Task<PagedList<Author>> GetAuthorsAsync(AuthorsResourceParameters authorsResourceParameters);
        Task<Author> GetAuthorAsync(Guid authorId);
        Task<IEnumerable<Author>> GetAuthorsAsync(IEnumerable<Guid> authorIds);
        void AddAuthor(Author author);
        void DeleteAuthor(Author author);
        void UpdateAuthor(Author author);
        Task<bool> AuthorExistsAsync(Guid authorId);
        Task<bool> SaveAsync();
    }
}
