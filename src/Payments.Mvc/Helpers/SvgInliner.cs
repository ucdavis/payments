using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Payments.Mvc.Helpers
{
    public static class SvgInliner
    {
        public static IHtmlContent InlineSvg(this IHtmlHelper html, string path)
        {
            var host = (IWebHostEnvironment)html.ViewContext.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment));
            var root = host.ContentRootPath;

            var svg = File.ReadAllText(root + path);
            return new Microsoft.AspNetCore.Html.HtmlString(svg);
        }
    }
}
