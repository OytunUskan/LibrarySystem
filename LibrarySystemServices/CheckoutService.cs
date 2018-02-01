using LibraryDataAccess;
using LibraryDataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibrarySystemServices
{
    public class CheckoutService : ICheckout
    {
        private const string NotCheckedOut = "Not checked out.";
        private LibraryContext _context;
        public CheckoutService(LibraryContext context)
        {
            _context = context;
        }


        public void Add(Checkout newCheckout)
        {
            _context.Add(newCheckout);
            _context.SaveChanges();
        }

        public IEnumerable<Checkout> GetAll()
        {
            return _context.Checkouts;
        }

        public Checkout GetById(int checkoutId)
        {
            return GetAll().FirstOrDefault(cout => cout.Id == checkoutId);
        }

        public IEnumerable<CheckoutHistory> GetCheckOutHistory(int id)
        {
            return _context.CheckoutHistories
                .Include(cout => cout.LibraryAsset)
                .Include(cout => cout.LibraryCard)
                .Where(cout => cout.LibraryAsset.Id == id);
        }

        public IEnumerable<Hold> GetCurrentHolds(int id)
        {
            return _context.Holds
                .Include(cout => cout.LibraryAsset)
                .Where(cout => cout.LibraryAsset.Id == id);
        }

        public Checkout GetLatesCheckout(int assetId)
        {
            return _context.Checkouts
                .Where(cout => cout.LibraryAsset.Id == assetId)
                .OrderBy(cout => cout.Since)
                .FirstOrDefault();
        }

        public void MarkFound(int assetId)
        {
            var now = DateTime.Now;
            

            UpdateAssetStatus(assetId, AssetStatus.Available);

            //remove any existing checkout on the item
            RemoveExistingCheckouts(assetId);

            //close any existing checkout history
            CloseExistingCheckoutHistory(assetId, now);

            _context.SaveChanges();

        }

        private void UpdateAssetStatus(int assetId, string assetStatus)
        {
            var item = _context.LibraryAssets
                .FirstOrDefault(asset => asset.Id == assetId);

            _context.Update(item);

            item.Status = _context.Statuses
                .FirstOrDefault(status => status.Name == assetStatus);
        }

        private void CloseExistingCheckoutHistory(int assetId, DateTime now)
        {
            var history = _context.CheckoutHistories
                .FirstOrDefault(chis => chis.LibraryAsset.Id == assetId
                                        && chis.CheckIn == null);

            if (history != null)
            {
                _context.Update(history);
                history.CheckIn = now;
            }
        }

        private void RemoveExistingCheckouts(int assetId)
        {
            var checkout = _context.Checkouts
                .FirstOrDefault(cout => cout.LibraryAsset.Id == assetId);

            if (checkout != null)
            {
                _context.Remove(checkout);
            }
        }

        public void MarkLost(int assetId)
        {
            UpdateAssetStatus(assetId, AssetStatus.Lost);
            _context.SaveChanges();
        }

        public void CheckInItem(int assetId)
        {
            var now = DateTime.Now;
            var asset = _context.LibraryAssets
                .FirstOrDefault(a => a.Id == assetId);

            //remove any existing checkout on the item
            RemoveExistingCheckouts(assetId);
            //close any existing chekout history
            CloseExistingCheckoutHistory(assetId, now);
            //look for existing holds on the item
            var currentHolds = _context.Holds
                               .Include(h => h.LibraryAsset)
                               .Include(h => h.LibraryCard)
                               .Where(h => h.LibraryAsset.Id == assetId);
            //if there are holds, checkout item to the librarycard with the earlist hold.
            if (currentHolds.Any())
            {
                CheckoutToEarliestHold(assetId, currentHolds);
                return;
            }
            //otherwise, update the item status to avaliable.
            UpdateAssetStatus(assetId, AssetStatus.Available);

            _context.SaveChanges();

        }

        private void CheckoutToEarliestHold(int assetId, IQueryable<Hold> currentHolds)
        {
            var earliestHold = currentHolds
                .OrderBy(hold => hold.HoldPlaced)
                .FirstOrDefault();

            var card = earliestHold.LibraryCard;
            _context.Remove(earliestHold);
            _context.SaveChanges();

            CheckOutItem(assetId, card.Id);
        }

        public void CheckOutItem(int assetId, int libraryCardId)
        {
            if (IsCheckedOut(assetId))
            {
                return;
                //Add logic here to feedback to the user
            }

            var item = _context.LibraryAssets
                .FirstOrDefault(asset => asset.Id == assetId);

            UpdateAssetStatus(assetId, AssetStatus.CheckedOut);

            var libraryCard = _context.LibraryCards
                .Include(card => card.Checkouts)
                .FirstOrDefault(card => card.Id == libraryCardId);
            var now = DateTime.Now;
            var checkout = new Checkout
            {
                LibraryAsset = item,
                LibraryCard = libraryCard,
                Since = now,
                Until = GetDefaultCheckoutTime(now)
            };

            _context.Add(checkout);


            var checkoutHistory = new CheckoutHistory
            {
                LibraryAsset = item,
                LibraryCard = libraryCard,
                CheckOut = now
            };

            _context.Add(checkoutHistory);
             
            _context.SaveChanges();
        }

        private DateTime GetDefaultCheckoutTime(DateTime now)
        {
            return DateTime.Now.AddDays(30);
        }

        public bool IsCheckedOut(int assetId)
        {
            return _context.Checkouts.Where(co => co.LibraryAsset.Id == assetId).Any();
        }

        public void PlaceHold(int assetId, int libraryCardId)
        {
            var now = DateTime.Now;

            var asset = _context.LibraryAssets
                .Include(a => a.Status)
               .FirstOrDefault(a => a.Id == assetId);

            var card = _context.LibraryCards
                .FirstOrDefault(c => c.Id == libraryCardId);

            if(asset.Status.Name == AssetStatus.Available)
            {
                UpdateAssetStatus(assetId, AssetStatus.OnHold);
            }

            var hold = new Hold
            {
                LibraryAsset = asset,
                LibraryCard = card,
                HoldPlaced = now
            };

            _context.Add(hold);
            _context.SaveChanges();
        }


        public string GetCurrentHoldPatronName(int holdId)
        {
            var hold = _context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .FirstOrDefault(h => h.Id == holdId);

            var cardId = hold?.LibraryCard.Id;
            var patron = _context.Patrons
                .Include(p => p.LibraryCard)
                .FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron?.FirstName + " " + patron?.LastName;


        }

        public DateTime GetCurrentHoldPlaced(int holdId)
        {
            return _context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .FirstOrDefault(h => h.Id == holdId).HoldPlaced;

        }

        public string GetCurrentCheckoutPatron(int assetId)
        {

            var checkout = GetCheckoutByAssetId(assetId);
            if (checkout == null)
            {
                return NotCheckedOut;
            }

            var cardId = checkout.LibraryCard.Id;

            var patron = _context.Patrons
                .Include(p => p.LibraryCard)
                .FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron?.FirstName + " " + patron?.LastName;
        }

        private Checkout GetCheckoutByAssetId(int assetId)
        {
            return _context.Checkouts
                 .Include(c => c.LibraryAsset)
                 .Include(c => c.LibraryCard)
                 .FirstOrDefault(c => c.LibraryAsset.Id == assetId);
        }

        
    }

    public static class AssetStatus
    {
        public const string CheckedOut = "Checked Out";
        public const string Available = "Available";
        public const string Lost = "Lost";
        public const string OnHold = "On Hold";

    }
}
