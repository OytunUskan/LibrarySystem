using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryDataAccess;
using LibrarySystem.Models.Catalog;
using LibrarySystem.Models.CheckoutModels;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Controllers
{
    public class CatalogController : Controller
    {
        private ILibraryAsset _asset;
        private ICheckout _checkout;

        public CatalogController(ILibraryAsset asset, ICheckout checkout)
        {
            _asset = asset;
            _checkout = checkout;
        }

        public IActionResult Index()
        {
            var assetModel = _asset.GetAll();

            var listingResult = assetModel.Select(result => new AssetIndexListingModel
            {
                Id = result.Id,
                ImageUrl = result.ImageUrl,
                AuthorOrDirector = _asset.GetAuthorOrDirector(result.Id),
                DeweyCallNumber = _asset.GetDeweyIndex(result.Id),
                NumberOfCopies = result.NumberOfCopies,
                Title = result.Title,
                Type = _asset.GetType(result.Id)
            });

            var model = new AssetIndexModel()
            {
                Assets = listingResult
            };

            return View(model);
        }

        public IActionResult Detail(int id)
        {
            var asset = _asset.GetById(id);
            var currentHolds = _checkout.GetCurrentHolds(id)
                .Select(a => new AssetHoldModel
                {
                    HoldPlaced = _checkout.GetCurrentHoldPlaced(a.Id).ToString("d"),
                    PatronName = _checkout.GetCurrentHoldPatronName(a.Id)
                });
            var model = new AssetDetailModel
            {
                AssetId = id,
                Title = asset.Title,
                Year = asset.Year,
                Cost = asset.Cost,
                Status = asset.Status.Name,
                ImageUrl = asset.ImageUrl,
                AuthorOrDirector = _asset.GetAuthorOrDirector(id),
                CurrentLocation = _asset.GetCurrentLocation(id).Name,
                DeweyCallNumber = _asset.GetDeweyIndex(id),
                ISBN = _asset.GetIsbn(id),
                CheckoutHistory = _checkout.GetCheckOutHistory(id),
                LatestCheckout = _checkout.GetLatesCheckout(id),
                PatronName = _checkout.GetCurrentCheckoutPatron(id),
                CurrentHolds = currentHolds,
                Type = _asset.GetType(id)
            };
            return View(model);
        }


        public IActionResult Checkout(int id)
        {

            var asset = _asset.GetById(id);

            var model = new CheckoutModel {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                LibraryCardId = "",
                Title = asset.Title,
                IsCheckedOut = _checkout.IsCheckedOut(id)
            };


            return View(model);
        }

        public IActionResult CheckIn(int id)
        {
            _checkout.CheckInItem(id);
            return RedirectToAction("Detail", new { id = id });
        }


        public IActionResult Hold(int id)
        {
            var asset = _asset.GetById(id);

            var model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                LibraryCardId = "",
                Title = asset.Title,
                IsCheckedOut = _checkout.IsCheckedOut(id),
                HoldCount = _checkout.GetCurrentHolds(id).Count()
            };


            return View(model);
        }

        public IActionResult MarkLost(int assetId)
        {
            _checkout.MarkLost(assetId);
            return RedirectToAction("Detail", new { id = assetId });
        }

        public IActionResult MarkFound(int assetId)
        {
            _checkout.MarkFound(assetId);
            return RedirectToAction("Detail", new { id = assetId });
        }


        [HttpPost]
        public IActionResult PlacedCheckout(int assetId, int libraryCardId)
        {
            _checkout.CheckOutItem(assetId, libraryCardId);
            return RedirectToAction("Detail", new { id = assetId });
        }

        [HttpPost]
        public IActionResult PlaceHold(int assetId, int libraryCardId)
        {
            _checkout.PlaceHold(assetId, libraryCardId);
            return RedirectToAction("Detail", new { id = assetId });
        }
    }
}