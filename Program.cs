using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XmlToSql
{
    class Program
    {
        //Variables
        static XmlDocument doc;
        static XmlNodeList items;
        static XmlElement root;
        static ArrayList[] attributes; //Array of arraylists
        static string path = "pathToXmlFile";
        static int numOfAttr = 0;


        public static void Main(string[] args)
        {
            Init(path); //Initiates the process
            Process(); //Reads and parses the XML File
            generateDBScript(); //Generates the script
        }

        public static void Init(string mpath)
        {
            doc = new XmlDocument();
            
            if (File.Exists(mpath)){
                doc.Load(mpath);
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        public static bool getItems(XmlDocument document)
        {
            root = doc.DocumentElement; // Root Node

            if(root == null) // Empty file
            {
                return false;
            }

            items = root.ChildNodes;    // Items

            if (items.Count == 0)
            {
                return false;
            }
            return true;
        }

        public static void getAttributes()
        {
            /*
             * numOfAttrs : Columns of the table
             * attributes : the array of arraylists
             * each element of attributes stores the data of related column, column name is given as the first element of each different arraylist
             * 
             */
            XmlNode aChild = items[0];
            
            int index = 0; 

            if (aChild.HasChildNodes) //If the columns are distributed by inner tags
            {
                numOfAttr = aChild.ChildNodes.Count;
                attributes = new ArrayList[numOfAttr];

                foreach (XmlNode node in aChild.ChildNodes)
                {
                    ArrayList list = new ArrayList();

                    string tagName = node.Name;
                    list[0] = tagName;

                    attributes[index] = list;
                    index++;
                }
            }
            else //If the columns are distributed by attributes inside the tags
            {
                numOfAttr = aChild.Attributes.Count;
                attributes = new ArrayList[numOfAttr];

                foreach(XmlAttribute a in aChild.Attributes)
                {
                    ArrayList list = new ArrayList();

                    string tagName = a.Name;
                    list.Add(tagName);

                    attributes[index] = list;
                    index++;
                }
            }
        }

        public static void Process()
        {
            if (getItems(doc))
            {
                getAttributes();

                foreach(XmlNode item in items)
                {
                    if (item.HasChildNodes)//Second case : item distributed with child nodes
                    {
                        for(int i=0; i < item.ChildNodes.Count; i++)
                        {
                            string tag = attributes[i][0].ToString();
                            string val = item.SelectSingleNode(tag).InnerText;

                            attributes[i].Add(val);
                        }
                    }
                    else //First case : item distributed with attributes
                    {
                        for(int i=0; i<numOfAttr; i++)
                        {
                            string tag = attributes[i][0].ToString();
                            string val = item.Attributes[tag].Value.ToString();

                            attributes[i].Add(val);
                        }
                    }
                }
            }
        }



        public static void generateDBScript()
        {
            string sqlCommand = "CREATE TABLE "+root.Name.ToString()+"(";

            for(int i=0; i < numOfAttr; i++){
                if(i == numOfAttr - 1)
                {
                    sqlCommand += attributes[i][0].ToString()+" VARCHAR(50)";
                }else
                {
                    sqlCommand += attributes[i][0].ToString()+" VARCHAR(50), ";
                }                
            }
            sqlCommand += ");$";

            for(int i=0; i < items.Count; i++)
            {
                sqlCommand += "$INSERT INTO " + root.Name.ToString() + " VALUES(";
                for (int k=0; k < numOfAttr; k++)
                {
                    if (k == numOfAttr - 1)
                    {
                        sqlCommand += attributes[k][i + 1];
                    }
                    else
                    {
                        sqlCommand += attributes[k][i + 1]+", ";
                    }                    
                }
                sqlCommand += ");";
            }

            sqlCommand = sqlCommand.Replace("$", "" + System.Environment.NewLine);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("pathForSavingTheFile.txt"))
            {
                file.WriteLine(sqlCommand);
                Console.WriteLine("Complete! sqlOutput.txt has been generated.");
                Console.ReadKey();
            }


        }
               
    }
}
