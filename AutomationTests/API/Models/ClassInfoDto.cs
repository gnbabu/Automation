namespace API.Models
{
    public class ClassInfoDto
    {
        public string ClassName { get; set; }
        public List<MethodInfoDto> Methods { get; set; } = new();
    }
}
