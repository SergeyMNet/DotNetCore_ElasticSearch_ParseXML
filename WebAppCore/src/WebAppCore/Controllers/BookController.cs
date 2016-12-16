using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppCore.Data;
using WebAppCore.Models.BookViewModel;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using WebAppCore.Services;


namespace WebAppCore.Controllers
{
    public class BookController : Controller
    {
        private readonly StorageContext _context;
        private readonly IElasticSearch _elasticSearch;
        
        public BookController(StorageContext context, IElasticSearch elasticSearch)
        {
            _context = context;
            _elasticSearch = elasticSearch;

            
        }

        // GET: Book/GetCategories
        [HttpGet]
        public IActionResult GetCategories()
        {
            return Json(_elasticSearch.GetAllTypes());
        }

        // GET: Book/GetFields?cat={category name}
        [HttpGet]
        public IActionResult GetFields(string cat)
        {
            return Json(_elasticSearch.GetAllFields(cat));
        }


        // GET: Book/GetXml?filePath={some url}
        [HttpGet]
        public IActionResult GetXml(string filePath)
        {
            if (filePath == null)
                return NotFound();

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    var doc = new XmlDocument();
                    doc.Load(stream);

                    return Content(doc.InnerXml, "text/xml");
                }
            }
            catch (Exception)
            {
                return NotFound();
            }
        }


        
        // GET: Book/Search
        [HttpGet]
        public IActionResult Search()
        {
            return View(new SearchVM());
        }

        // POST: Book/Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(SearchModel q)
        {
            var isAjax = HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";


            if (String.IsNullOrEmpty(q.SearchQuery))
            {
                return View(new SearchVM(new List<BookBase>()));
            }

            // ES
            List<BookBase> elRes = _elasticSearch.Search(q);
            

            var result = new SearchVM(elRes);
            
            await Task.Delay(1000);
            
            return View(result);
        }


        // GET: Book/Submit
        [HttpGet]
        public IActionResult Submit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(List<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return View();
                }

                long size = files.Sum(f => f.Length);
                string result = "";

                if (size > 0)
                {
                    string filePath = "";
                    foreach (var formFile in files)
                    {
                        if (formFile.Length > 0)
                        {
                            // full path to file in temp location
                            filePath = Path.Combine(System.AppContext.BaseDirectory,
                                @"..\..\..\Temp\" + formFile.FileName);
                            
                            #region Storage
                            // Save to local Storage
                            using (var stream = new FileStream(filePath, FileMode.OpenOrCreate))
                            {
                                // todo - refactor (dal save files to storage)
                                // copy to storage
                                await formFile.CopyToAsync(stream);
                            } 
                            #endregion

                            #region DB
                            // todo - refactor (dal save files to db)
                            // save to DB
                            string name = Path.GetFileNameWithoutExtension(formFile.FileName);
                            var hasAny = _context.Books.FirstOrDefault(b => b.ID == name);
                            if (hasAny != null)
                            {
                                hasAny.Title = name;
                                hasAny.Url = filePath;

                                _context.Books.Update(hasAny);
                            }
                            else
                            {
                                _context.Books.Add(new BookBase()
                                {
                                    ID = name,
                                    Title = name,
                                    Url = filePath
                                });
                            }
                            _context.SaveChanges();
                            #endregion
                            
                            #region ES
                            // ES
                            
                            result = _elasticSearch.SaveObject(filePath); 
                            #endregion
                        }
                    }



                    ViewBag.Message = result; // "Files save success.";
                    return View();
                }

                ViewBag.Message = "Nothing to save...";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View();
            }
        }
        

        // GET: Book
        public async Task<IActionResult> Index(int page = 1)
        {
            double maxItems = _elasticSearch.GetCount();

            int pageSize = 5;
            int countPages = (int)Math.Ceiling(maxItems / pageSize);
            int startIndex = (page*pageSize) - pageSize;
            var res = _elasticSearch.GetAll(startIndex, pageSize);
            var resVM = new ListBooksVM(res);
            resVM.CurentPage = page;
            resVM.CountPages = (int) countPages;

            return View(resVM);
        }

        // GET: Book/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookBase = await _context.Books.SingleOrDefaultAsync(m => m.ID == id);
            if (bookBase == null)
            {
                return NotFound();
            }

            return View(bookBase);
        }

        // GET: Book/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Book/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Title,Url")] BookBase bookBase)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bookBase);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(bookBase);
        }

        // GET: Book/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookBase = await _context.Books.SingleOrDefaultAsync(m => m.ID == id);
            if (bookBase == null)
            {
                return NotFound();
            }
            return View(bookBase);
        }

        // POST: Book/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ID,Title,Url")] BookBase bookBase)
        {
            if (id != bookBase.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookBase);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookBaseExists(bookBase.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(bookBase);
        }

        // GET: Book/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var bookBase = await _context.Books.SingleOrDefaultAsync(m => m.ID == id);
            //if (bookBase == null)
            //{
            //    return NotFound();
            //}

            var bookBase = _elasticSearch.GetById(id);

            if (bookBase == null)
            {
                return NotFound();
            }

            return View(bookBase);
        }

        // POST: Book/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            #region DB

            var bookBase = await _context.Books.SingleOrDefaultAsync(m => m.ID == id);
            if (bookBase != null)
            {
                _context.Books.Remove(bookBase);
                await _context.SaveChangesAsync();
            } 
            #endregion

            _elasticSearch.Delete(id);

            return RedirectToAction("Index");
        }

        private bool BookBaseExists(string id)
        {
            return _context.Books.Any(e => e.ID == id);
        }
    }
}
