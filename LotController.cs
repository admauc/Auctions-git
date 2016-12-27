using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Auctions.Domain.Abstract;
using Auctions.Domain.Entities;
using Auctions.WebUI.Models;
using Auctions.Domain.Concrete;
using System.Data.Entity;
using System.Net.Mail;

namespace Auctions.WebUI.Controllers
{
    
    public class LotController : Controller
    {
        EFDbContext db = new EFDbContext();
        private ILotRepository repository;
        public int PageSize = 12;
        public LotController(ILotRepository lotRepository)
        {
            this.repository = lotRepository;
        }


        public ViewResult List(string category, int page = 1)
        {

        LotsListViewModel model = new LotsListViewModel
        {


            Lots = repository.Lots
            .Where(p => category == null || p.Category == category)
            .OrderBy(p => p.LotID)
            .Skip((page - 1) * PageSize)
            .Take(PageSize),
            PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = PageSize,
                TotalItems = category == null ?
                repository.Lots.Count() :
                repository.Lots.Where(e => e.Category == category).Count()
            },
            CurrentCategory = category

        };
        return View(model);
        }

        public FileContentResult GetImage(int LotID)
        {
            Lot lot = repository.Lots.FirstOrDefault(p => p.LotID == LotID);
            if (lot != null)
            {
                return File(lot.ImageData, lot.Preview);
            }
            else
            {
                return null;
            }
        }

        public ActionResult SearchForName(string searchString)
        {
            IQueryable<Lot> lots = db.Lots.Where(p => p.Name.Contains(searchString));


            return View(lots);
        }

        [Authorize]
        [HttpGet]
        public ActionResult LinkLot(int LotID)
        {
            IQueryable<Lot> lots = db.Lots.Where(p => p.LotID.Equals(LotID));
            return View(lots);
        }

        [Authorize]
        [HttpGet]
        public ActionResult MyLot(int LotID)
        {
            IQueryable<Lot> lot = db.Lots.Where(p => p.LotID.Equals(LotID));
            return View(lot);
        }

        [Authorize]
        public ActionResult ShiftLot(int LotID, decimal Bid)
        {
            IQueryable<Lot> lots = db.Lots.Where(p => p.LotID.Equals(LotID));
            if (Bid > 0)
            {
                Lot lot = repository.Lots.FirstOrDefault(p => p.LotID == LotID);
                if (lot != null)
                {
                    decimal a = lot.CurrentPrice;
                    lot.CurrentPrice = (a + Bid);
                    lot.UserEmailBid = User.Identity.Name;
                    repository.SaveLot(lot);
                    db.SaveChanges();
                }
                return View("LinkLot", lots);
            }
            return View(lots);
        }


        [Authorize]
        public ActionResult LotBuy(int LotID)
        {
            Lot lot = repository.Lots
            .FirstOrDefault(p => p.LotID == LotID);
            return View(lot);
        }

    }
}