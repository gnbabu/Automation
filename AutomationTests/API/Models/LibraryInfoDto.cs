namespace API.Models
{
    public class LibraryInfoDto
    {
        public string LibraryName { get; set; }
        public List<ClassInfoDto> Classes { get; set; } = new();
    }
}
