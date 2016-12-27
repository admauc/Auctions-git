using Auctions.Domain.Abstract;
using Auctions.Domain.Entities;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Auctions.Domain.Concrete;
using System.Net;
using System.Net.Mail;

namespace Auctions.WebUI.Controllers
{
    public class UserController : Controller
    {
        EFDbContext db = new EFDbContext();
        private ILotRepository repository;
        public UserController(ILotRepository repo)
        {
            repository = repo;
        }

        [Authorize]
        public ViewResult Index()
        {
            IQueryable<Lot> lots = db.Lots.Where(p => p.UserEmail.Contains(User.Identity.Name));
            return View(lots);
        }

        [Authorize]
        [HttpGet]
        public ViewResult Edit(int LotId)
        {
        Lot lot = repository.Lots
        .FirstOrDefault(p => p.LotID == LotId);
        return View(lot);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Edit(Lot lot, HttpPostedFileBase image, int days)
        {
            if (ModelState.IsValid)
            {
                if (image == null)
                {
                    return View("ErrorImage");
                }
                if (image != null)
                {
                    lot.StartDate = DateTime.Now;
                    lot.EndDate = (lot.StartDate + new TimeSpan(days, 0, 0, 0));
                    lot.UserEmail = User.Identity.Name;
                    lot.UserEmailBid = User.Identity.Name;
                    lot.Preview = image.ContentType;
                    lot.ImageData = new byte[image.ContentLength];
                    image.InputStream.Read(lot.ImageData, 0, image.ContentLength);
                }
                repository.SaveLot(lot);
                return RedirectToAction("Index");
            }
            else
            {
                // there is something wrong with the data values
                return View(lot);
            }

        }

        [Authorize]
        public ViewResult Create()
        {

            return View("Edit", new Lot());
        }

        [Authorize]
        [HttpPost]
        public ActionResult Delete(int LotID)
        {
            Lot deletedLot = repository.DeleteLot(LotID);
            if (deletedLot != null)
            {
                TempData["message"] = string.Format("{0} Удалён", deletedLot.Name);
            }
            return RedirectToAction("Index");


        }

        [Authorize]
        [HttpPost]
        public ActionResult ShiftBid(int LotID, decimal Bid)
        {
            IQueryable<Lot> lots = db.Lots.Where(p => p.UserEmailBid.Contains(User.Identity.Name));
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
                return RedirectToAction("CreateBid");
            }
            return View(lots);
        }


        [Authorize]
        [HttpGet]
        public ActionResult CreateBid()
        {
            IQueryable<Lot> lots = db.Lots.Where(p => p.UserEmailBid.Contains(User.Identity.Name));
            return View(lots);
        }

        public ActionResult LotBuy(int LotID)
        {
            Lot lot = repository.Lots
            .FirstOrDefault(p => p.LotID == LotID);
            return View(lot);
        }


    }
}