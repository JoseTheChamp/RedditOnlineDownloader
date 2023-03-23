using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTesting.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public string UserName { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            UserName = HttpContext.Session.GetString("UserName");
            return Page();
        }

        public void OnPost() { 
            //TODO Mozna nejake if nebo na asction atd
            HttpContext.Session.Clear();
        }
    }
}