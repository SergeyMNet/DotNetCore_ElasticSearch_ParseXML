using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAppCore.Models.BookViewModel
{
    public class ListBooksVM
    {
        public List<BookBase> BookBases { get; set; } = new List<BookBase>();

        public int CurentPage { get; set; } = 0;
        public int CountPages { get; set; } = 0;

        public ListBooksVM()
        {
            
        }

        public ListBooksVM(List<BookBase> books)
        {
            this.BookBases = books;
        }
    }
}
