﻿using LibraryDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryDataAccess
{
    public interface ILibraryAsset
    {
        IEnumerable<LibraryAsset> GetAll();
        LibraryAsset GetById(int id);

        void Add(LibraryAsset newLibraryAsset);
        string GetAuthorOrDirector(int id);
        string GetDeweyIndex(int id);
        string GetType(int id);
        string GetTitle(int id);
        string GetIsbn(int id);

        LibraryBranch GetCurrentLocation(int id);
    }
}
