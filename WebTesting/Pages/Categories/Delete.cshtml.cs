using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTesting.Models;
using WebTesting.Services;

namespace WebTesting.Pages.Categories
{
    [BindProperties]
	public class DeleteModel : PageModel
	{
		private readonly ApplicationDbContext _db;
		//[BindProperty]
		public Category Category { get; set; }
		public DeleteModel(ApplicationDbContext db)
		{
			_db = db;
		}
		public void OnGet(int id)
		{

			Category = _db.Category.Find(id);
			//FirstOrDefault SingleOrDefault Where().FirtsOrDefaul - uvnit5 jsou lambdy (u -> u.Id == id)
		}
		public async Task<IActionResult> OnPost(/*Category category*/) {
			var categoryFromDb = _db.Category.Find(Category.Id);
			if (categoryFromDb != null)
			{
				_db.Category.Remove(categoryFromDb);
				await _db.SaveChangesAsync();
				TempData["success"] = "Category deleted successfully.";
				return RedirectToPage("Index");
			}
			return Page();
		}
	}
}
