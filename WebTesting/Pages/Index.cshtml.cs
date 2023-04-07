using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTesting.Pages
{
    public class IndexModel : PageModel
    {
        public string UserName { get; set; }
        public string AllPosts { get; set; }

        public IActionResult OnGet()
        {
            UserName = HttpContext.Session.GetString("UserName");
            AllPosts = HttpContext.Session.GetString("AllPosts");
            return Page();
        }

        public void OnPost() { 
            HttpContext.Session.Clear();
        }
    }
}