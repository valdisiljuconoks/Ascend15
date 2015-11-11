using System.Collections.Generic;
using Ascend15.Models.Catalog;
using Ascend15.Models.Domain;

namespace Ascend15.Models.ViewModels
{
    public class CatalogVariationViewModel
    {
        public AwesomeVariation Content { get; set; }
        public string Price { get; set; }
        public string Currency { get; set; }
        public string ImageUrl { get; set; }
        public IEnumerable<NameAndLinkPair> Colors { get; set; }
    }
}