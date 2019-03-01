using System.Web;
using System.Web.Optimization;

namespace track
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery-ui").Include(
                        "~/Scripts/jquery-ui.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/dragscroll").Include(
                        "~/Scripts/dragscroll.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/popper.min.js",
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/site").Include(
                      "~/Scripts/moment.js",
                        "~/Scripts/chartist.min.js",
                        "~/Scripts/chartist-plugin-tooltip.min.js",
                        "~/Scripts/bootstrap-datetimepicker.min.js"));
            

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.min.css",
                      "~/Content/bootstrap-datetimepicker.min.css",
                      "~/Content/chartist.min.css",
                      "~/Content/chartist-plugin-tooltip.css",
                      "~/Content/site.css"));
            
        }
    }
}
