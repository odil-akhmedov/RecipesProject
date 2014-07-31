using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RecipeProject;
using System.IO;
using RecipeProject.Models;

namespace RecipeProject.Controllers
{
    public class RecipesController : Controller
    {
        private Team_2_RecipesEntities db = new Team_2_RecipesEntities();

        // GET: Recipes
        public ActionResult Index(string sortOrder)
        {
            ViewBag.TitleSortParm = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewBag.IngridientSortParm = String.IsNullOrEmpty(sortOrder) ? "ingridients_desc" : "";
            ViewBag.DirectionsSortParm = String.IsNullOrEmpty(sortOrder) ? "directions_desc" : "";
            ViewBag.PrepTimeSortParm = String.IsNullOrEmpty(sortOrder) ? "prep_time_desc" : "";
            ViewBag.CookingTimeSortParm = String.IsNullOrEmpty(sortOrder) ? "cooking_time_desc" : "";
            ViewBag.NOSSortParm = String.IsNullOrEmpty(sortOrder) ? "nos_desc" : "";
            var recipes = db.Recipes.Include(r => r.User);

            switch (sortOrder)
            {
                case "title_desc":
                    recipes = recipes.OrderByDescending(s => s.Title);
                    break;
                case "ingridients_desc":
                    recipes = recipes.OrderBy(s => s.Ingridients);
                    break;
                case "directions_desc":
                    recipes = recipes.OrderByDescending(s => s.Directions);
                    break;
                case "prep_time_desc":
                    recipes = recipes.OrderByDescending(s => s.Prep_time);
                    break;
                case "cooking_time_desc":
                    recipes = recipes.OrderByDescending(s => s.Cooking_time);
                    break;
                case "nos_desc":
                    recipes = recipes.OrderByDescending(s => s.NumberOfServings);
                    break;
                default:
                    recipes = recipes.OrderBy(s => s.Title);
                    break;
            }
        
            return View(recipes.ToList());
        }

        // GET: Recipes/Details/5
        public ActionResult Details(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Recipe recipe = db.Recipes.Find(id);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            return View(recipe);
        }

        // GET: Recipes/Create
        public ActionResult Create()
        {
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName");
            return View();
        }

        // POST: Recipes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Ingridients,Directions,Prep_time,Cooking_time,NumberOfServings,ImgSrc,UserId")] Recipe recipe)
        {
            if (ModelState.IsValid)
            {
                db.Recipes.Add(recipe);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", recipe.UserId);
            return View(recipe);
        }

        // GET: Recipes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Recipe recipe = db.Recipes.Find(id);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", recipe.UserId);
            return View(recipe);
        }

        // POST: Recipes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Ingridients,Directions,Prep_time,Cooking_time,NumberOfServings,ImgSrc,UserId")] Recipe recipe)
        {
            if (ModelState.IsValid)
            {
                db.Entry(recipe).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", recipe.UserId);
            return View(recipe);
        }

        // GET: Recipes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Recipe recipe = db.Recipes.Find(id);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            return View(recipe);
        }

        // POST: Recipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Recipe recipe = db.Recipes.Find(id);
            db.Recipes.Remove(recipe);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // Images controller

        [HttpGet]
        public ActionResult Show(int? id)
        {
            string mime;
            byte[] bytes = LoadImage(id.Value, out mime);
            return File(bytes, mime);
        }

        [HttpPost]
        public ActionResult Upload()
        {
            SuccessModel viewModel = new SuccessModel();
            if (Request.Files.Count == 1)
            {
                var name = Request.Files[0].FileName;
                var size = Request.Files[0].ContentLength;
                var type = Request.Files[0].ContentType;
                viewModel.Success = HandleUpload(Request.Files[0].InputStream, name, size, type);
            }
            return Json(viewModel);
        }

        private bool HandleUpload(Stream fileStream, string name, int size, string type)
        {
            bool handled = false;

            try
            {
                byte[] documentBytes = new byte[fileStream.Length];
                fileStream.Read(documentBytes, 0, documentBytes.Length);

                Recipe databaseDocument = new Recipe
                {
                    ImgFileContent = documentBytes,
                    ImgName = name,
                    ImgSize = size,
                    ImgType = type
                };
                

                using (Team_2_RecipesEntities databaseContext = new Team_2_RecipesEntities())
                {
                    databaseContext.Recipes.Add(databaseDocument);
                    handled = (databaseContext.SaveChanges() > 0);
                }
            }
            catch (Exception ex)
            {
                // Oops, something went wrong, handle the exception
            }

            return handled;
        }

        private byte[] LoadImage(int id, out string type)
        {
            byte[] fileBytes = null;
            string fileType = null;
            using (Team_2_RecipesEntities databaseContext = new Team_2_RecipesEntities())
            {
                var databaseDocument = databaseContext.Recipes.FirstOrDefault(doc => doc.Id == id);
                if (databaseDocument != null)
                {
                    fileBytes = databaseDocument.ImgFileContent;
                    fileType = databaseDocument.ImgType;
                }
            }
            type = fileType;
            return fileBytes;
        }

    }
}
