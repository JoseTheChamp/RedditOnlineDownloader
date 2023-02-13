using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTesting.Models;
using WebTesting.Services;

namespace WebTesting.Pages.Categories
{
    [BindProperties]
	public class EditModel : PageModel
	{
		private readonly ApplicationDbContext _db;
		//[BindProperty]
		public Category Category { get; set; }
		public EditModel(ApplicationDbContext db)
		{
			_db = db;
		}
		public void OnGet(int id)
		{

			Category = _db.Category.Find(id);
			//FirstOrDefault SingleOrDefault Where().FirtsOrDefaul - uvnit5 jsou lambdy (u -> u.Id == id)
		}
		public async Task<IActionResult> OnPost(/*Category category*/) {
			if (Category.Name == Category.DisplayOrder.ToString()) {
				ModelState.AddModelError("Category.DisplayOrder","Display order cannot match the name.");
			}
			if (ModelState.IsValid)
			{
				_db.Category.Update(Category);
				await _db.SaveChangesAsync();
				TempData["success"] = "Category updated successfully.";
				return RedirectToPage("Index");
			}
			return Page();
		}
	}
}
