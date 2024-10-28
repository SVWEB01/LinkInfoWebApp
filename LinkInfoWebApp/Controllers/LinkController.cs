using LinkInfoWebApp.Models;
using LinkInfoWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinkInfoWebApp.Controllers
{
    public class LinkController : Controller
    {
        private readonly LinkService _linkService;

        public LinkController(LinkService linkService)
        {
            _linkService = linkService;
        }

        public IActionResult Index(string domainFilter = null)
        {
            var links = _linkService.GetAllLinks();

            // Filtrar por dominio si se proporciona
            if (!string.IsNullOrEmpty(domainFilter))
            {
                links = links.Where(link =>
                    link != null &&
                    link.Domain != null &&
                    link.Domain.Equals(domainFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return View(links);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            _linkService.DeleteLink(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AddUrl(string url)
        {
            string resultMessage = _linkService.SaveUrl(url);
            ViewBag.ResultMessage = resultMessage; // Pasar el mensaje a la vista
            return RedirectToAction("Index");
        }
    }
}
