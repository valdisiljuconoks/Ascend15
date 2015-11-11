using System.Collections.Generic;
using Ascend15.Models.Domain;

namespace Ascend15.Models.ViewModels
{
    public class CatalogViewModel
    {
        public IEnumerable<NameAndLinkPair> Nodes { get; set; }

        public IEnumerable<NameAndLinkPair> Products { get; set; }
    }
}
