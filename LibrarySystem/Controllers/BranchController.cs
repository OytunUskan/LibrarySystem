using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryDataAccess;
using LibrarySystem.Models.Branch;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Controllers
{
    public class BranchController : Controller
    {
        private ILibraryBranch _branch;
        public BranchController(ILibraryBranch branch)
        {
            _branch = branch;
        }

        public IActionResult Index()
        {
            var branchModels = _branch.GetAll()
                .Select(br => new BranchDetailModel
                {
                    Id = br.Id,
                    BranchName = br.Name,
                    NumberOfAssets = _branch.GetAssetCount(br.LibraryAssets),
                    NumberOfPatrons = _branch.GetPatronCount(br.Patrons),
                    IsOpen = _branch.IsBranchOpen(br.Id)
                }).ToList();

            var model = new BranchIndexModel()
            {
                Branches = branchModels
            };

            return View(model);
        }

        public IActionResult Detail(int id)
        {
            var branch = _branch.Get(id);
            var model = new BranchDetailModel
            {
                BranchName = branch.Name,
                Description = branch.Description,
                Address = branch.Address,
                Telephone = branch.Telephone,
                BranchOpenedDate = branch.OpenDate.ToString("dd-MM-yyy"),
                NumberOfPatrons = _branch.GetPatronCount(branch.Patrons),
                NumberOfAssets = _branch.GetAssetCount(branch.LibraryAssets),
                TotalAssetValue = _branch.GetAssetsValue(id),
                ImageUrl = branch.ImageUrl,
                HoursOpen = _branch.GetBranchHours(id)
            };

            return View(model);
        }
    }
}
