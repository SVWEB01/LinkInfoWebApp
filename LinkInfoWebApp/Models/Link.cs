namespace LinkInfoWebApp.Models
{
    public class Link
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Thumbnail { get; set; }
        public string Domain { get; set; } // Nueva propiedad para el dominio
    }
}
