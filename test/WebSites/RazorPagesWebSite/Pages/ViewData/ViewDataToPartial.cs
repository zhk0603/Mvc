using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPagesWebSite
{
    public class ViewDataToPartial : PageModel
    {
        [ViewData]
        public IList<string> AreaCodes { get; set; }

        public void OnGet()
        {
            AreaCodes = new[] { "206", "425" };
        }
    }
}
