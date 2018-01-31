using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryDataAccess;
using LibrarySystem.Models.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Controllers
{
    public class CatalogController : Controller
    {
        private ILibraryAsset _asset;

        public CatalogController(ILibraryAsset asset)
        {
            _asset = asset;
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

    }
}