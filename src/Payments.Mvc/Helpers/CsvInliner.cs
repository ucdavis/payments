using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Payments.Mvc.Helpers
{
    public static class CsvInliner
    {
        public static IHtmlContent EmbedCss(this IHtmlHelper html, string path)
        {
            var host = (IHostingEnvironment)html.ViewContext.HttpContext.RequestServices.GetService(typeof(IHostingEnvironment));
            var root = host.ContentRootPath;

            var css = File.ReadAllText(root + path);
            var styleElement = new TagBuilder("style");
            styleElement.InnerHtml.AppendHtml(css);

            return styleElement;
        }
    }
}
