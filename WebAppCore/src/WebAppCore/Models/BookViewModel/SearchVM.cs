﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAppCore.Models.BookViewModel
{
    public class SearchVM
    {
        public SearchModel SearchModel { get; set; }
        public List<BookBase> Books { get; set; }

        //public SearchSettingsVM SearchSettingsVm { get; set; } = new SearchSettingsVM();



        public SearchVM()
        {
            SearchModel = new SearchModel() { SearchQuery = "" };
        }

        public SearchVM(List<BookBase> list)
        {
            SearchModel = new SearchModel() { SearchQuery = "" };
            this.Books = list;
        }

        //public SearchVM(List<BookBase> list, SearchSettingsVM settings)
        //{
        //    SearchModel = new SearchModel() { SearchQuery = "" };
        //    this.Books = list;
        //    this.SearchSettingsVm = settings;
        //}
    }
}
