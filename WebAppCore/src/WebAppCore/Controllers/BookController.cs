using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppCore.Data;
using WebAppCore.Models.BookViewModel;
using System.Xml.Linq;
using System.Xml;
using Microsoft.AspNetCore.Http;
using NuGet.Protocol.Core.v3;
using WebAppCore.Services;

namespace WebAppCore.Controllers
{
    public class BookController : Controller
    {
        private readonly StorageContext _context;

        public BookController(StorageContext context)
        {
            _context = context;    
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
            await Task.Delay(1000);

            if (String.IsNullOrEmpty(q.SearchQuery))
            {
                return View(new SearchVM(new List<BookBase>()));
            }

            // ES
            var es = new ElasticSearch();
            var elRes = es.Search(q.SearchQuery);

            //var result = new SearchVM(_context.Books.Where(b => b.Title.Contains(q.SearchQuery)).ToList());

            var result = new SearchVM(new List<BookBase>());
            foreach (var re in elRes)
            {
                result.Books.Add(_context.Books.FirstOrDefault(b => b.ID == re));
            }
            

            ViewBag.Message = "";
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
                            var es = new ElasticSearch();
                            result = es.SaveObject(filePath); 
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
        public async Task<IActionResult> Index()
        {
            var es = new ElasticSearch();
            var elRes = es.GetAll(0, 2000);

            return View(elRes);

            //return View(await _context.Books.ToListAsync());
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

            var bookBase = await _context.Books.SingleOrDefaultAsync(m => m.ID == id);
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
            var bookBase = await _context.Books.SingleOrDefaultAsync(m => m.ID == id);
            _context.Books.Remove(bookBase);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool BookBaseExists(string id)
        {
            return _context.Books.Any(e => e.ID == id);
        }
    }
}
