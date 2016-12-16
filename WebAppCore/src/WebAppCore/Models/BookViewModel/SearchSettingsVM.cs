using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WebAppCore.Models.BookViewModel
{
    public class SearchSettingsVM
    {
        public List<CategorieVM> Categories { get; set; } = new List<CategorieVM>();

        public SearchSettingsVM(List<CategorieVM> categories)
        {
            this.Categories = categories;
        }

        public SearchSettingsVM()
        {
            
        }
    }

    public class CategorieVM
    {
        public string Title { get; set; }
        public List<string> Fields { get; set; } = new List<string>();

        public CategorieVM(string title, List<string> fields)
        {
            this.Fields = fields;
            this.Title = title;
        }
    }
}
