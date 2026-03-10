using System.Diagnostics;
using System.Security.Claims;
using GameVault.Data;
using GameVault.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameVault.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                var games = await _context.Games
                    .Where(g => g.OwnerUserId == userId)
                    .ToListAsync();

                ViewData["Total"] = games.Count;
                ViewData["Playing"] = games.Count(g => g.Status == GameStatus.Playing);
                ViewData["Completed"] = games.Count(g => g.Status == GameStatus.Completed);
                ViewData["Backlog"] = games.Count(g => g.Status == GameStatus.Backlog);
                ViewData["Dropped"] = games.Count(g => g.Status == GameStatus.Dropped);

                // Últimos 4 jogos adicionados
                var recent = await _context.Games
                    .Include(g => g.Platform)
                    .Where(g => g.OwnerUserId == userId)
                    .OrderByDescending(g => g.CreatedAt)
                    .Take(4)
                    .ToListAsync();

                ViewData["RecentGames"] = recent;
            }

            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}