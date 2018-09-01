using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlToSql
{
    class Item
    {
        //Variables
        public string key, value;

        //Constructor
        public Item(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
