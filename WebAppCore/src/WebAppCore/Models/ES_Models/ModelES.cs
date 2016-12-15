using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;

namespace WebAppCore.Models.ES_Models
{
    public class ModelES
    {
        public string UrlStorage { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public List<NodeProp> NodeProps { get; set; } = new List<NodeProp>();
    }

    public class NodeProp
    {
        public string Title { get; set; }
        public string Value { get; set; }

        public List<NodeProp> NodeProps { get; set; } = new List<NodeProp>();
    }
        
}
