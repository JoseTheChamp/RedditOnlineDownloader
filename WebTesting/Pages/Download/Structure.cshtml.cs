using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTesting.Pages.Download
{
    public class StructureModel : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnGetDownload() { 
            //TODO Start download procces.
            return RedirectToPage("../Index");
        }
    }
}
