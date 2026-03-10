using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GameVault.Data;
using GameVault.Models;

namespace GameVault.Controllers
{
    public class GamesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GamesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Games
        public async Task<IActionResult> Index(string? search, int? platformId, GameStatus? status)
        {
            // Query base (inclui Platform para poderes mostrar o nome na tabela)
            var query = _context.Games
                .Include(g => g.Platform)
                .AsQueryable();

            // Pesquisa por título
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(g => g.Title.Contains(search));
            }

            // Filtro por plataforma
            if (platformId.HasValue)
            {
                query = query.Where(g => g.PlatformId == platformId.Value);
            }

            // Filtro por status
            if (status.HasValue)
            {
                query = query.Where(g => g.Status == status.Value);
            }

            // Prepara dropdowns para a View
            ViewData["PlatformId"] = new SelectList(
                _context.Platforms.OrderBy(p => p.Name),
                "PlatformId",
                "Name",
                platformId
            );

            ViewData["Status"] = new SelectList(
                Enum.GetValues(typeof(GameStatus)).Cast<GameStatus>(),
                status
            );

            // Para manter o texto da pesquisa na caixa
            ViewData["Search"] = search;

            // Ordenação (podes mudar como quiseres)
            var games = await query
                .OrderBy(g => g.Title)
                .ToListAsync();

            return View(games);
        }

        // GET: Games/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .Include(g => g.Platform)
                .FirstOrDefaultAsync(m => m.GameId == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // GET: Games/Create
        public IActionResult Create()
        {
            ViewData["PlatformId"] = new SelectList(_context.Platforms, "PlatformId", "Name");
            return View();
        }

        // POST: Games/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Games/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,ReleaseDate,Rating,Status,CoverUrl,PlatformId")] Game game)
        {
            if (ModelState.IsValid)
            {
                // Datas controladas pelo servidor
                game.CreatedAt = DateTime.UtcNow;
                game.UpdatedAt = null;

                // OwnerUserId só se quiseres login (por agora podes deixar null)
                _context.Add(game);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["PlatformId"] = new SelectList(_context.Platforms, "PlatformId", "Name", game.PlatformId);
            return View(game);
        }

        // GET: Games/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            ViewData["PlatformId"] = new SelectList(_context.Platforms, "PlatformId", "Name", game.PlatformId);
            return View(game);
        }

        // POST: Games/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Games/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GameId,Title,Description,ReleaseDate,Rating,Status,CoverUrl,PlatformId")] Game game)
        {
            if (id != game.GameId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Vai buscar o registo atual para manter CreatedAt e OwnerUserId
                    var existing = await _context.Games.AsNoTracking()
                        .FirstOrDefaultAsync(g => g.GameId == id);

                    if (existing == null) return NotFound();

                    game.CreatedAt = existing.CreatedAt;     // mantém o original
                    game.OwnerUserId = existing.OwnerUserId; // não deixa alterar no formulário
                    game.UpdatedAt = DateTime.UtcNow;        // atualiza

                    _context.Update(game);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameExists(game.GameId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["PlatformId"] = new SelectList(_context.Platforms, "PlatformId", "Name", game.PlatformId);
            return View(game);
        }

        // GET: Games/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .Include(g => g.Platform)
                .FirstOrDefaultAsync(m => m.GameId == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // POST: Games/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game != null)
            {
                _context.Games.Remove(game);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GameExists(int id)
        {
            return _context.Games.Any(e => e.GameId == id);
        }
    }
}
