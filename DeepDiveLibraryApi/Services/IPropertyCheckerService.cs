namespace DeepDiveLibraryApi.Services
{
    public interface IPropertyCheckerService
    {
        bool TypeHasProperties<T>(string? fields);
    }
}
