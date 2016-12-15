using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAppCore.Models.BookViewModel
{
    public class SearchModel
    {
        [Required]
        [StringLength(100)]
        public string SearchQuery { get; set; }
    }
}
