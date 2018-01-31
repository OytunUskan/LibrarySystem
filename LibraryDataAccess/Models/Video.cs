using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;


namespace LibraryDataAccess.Models
{
    public class Video : LibraryAsset
    {

        [Required]
        public string Directory { get; set; }
    }
}
