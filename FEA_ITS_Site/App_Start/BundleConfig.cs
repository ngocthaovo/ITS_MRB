using System.Web;
using System.Web.Optimization;

namespace FEA_ITS_Site
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/FEA/js").Include(
                        "~/assets/plugins/jquery-ui/jquery-ui-1.10.2.custom.min.js",
                        "~/assets/plugins/bootstrap/js/bootstrap.min.js",
                        "~/assets/plugins/blockUI/jquery.blockUI.js",
                        "~/assets/plugins/iCheck/jquery.icheck.min.js",
                        "~/assets/plugins/perfect-scrollbar/src/jquery.mousewheel.js",
                        "~/assets/plugins/perfect-scrollbar/src/perfect-scrollbar.js",
                        "~/assets/js/main.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));

            bundles.Add(new StyleBundle("~/FEA/css").Include(
            "~/assets/plugins/bootstrap/css/bootstrap.min.css",
            "~/assets/plugins/font-awesome/css/font-awesome.min.css",
            "~/assets/fonts/style.css",
            "~/assets/css/main.css",
            "~/assets/css/main-responsive.css",
            "~/assets/plugins/iCheck/skins/all.css",
            "~/assets/plugins/perfect-scrollbar/src/perfect-scrollbar.css",
            "~/assets/css/theme_light.css"));


        }


    }
}