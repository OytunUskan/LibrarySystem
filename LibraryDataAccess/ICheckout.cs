using LibraryDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryDataAccess
{
    public interface ICheckout
    {
        void Add(Checkout newCheckout);

        IEnumerable<Checkout> GetAll();
        IEnumerable<CheckoutHistory> GetCheckOutHistory(int id);
        IEnumerable<Hold> GetCurrentHolds(int id);
        Checkout GetById(int checkoutId);
        Checkout GetLatesCheckout(int assetId);

        string GetCurrentCheckoutPatron(int assetId);
        string GetCurrentHoldPatronName(int id);
        DateTime GetCurrentHoldPlaced(int id);

        void PlaceHold(int assetId, int libraryCardId);
        void CheckOutItem(int assetId, int libraryCardId);
        void CheckInItem(int assetId, int libraryCardId);
        void MarkLost(int assetId);
        void MarkFound(int assetId);
    }
}
