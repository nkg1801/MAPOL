using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Collections;

namespace MAPol
{
    internal static class EclassMapping
    {
        static Dictionary<int,string> eClassMapping = new Dictionary<int,string>();

        internal static Dictionary<int, string> GetMDMapping()
        {
            eClassMapping.Clear();
            XmlDocument doc = new XmlDocument();
            doc.Load("mapping.xml");
            XmlNodeList nodes = doc.ChildNodes;
            foreach (XmlNode node in nodes)
            {
                if (node.Name == "Mapping")
                {
                    XmlNodeList xmlEclassList = node.ChildNodes;
                    foreach (XmlNode n in xmlEclassList)
                    {
                        int eclass = int.Parse(n.Attributes["Number"].Value);
                        string visualObject = n.Attributes["MDVisualObjectMapping"].Value;
                        if (eClassMapping.ContainsKey(eclass))
                        {
                            eClassMapping[eclass] = visualObject;
                        }
                        else
                        {
                            eClassMapping.Add(eclass, visualObject);
                        }
                    }
                }
            }
            return eClassMapping;
        }

        internal static Dictionary<int, string> GetXaMapping()
        {
            eClassMapping.Clear();
            XmlDocument doc = new XmlDocument();
            doc.Load("mapping.xml");
            XmlNodeList nodes = doc.ChildNodes;
            foreach (XmlNode node in nodes)
            {
                if (node.Name == "Mapping")
                {
                    XmlNodeList xmlEclassList = node.ChildNodes;
                    foreach (XmlNode n in xmlEclassList)
                    {
                        int eclass = int.Parse(n.Attributes["Number"].Value);
                        string visualObject = n.Attributes["MDVisualObjectMapping"].Value;
                        if (eClassMapping.ContainsKey(eclass))
                        {
                            eClassMapping[eclass] = visualObject;
                        }
                        else
                        {
                            eClassMapping.Add(eclass, visualObject);
                        }
                    }
                }
            }
            return eClassMapping;
        }

        internal static Dictionary<int, string> GetThisApplicationMapping()
        {
            eClassMapping.Clear();
            XmlDocument doc = new XmlDocument();
            doc.Load("mapping.xml");
            XmlNodeList nodes = doc.ChildNodes;
            foreach (XmlNode node in nodes)
            {
                if (node.Name == "Mapping")
                {
                    XmlNodeList xmlEclassList = node.ChildNodes;
                    foreach (XmlNode n in xmlEclassList)
                    {
                        int eclass = int.Parse(n.Attributes["Number"].Value);
                        string visualObject = n.Attributes["VOM"].Value;
                        if (eClassMapping.ContainsKey(eclass))
                        {
                            eClassMapping[eclass] = visualObject;
                        }
                        else
                        {
                            eClassMapping.Add(eclass, visualObject);
                        }
                    }
                }
            }
            return eClassMapping;
        }
    }
}
