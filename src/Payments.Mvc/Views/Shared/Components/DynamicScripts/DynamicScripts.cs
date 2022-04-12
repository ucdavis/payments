using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Caching.Memory;

namespace Payments.Mvc.Views.Shared.Components.DynamicScripts
{
    [ViewComponent(Name = "DynamicScripts")]
    public class DynamicScripts : ViewComponent
    {
        private readonly IFileProvider _fileProvider;
        private readonly IMemoryCache _memoryCache;

        public DynamicScripts(IFileProvider fileProvider, IMemoryCache memoryCache)
        {
            this._memoryCache = memoryCache;
            this._fileProvider = fileProvider;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _memoryCache.GetOrCreateAsync<DynamicScriptModel>("DynamicScripts", async (cacheEntry) =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                // Get the CRA generated index file, which includes optimized scripts
                var indexPage = _fileProvider.GetFileInfo("ClientApp/build/index.html");

                // read the file
                var fileContents = await File.ReadAllTextAsync(indexPage.PhysicalPath);

                // find all script tags
                var scriptTags = Regex.Matches(fileContents, "<script.*?</script>", RegexOptions.Singleline);

                // get the script tags as strings
                var scriptTagsAsStrings = scriptTags.Select(m => m.Value);

                // get the ones with a src attribute
                var scriptTagsWithSrc = scriptTagsAsStrings.Where(s => s.Contains("src=")).ToArray();

                // get the one with inline script (bootstrapper)
                var scriptTagWithInlineScript = scriptTagsAsStrings.Where(s => s.StartsWith("<script>!function")).Single();

                // cut off the ends (<script> and </script>) to just get the inner function
                // a little hacky, but it works
                var scriptTagWithInlineScriptContents = scriptTagWithInlineScript.Substring(8, scriptTagWithInlineScript.Length - (8 + 9));

                return new DynamicScriptModel { Scripts = scriptTagsWithSrc, Inline = scriptTagWithInlineScriptContents };
            });

            return View(model);
        }
    }

    public class DynamicScriptModel
    {
        public string[] Scripts { get; set; }
        public string Inline { get; set; }
    }
}