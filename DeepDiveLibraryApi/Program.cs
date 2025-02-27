using DeepDiveLibraryApi.DbContexts;
using DeepDiveLibraryApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.s
builder.Services.AddControllers(configure =>
{
    configure.ReturnHttpNotAcceptable = true;
    configure.CacheProfiles.Add("240SecondsCacheProfile",
              new() { Duration = 240 });
}).AddNewtonsoftJson(setupAction =>
{
    setupAction.SerializerSettings.ContractResolver =
        new CamelCasePropertyNamesContractResolver();
})
.AddXmlSerializerFormatters()
.ConfigureApiBehaviorOptions(setupAction =>
{
    setupAction.InvalidModelStateResponseFactory = context =>
    {
        // create a validation problem details object
        var problemDetailsFactory = context.HttpContext.RequestServices
            .GetRequiredService<ProblemDetailsFactory>();

        var validationProblemDetails = problemDetailsFactory
            .CreateValidationProblemDetails(
                context.HttpContext,
                context.ModelState);

        // add additional info not added by default
        validationProblemDetails.Detail =
            "See the errors field for details.";
        validationProblemDetails.Instance =
            context.HttpContext.Request.Path;

        // report invalid model state responses as validation issues
        validationProblemDetails.Type =
            "https://courselibrary.com/modelvalidationproblem";
        validationProblemDetails.Status =
            StatusCodes.Status422UnprocessableEntity;
        validationProblemDetails.Title =
            "One or more validation errors occurred.";

        return new UnprocessableEntityObjectResult(validationProblemDetails)
        {
            ContentTypes = { "application/problem+json" }
        };

    };


});
builder.Services.AddResponseCaching();

builder.Services.AddHttpCacheHeaders(
    (expirationModelOptions) =>
    {
        expirationModelOptions.MaxAge = 60;
        expirationModelOptions.CacheLocation =
            Marvin.Cache.Headers.CacheLocation.Private;
    },
    (validationModelOptions) =>
    {
        validationModelOptions.MustRevalidate = true;
    }
 );

builder.Services.Configure<MvcOptions>(config =>
{
    var newtonsoftJsonOutputFormatter = config.OutputFormatters
          .OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();

    if (newtonsoftJsonOutputFormatter != null)
    {
        newtonsoftJsonOutputFormatter.SupportedMediaTypes
            .Add("application/vnd.marvin.hateoas+json");
    }
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



//--------------------------------------------------------------
builder.Services.AddTransient<IPropertyCheckerService,
            PropertyCheckerService>();
builder.Services.AddTransient<IPropertyMappingService, PropertyMappingService>();
builder.Services.AddScoped<ICourseLibraryRepository,
            CourseLibraryRepository>();
//--------------------------------------------------------------
builder.Services.AddDbContext<CourseLibraryContext>(options =>
{
    options.UseSqlite(@"Data Source=library.db");
});
//--------------------------------------------------------------
builder.Services.AddAutoMapper(
    AppDomain.CurrentDomain.GetAssemblies());
//--------------------------------------------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseHttpCacheHeaders();
app.UseResponseCaching();
app.UseAuthorization();

app.MapControllers();

app.Run();
