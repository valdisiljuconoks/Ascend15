using System.Web.Optimization;

namespace Ascend15.Infrastructure.Bundling
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            AddStyles(bundles);
            AddScripts(bundles);
        }

        private static void AddScripts(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/frontend/lib/modernizr/modernizr-2.8.3.js"));

            bundles.Add(new ScriptBundle("~/bundles/libraries").Include(
                                                                        "~/frontend/lib/jquery/dist/jquery.min.js",
                                                                        "~/frontend/lib/bootstrap-sass/assets/javascripts/bootstrap.min.js"));
        }

        private static void AddStyles(BundleCollection bundles)
        {
            var kolumbusStyles = new StyleBundle("~/frontend/content/css").Include(
                                                                                   "~/frontend/public/site.css");

            kolumbusStyles.Transforms.Add(new CssMinify());
            bundles.Add(kolumbusStyles);
        }
    }
}
