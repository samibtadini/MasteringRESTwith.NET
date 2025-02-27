﻿using AutoMapper;
using DeepDiveLibraryApi.Models;
using DeepDiveLibraryApi.Services;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace DeepDiveLibraryApi.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/courses")]
    //[ResponseCache(CacheProfileName = "240SecondsCacheProfile")]
    [HttpCacheExpiration(CacheLocation = CacheLocation.Public)]
    [HttpCacheValidation(MustRevalidate = true)]
    public class CoursesController : Controller
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public CoursesController(ICourseLibraryRepository courseLibraryRepository,
            IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        //[ResponseCache(Duration = 120)]
        [HttpGet(Name = "GetCoursesForAuthor")]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 1000)]
        [HttpCacheValidation(MustRevalidate = false)]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCoursesForAuthor(Guid authorId)
        {
            if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var coursesForAuthorFromRepo = await _courseLibraryRepository.GetCoursesAsync(authorId);
            return Ok(_mapper.Map<IEnumerable<CourseDto>>(coursesForAuthorFromRepo));
        }

        [HttpGet("{courseId}", Name = "GetCourseForAuthor")]
        public async Task<ActionResult<CourseDto>> GetCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var courseForAuthorFromRepo = await _courseLibraryRepository.GetCourseAsync(authorId, courseId);

            if (courseForAuthorFromRepo == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<CourseDto>(courseForAuthorFromRepo));
        }


        [HttpPost(Name = "CreateCourseForAuthor")]
        public async Task<ActionResult<CourseDto>> CreateCourseForAuthor(
                Guid authorId, CourseForCreationDto course)
        {
            if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var courseEntity = _mapper.Map<Entities.Course>(course);
            _courseLibraryRepository.AddCourse(authorId, courseEntity);
            await _courseLibraryRepository.SaveAsync();

            var courseToReturn = _mapper.Map<CourseDto>(courseEntity);
            //return Ok(courseToReturn);
            return CreatedAtRoute("GetCourseForAuthor",
           new { authorId, courseId = courseToReturn.Id },
           courseToReturn);
        }

        [HttpPut("{courseId}")]
        public async Task<IActionResult> UpdateCourseForAuthor(Guid authorId,
      Guid courseId,
      CourseForUpdateDto course)
        {
            if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var courseForAuthorFromRepo = await _courseLibraryRepository
                .GetCourseAsync(authorId, courseId);
            // if theirs no course for an auther you should to add a new course otherwise update
            if (courseForAuthorFromRepo == null)
            {
                var courseToAdd = _mapper.Map<Entities.Course>(course);
                courseToAdd.Id = courseId;
                _courseLibraryRepository.AddCourse(authorId, courseToAdd);
                await _courseLibraryRepository.SaveAsync();

                var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);
                return CreatedAtRoute("GetCourseForAuthor",
                    new { authorId, courseId = courseToReturn.Id },
                    courseToReturn);
            }

            _mapper.Map(course, courseForAuthorFromRepo);

            _courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);

            await _courseLibraryRepository.SaveAsync();
            return NoContent();
        }

        // validation note: other piece of validation happen is on the jsonPatchDocument itself that is inputted 
        // keep in mind this jsonPatchDocument can be syntactically correct but sementaically wrong 
        [HttpPatch("{courseId}")]
        public async Task<IActionResult> PartiallyUpdateCourseForAuthor(
        Guid authorId,
        Guid courseId,
        JsonPatchDocument<CourseForUpdateDto> patchDocument)
        {
            if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var courseForAuthorFromRepo = await _courseLibraryRepository
                .GetCourseAsync(authorId, courseId);

            if (courseForAuthorFromRepo == null)
            {
                var courseDto = new CourseForUpdateDto();
                patchDocument.ApplyTo(courseDto);
                // check validation
                if (!TryValidateModel(courseDto))
                {
                    return ValidationProblem(ModelState);
                }
                var courseToAdd = _mapper.Map<Entities.Course>(courseDto);
                courseToAdd.Id = courseId;

                _courseLibraryRepository.AddCourse(authorId, courseToAdd);
                await _courseLibraryRepository.SaveAsync();

                var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);
                return CreatedAtRoute("GetCourseForAuthor",
                    new { authorId, courseId = courseToReturn.Id },
                    courseToReturn);
            }

            var courseToPatch = _mapper.Map<CourseForUpdateDto>(
                courseForAuthorFromRepo);
            patchDocument.ApplyTo(courseToPatch, ModelState); // instead of returning 500 server error it return 400 bad rea
            // check validation
            if (!TryValidateModel(courseToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(courseToPatch, courseForAuthorFromRepo);

            _courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);

            await _courseLibraryRepository.SaveAsync();

            return NoContent();
        }

        [HttpDelete("{courseId}")]
        public async Task<ActionResult> DeleteCourseForAuthor(Guid authorId, Guid courseId)
        {
            if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var courseForAuthorFromRepo = await _courseLibraryRepository.GetCourseAsync(authorId, courseId);

            if (courseForAuthorFromRepo == null)
            {
                return NotFound();
            }

            _courseLibraryRepository.DeleteCourse(courseForAuthorFromRepo);
            await _courseLibraryRepository.SaveAsync();

            return NoContent();
        }

        public override ActionResult ValidationProblem(
      [ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices
                .GetRequiredService<IOptions<ApiBehaviorOptions>>();

            return (ActionResult)options.Value
                .InvalidModelStateResponseFactory(ControllerContext);
        }
    }
}
