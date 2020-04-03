using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Mvc.Models.CouponViewModels;

namespace Payments.Mvc.Controllers
{
    public class CouponsController : SuperController
    {
        private readonly ApplicationDbContext _dbContext;

        public CouponsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var coupons = await _dbContext.Coupons
                .Where(i => i.Team.Slug == TeamSlug)
                .ToListAsync();

            return View(coupons);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCouponViewModel model)
        {
            var team = await _dbContext.Teams
                .Include(t => t.Coupons)
                .FirstOrDefaultAsync(t => t.Slug == TeamSlug);

            var coupon = new Coupon
            {
                Name            = model.Name,
                Code            = model.Code,
                DiscountAmount  = model.DiscountAmount,
                DiscountPercent = model.DiscountPercent / 100,
                ExpiresAt       = model.ExpiresAt,
                Team            = team
            };

            if (team.Coupons.Any(a => a.Code != null && a.Code.Equals(coupon.Code, StringComparison.OrdinalIgnoreCase)))
            {
                ErrorMessage = $"Coupon Code {coupon.Code} is already in use. Please select a new one.";
                return RedirectToAction("Index");
            }

            team.Coupons.Add(coupon);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var coupon = await _dbContext.Coupons
                .Include(c => c.Invoices)
                .Where(c => c.Team.Slug == TeamSlug)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }
    }
}
