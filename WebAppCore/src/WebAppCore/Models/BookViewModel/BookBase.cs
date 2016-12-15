using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppCore.Models.ES_Models;

namespace WebAppCore.Models.BookViewModel
{
    public class BookBase
    {
        public BookBase()
        {
            
        }

        public BookBase(ModelES source)
        {
            this.ID = source.Title;
            this.Title = source.Title;
            this.Url = source.UrlStorage;
            this.Category = source.Category;
        }

        public string ID { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Category { get; set; }
    }
}
