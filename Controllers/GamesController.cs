using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GameVault.Data;
using GameVault.Models;
using System.Security.Claims;

namespace GameVault.Controllers
{
    [Authorize]
    public class GamesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GamesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: Games
        public async Task<IActionResult> Index(string? search, int? platformId, GameStatus? status)
        {
            var query = _context.Games
                .Include(g => g.Platform)
                .Where(g => g.OwnerUserId == CurrentUserId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(g => g.Title.Contains(search));

            if (platformId.HasValue)
                query = query.Where(g => g.PlatformId == platformId.Value);

            if (status.HasValue)
                query = query.Where(g => g.Status == status.Value);

            ViewData["PlatformId"] = new SelectList(
                _context.Platforms.OrderBy(p => p.Name), "PlatformId", "Name", platformId);

            ViewData["Status"] = new SelectList(
                Enum.GetValues(typeof(GameStatus)).Cast<GameStatus>(), status);

            ViewData["Search"] = search;

            var games = await query.OrderBy(g => g.Title).ToListAsync();
            return View(games);
        }

        // GET: Games/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var game = await _context.Games
                .Include(g => g.Platform)
                .Include(g => g.GameGenres)
                    .ThenInclude(gg => gg.Genre)
                .FirstOrDefaultAsync(m => m.GameId == id && m.OwnerUserId == CurrentUserId);

            if (game == null) return NotFound();

            return View(game);
        }

        // GET: Games/Create
        public IActionResult Create()
        {
            ViewData["PlatformId"] = new SelectList(_context.Platforms, "PlatformId", "Name");
            ViewData["Genres"] = _context.Genres.OrderBy(g => g.Name).ToList();
            return View();
        }

        // POST: Games/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,Description,ReleaseDate,Rating,Status,CoverUrl,PlatformId")] Game game,
            int[]? selectedGenres)
        {
            if (ModelState.IsValid)
            {
                game.CreatedAt = DateTime.UtcNow;
                game.UpdatedAt = null;
                game.OwnerUserId = CurrentUserId;

                _context.Add(game);
                await _context.SaveChangesAsync();

                if (selectedGenres != null)
                {
                    foreach (var genreId in selectedGenres)
                    {
                        _context.GameGenres.Add(new GameGenre
                        {
                            GameId = game.GameId,
                            GenreId = genreId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["PlatformId"] = new SelectList(_context.Platforms, "PlatformId", "Name", game.PlatformId);
            ViewData["Genres"] = _context.Genres.OrderBy(g => g.Name).ToList();
            return View(game);
        }

        // GET: Games/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var game = await _context.Games
                .Include(g => g.GameGenres)
                .FirstOrDefaultAsync(g => g.GameId == id && g.OwnerUserId == CurrentUserId);

            if (game == null) return NotFound();

            ViewData["PlatformId"] = new SelectList(_context.Platforms, "PlatformId", "Name", game.PlatformId);
            ViewData["Genres"] = _context.Genres.OrderBy(g => g.Name).ToList();
            ViewData["SelectedGenres"] = game.GameGenres.Select(gg => gg.GenreId).ToList();
            return View(game);
        }

        // POST: Games/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("GameId,Title,Description,ReleaseDate,Rating,Status,CoverUrl,PlatformId")] Game game,
            int[]? selectedGenres)
        {
            if (id != game.GameId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Games.AsNoTracking()
                        .FirstOrDefaultAsync(g => g.GameId == id && g.OwnerUserId == CurrentUserId);

                    if (existing == null) return NotFound();

                    game.CreatedAt = existing.CreatedAt;
                    game.OwnerUserId = existing.OwnerUserId;
                    game.UpdatedAt = DateTime.UtcNow;

                    _context.Update(game);

                    // Substituir géneros
                    var oldGenres = _context.GameGenres.Where(gg => gg.GameId == id);
                    _context.GameGenres.RemoveRange(oldGenres);

                    if (selectedGenres != null)
                    {
                        foreach (var genreId in selectedGenres)
                        {
                            _context.GameGenres.Add(new GameGenre
                            {
                                GameId = id,
                                GenreId = genreId
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameExists(game.GameId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["PlatformId"] = new SelectList(_context.Platforms, "PlatformId", "Name", game.PlatformId);
            ViewData["Genres"] = _context.Genres.OrderBy(g => g.Name).ToList();
            ViewData["SelectedGenres"] = selectedGenres?.ToList() ?? new List<int>();
            return View(game);
        }

        // GET: Games/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var game = await _context.Games
                .Include(g => g.Platform)
                .FirstOrDefaultAsync(m => m.GameId == id && m.OwnerUserId == CurrentUserId);

            if (game == null) return NotFound();

            return View(game);
        }

        // POST: Games/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _context.Games
                .FirstOrDefaultAsync(g => g.GameId == id && g.OwnerUserId == CurrentUserId);

            if (game != null)
                _context.Games.Remove(game);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GameExists(int id)
        {
            return _context.Games.Any(e => e.GameId == id);
        }
    }
}