using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml;

namespace jFunc.Azure
{
    public class Blob
    {
        public string Path { get; private set; } = "";
        public string Name { get; private set; }
        public int Size { get; private set; } = -1;
        public bool IsFile { get => Size > 0; }

        public Dictionary<string, string> Properties { get;private set; } = new Dictionary<string, string>();

        Messages msg;
        internal Blob(Messages st, XmlElement n)
        {
            msg = st;
            var l = n["Name"].InnerText.Split(Messages.delimiter).Where(x => x.Trim() != "");
            Name = l.Last();
            if (l.Count() > 1) Path = String.Join(Messages.delimiter + "", l.Take(l.Count() - 1));
            var props = n["Properties"];
            if (props != null)
            {
                props = props["Content-Length"];
                if (props != null) Size = Int32.Parse(props.InnerText);
            }

            var meta = n["Metadata"];
            if (meta!= null)
            {
                var c= meta.FirstChild;
                while (c != null)
                {
                    if (c is XmlElement) Properties[((XmlElement)c).LocalName]= ((XmlElement)c).InnerText;
                    c=c.NextSibling; 
                } 
            }
        }

        internal Blob(Messages st, string path,int size)
        {
            msg = st;
            var l = path.Split(Messages.delimiter).Where(x => x.Trim() != "");
            Name = l.Last();
            if (l.Count() > 1) Path = String.Join(Messages.delimiter + "", l.Take(l.Count() - 1));
            Size = size;
        }


        public bool Delete() => msg.Delete(this);

        public bool Download(string folder) => msg.Download(this,folder);
    }
}
