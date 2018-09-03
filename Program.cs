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

    /// <summary>
    /// 
    /// XmlNodeList items :  contains the items inside an xml file under a root node
    /// parameters  : Array of Arraylists , contains Arraylists which starts with a tag and continues with the values e.g | "key"-val1-val2-val3-val4.... | "key2"-val1-val2-val3-val4.... |
    /// numOfParameter : number of columns, item properties e.g      <item1> <p1></p1>  <p2></p2>  <p3></p3> </item1> OR <item1 p1="" p2=""></item1>
    /// 
    /// 
    /// </summary>
    class Program
    {
        //Variables
        static XmlDocument doc;
        static XmlNodeList items;
        static XmlElement root;
        static ArrayList[] parametersAndValues; //Array of arraylists
        static string path = "C:/Users/etuna/source/repos/XmlToSql/newtest.xml";
        static int numOfParameter = 0;
        static Hashtable hashtable;
        static ArrayList itemValArrayList;
        static ArrayList parameters;
        //----------------------------------------------------------------------

        static ArrayList[] attributes; //Array of arraylists
        static string path = "pathToXmlFile";
        static int numOfAttr = 0;

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            Init(path); //Initiates the process
            PopulateData(); //Reads and parses the XML File
            GenerateDBScript(); //Generates the script
        }

        /// <summary>
        /// It initiates the process
        /// Checks whether the file exists or not, loads it if so
        /// </summary>
        /// <param name="mpath"></param>
        public static void Init(string mpath)
        {
            doc = new XmlDocument();

            if (File.Exists(mpath))
            {
                doc.Load(mpath);
            }
            else
            {
                throw new FileNotFoundException();
            }
        }

        /// <summary>
        /// It gets items given a XmlDocument
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static bool getItems(XmlDocument document)
        {
            root = doc.DocumentElement; // Root Node

            if (root == null) // Empty file
            {
                return false;
            }

            items = root.ChildNodes;    // Items

            if (items.Count == 0)
            {
                return false;
            }
            hashtable = new Hashtable();

            return true;
        }
        
        /// <summary>
        /// getXmlParameters() method gets the parameter in the xml file.
        /// 
        /// Parameters can be distributed with child nodes or as attributes in an item.
        /// The method finds the parameters in either cases.
        /// 
        /// </summary>
        public static void getXmlParameters()
        {
            /*
             * numOfParameter : Columns of the table
             * parameters : the array of arraylists
             * each element of parameters stores the data of related column, column name is given as the first element of each different arraylist
             * 
             */
            XmlNode aChild = items[0];

            int index = 0;

            if (aChild.HasChildNodes) //If the columns are distributed by inner tags e.g <item> <child1></child1> </item>
            {

                numOfParameter = aChild.ChildNodes.Count;
                parametersAndValues = new ArrayList[numOfParameter];

                foreach (XmlNode node in aChild.ChildNodes)
                {
                    ArrayList list = new ArrayList();

                    string tagName = node.Name;
                    list[0] = tagName;

                    parametersAndValues[index] = list;


                    index++;
                }
            }
            else //If the columns are distributed with attributes inside the tags e.g <item attribute1 = "" attribute2 = ""></item>
            {
                numOfParameter = aChild.Attributes.Count;
                parametersAndValues = new ArrayList[numOfParameter];

                foreach (XmlAttribute a in aChild.Attributes)
                {
                    ArrayList list = new ArrayList();

                    string tagName = a.Name;
                    list.Add(tagName);

                    parametersAndValues[index] = list;
                    index++;
                }
            }
        }
        
        /// <summary>
        /// 
        /// PopulateData() populates the data into the arraylists in two different cases as given above
        ///
        /// 
        /// </summary>
        public static void PopulateData()
        {
            if (getItems(doc))
            {
                getXmlParameters();

                foreach (XmlNode item in items)
                {
                    if (item.HasChildNodes)//Second case : item distributed with child nodes
                    {
                        for (int i = 0; i < item.ChildNodes.Count; i++)
                        {
                            string tag = parametersAndValues[i][0].ToString();
                            string val = item.SelectSingleNode(tag).InnerText;

                            parametersAndValues[i].Add(val);
                        }
                    }
                    else //First case : item distributed with parameters
                    {
                        for (int i = 0; i < numOfParameter; i++)
                        {
                            string tag = parametersAndValues[i][0].ToString();
                            string val = item.Attributes[tag].Value.ToString();

                            parametersAndValues[i].Add(val);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 
        /// GenerateDBScript() generates the sql command and writes it into a txt file
        /// 
        /// </summary>
        public static void GenerateDBScript()
        {
            string sqlCommand = "CREATE TABLE " + root.Name.ToString() + "(";

            for (int i = 0; i < numOfParameter; i++)
            {
                if (i == numOfParameter - 1)
                {
                    sqlCommand += parametersAndValues[i][0].ToString() + " VARCHAR(50)";
                }
                else
                {
                    sqlCommand += parametersAndValues[i][0].ToString() + " VARCHAR(50), ";
                }
            }
            sqlCommand += ");$";

            for (int i = 0; i < items.Count; i++)
            {
                sqlCommand += "$INSERT INTO " + root.Name.ToString() + " VALUES(";
                for (int k = 0; k < numOfParameter; k++)
                {
                    if (k == numOfParameter - 1)
                    {
                        sqlCommand += parametersAndValues[k][i + 1];
                    }
                    else
                    {
                        sqlCommand += parametersAndValues[k][i + 1] + ", ";
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
        
        /// <summary>
        /// It generates hashtable from the Array Of Arraylist that holds the parameters and the values
        /// It simplifies the structure, it is only key value pair. KEY : TAG, VALUE : ARRAYLIST of items' attributes
        /// </summary>
        static void generateHashtable()
        {
            for (int i = 0; i < numOfParameter; i++)
            {
                string tag = parametersAndValues[i][0].ToString();
                ArrayList tempList = new ArrayList();

                for (int k = 0; k < items.Count; k++)
                {
                    KeyValuePair<string, string> kvp = new KeyValuePair<string, string>("item" + k, parametersAndValues[i][k + 1].ToString());
                    tempList.Add(kvp);
                }

                hashtable.Add(tag, tempList);
            }
        }

        /// <summary>
        /// This method finds the value with a given tag
        /// </summary>
        /// <param name="itemNo"></param>
        /// <param name="tag"></param>
        /// <returns> a string value </returns>
        static string GetValueOfItemGivenTag(int itemNo, string tag)
        {
            return ((ArrayList)hashtable[tag])[itemNo].ToString();
        }

        /// <summary>
        /// This method returns the item info, given an itemNo
        /// </summary>
        /// <param name="itemNo"></param>
        /// <returns> a string information of an item </returns>
        static string ItemInfo(int itemNo)
        {
            String itemStr = "Item" + itemNo + "\n";

            for (int i = 0; i < numOfParameter; i++)
            {

                itemStr += parametersAndValues[i][0] + " : " + GetValueOfItemGivenTag(itemNo, parametersAndValues[i][0].ToString()) + "\n";
            }
            return itemStr;
        }
        
        /// <summary>
        /// This method takes a string tableName and a hashtable(attributes and types e.g KEY VARCHAR(60))
        /// And generates a CREATE TABLE sql command
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="attrAndTypes"></param>
        /// <returns></returns>
        static string CreateTableCommand(string tableName, Hashtable attrAndTypes)
        {
            string command = "CREATE TABLE " + tableName + "(";

            foreach (KeyValuePair<string, string> kvp in attrAndTypes)
            {
                string key = kvp.Key;
                string val = kvp.Value;

                command += key + " " + val + ", ";
            }

            command.Substring(0, command.Length - 2);
            command += ");";
            return command;
        }
        
        /// <summary>
        /// This method takes a table name and generates INSERT sql command for the items
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>an sql command</returns>
        public string InsertTableCommand(string tableName)
        {
            string sqlCommand = "";
            for (int i = 0; i < items.Count; i++)
            {
                sqlCommand += "INSERT INTO " + tableName + " VALUES(";
                for (int k = 0; k < numOfParameter; k++)
                {
                    if (k == numOfParameter - 1)
                    {
                        sqlCommand += parametersAndValues[k][i + 1];
                    }
                    else
                    {
                        sqlCommand += parametersAndValues[k][i + 1] + ", ";
                    }
                }
                sqlCommand += ");";
            }
            return sqlCommand;
        }

        /// <summary>
        /// It generates the parameters and stores them in an arraylist
        /// Takes an array of arraylists e.g parametersAndValues
        /// and returns a pure parameter arraylist
        /// </summary>
        /// <param name="arr"></param>
        /// <returns>a pure parameter(columns) arraylists e.g |NAME|AGE|ADDRESS|CREDIT CARD|ID NO|.... </returns>
        public ArrayList ParameterGeneration(ArrayList[] arr)
        {
            ArrayList tempParameters = new ArrayList();

            foreach(ArrayList al in arr)
            {
                tempParameters.Add(al[0]);
            }

            return tempParameters;
        }

    }
}
