using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using WebAppCore.Models.ES_Models;

namespace WebAppCore.Services
{
    public static class ConverterXml_to_Model
    {
        public static ModelES ToModelEs(XmlDocument doc)
        {
            ModelES result = new ModelES();

            foreach (XmlNode docChildNode in doc.ChildNodes)
            {
                result.NodeProps.Add(TryConvert(docChildNode));
            }
            
            return result;
        }

        private static NodeProp TryConvert(XmlNode node)
        {
            List<NodeProp> res = new List<NodeProp>();


            if (node.HasChildNodes && node.ChildNodes.Count > 1)
            {
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    res.Add(TryConvert(childNode));
                }

            }

            return new NodeProp() { Title = node.Name, Value = node.InnerText, NodeProps = res};
        }
    }
}
