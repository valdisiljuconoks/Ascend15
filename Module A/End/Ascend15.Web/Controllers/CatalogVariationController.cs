using System;
using System.Linq;
using System.Web.Mvc;
using Ascend15.Extensions;
using Ascend15.Models.Catalog;
using Ascend15.Models.Domain;
using Ascend15.Models.ViewModels;
using EPiServer;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Filters;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Pricing;

namespace Ascend15.Controllers
{
    public class CatalogVariationController : CatalogContentBaseController<AwesomeVariation>
    {
        private readonly ICurrentMarket _currentMarket;
        private readonly IPriceService _priceService;
        private readonly FilterPublished _filterPublished;

        public CatalogVariationController(IContentLoader contentLoader,
                                          AssetUrlResolver assetUrlResolver,
                                          ThumbnailUrlResolver thumbnailUrlResolver,
                                          ICurrentMarket currentMarket,
                                          IPriceService priceService,
                                          FilterPublished filterPublished) : base(contentLoader, assetUrlResolver, thumbnailUrlResolver)
        {
            _currentMarket = currentMarket;
            _priceService = priceService;
            _filterPublished = filterPublished;
        }

        public ActionResult Index(AwesomeVariation currentContent)
        {
            var product = currentContent.GetProduct();
            var otherVariations = product.GetVariations()
                .Cast<AwesomeVariation>()
                .Where(v => v.IsAvailableInCurrentMarket() && !_filterPublished.ShouldFilter(v));

            var market = _currentMarket.GetCurrentMarket();
            var defaultPrice = GetDefaultPrice(currentContent, market);

            var model = new CatalogVariationViewModel
            {
                Content = currentContent,
                Price = (defaultPrice != null ? defaultPrice.UnitPrice.Amount.ToString("F") : "N/A"),
                Currency = (defaultPrice != null ? defaultPrice.UnitPrice.Currency.ToString() : "N/A"),
                ImageUrl = GetDefaultAsset(currentContent),
                Colors = otherVariations.Select(v => new NameAndLinkPair
                {
                    Name = v.Color,
                    Url = v.ContentLink.GetUrl(),
                    ImageThumbnailUrl = v.GetVariationIconUrl("variation-thumbnail")
                })
            };

            return View(model);
        }

        private IPriceValue GetDefaultPrice(VariationContent variation, IMarket market)
        {
            return _priceService.GetDefaultPrice(market.MarketId,
                                                 DateTime.Now,
                                                 new CatalogKey(AppContext.Current.ApplicationId, variation.Code),
                                                 market.DefaultCurrency);
        }
    }
}
