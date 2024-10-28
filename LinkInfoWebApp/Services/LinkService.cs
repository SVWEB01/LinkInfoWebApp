using LinkInfoWebApp.Models;
using MySql.Data.MySqlClient;
using HtmlAgilityPack;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace LinkInfoWebApp.Services
{
    public class LinkService
    {
        private readonly string _connectionString;

        public LinkService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySQLConnection");
        }

        public string SaveUrl(string url)
        {
            string domain = GetDomain(url);

            // Verificar si el enlace ya existe
            if (LinkExists(url))
            {
                return $"El enlace ya existe en el dominio: {domain}";
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "INSERT INTO Links (url, domain) VALUES (@url, @domain)";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@url", url);
                    cmd.Parameters.AddWithValue("@domain", domain);
                    cmd.ExecuteNonQuery();
                }
            }

            return "Enlace guardado exitosamente.";
        }

        private bool LinkExists(string url)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT COUNT(*) FROM Links WHERE url = @url";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@url", url);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private string GetDomain(string url)
        {
            Uri uri = new Uri(url);
            string host = uri.Host; // Obtiene el host completo (por ejemplo, "m.animeflv.net")

            // Eliminar el prefijo "www." si existe
            if (host.StartsWith("www."))
            {
                host = host.Substring(4); // Elimina "www."
            }

            // Devuelve el dominio sin la parte de la extensión (por ejemplo, "animeflv" de "animeflv.net" o "m.animeflv")
            string[] parts = host.Split('.');
            if (parts.Length >= 2)
            {
                return string.Join(".", parts.Take(parts.Length - 1)); // Une todas las partes menos la última (extensión)
            }

            return host; // Si no tiene partes, devuelve el host
        }

        public List<Link> GetAllLinks()
        {
            var links = new List<Link>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT id, url, domain FROM Links"; // Asegúrate de seleccionar el domain aquí
                using (var cmd = new MySqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string url = reader.GetString("url");
                        string domain = reader.GetString("domain"); // Obtén el dominio de la base de datos
                        var (title, thumbnail) = GetUrlInfo(url);

                        links.Add(new Link
                        {
                            Id = reader.GetInt32("id"),
                            Url = url,
                            Title = title,
                            Thumbnail = thumbnail,
                            Domain = domain // Asegúrate de asignar el dominio
                        });
                    }
                }
            }

            return links;
        }

        private (string title, string thumbnail) GetUrlInfo(string url)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);

            var titleNode = doc.DocumentNode.SelectSingleNode("//title");
            string title = titleNode != null ? titleNode.InnerText : "Sin título";

            var thumbnailNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
            string thumbnail = thumbnailNode != null ? thumbnailNode.GetAttributeValue("content", "") : null;

            return (title, thumbnail);
        }

        public void DeleteLink(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var query = "DELETE FROM Links WHERE id = @id";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
