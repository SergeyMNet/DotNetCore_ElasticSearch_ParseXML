using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAppCore.Models.BookViewModel
{
    public class SearchVM
    {
        public SearchModel SearchModel { get; set; }
        public List<BookBase> Books { get; set; }

        public SearchVM()
        {
            SearchModel = new SearchModel() { SearchQuery = "" };
        }

        public SearchVM(List<BookBase> list)
        {
            SearchModel = new SearchModel() { SearchQuery = "" };
            this.Books = list;
        }
    }
}
