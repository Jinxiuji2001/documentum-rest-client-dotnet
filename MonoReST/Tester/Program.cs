﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Emc.Documentum.Rest.DataModel;
using Emc.Documentum.Rest.Net;
using Emc.Documentum.Rest.Http.Utility;
using System.IO;
using System.Collections.Specialized;
using System.Windows.Forms;

namespace Emc.Documentum.Rest.Test
{
    class Program
    {
        private static string ReSTHomeUri;
        private static string username;
        private static string password;
        private static ReSTController client;
        private static string repositoryName;
        private static bool printResult;

        [STAThread]
        static void Main(string[] args)
        {

            Console.ForegroundColor = ConsoleColor.White;
           // Console.BufferHeight = 360;
           // Console.BufferHeight = 210;
            SetupTestData(false);

            String line = PrintMenu();
            while(!(line.Equals("x") || line.Equals("exit")) )
            {
                if (line.Equals("exit")) break;
                String test = "";
                if(line != null && !line.Trim().Equals("")) {
                    if(line.IndexOf(" ") > 0) {
                        test = line.Substring(0, line.IndexOf(" "));
                    }
                    else
                    {
                        test = line;
                    }
                } 
                if(!test.Equals("")) {
                    String cmd = line.Substring(line.IndexOf(" ") + 1);
                    if (cmd.Equals(line)) cmd = "";

                    //Mock of setting up an array for properties to be passed
                 var properties=new List<KeyValuePair<string,string>> ();
                    properties.Add(new KeyValuePair<string, string>("object_name", @"LIKE '%Test%'"));
                    properties.Add(new KeyValuePair<string, string>("object_type", "dm_document"));

                    switch (test.ToLower())
                    {
                        case "dql":
                            DqlQueryTest.Run(client, ReSTHomeUri, cmd, 20, false, repositoryName, printResult);
                            break;
 
                        case "test": // will run the conditions for Processdoc
                        case "Processdoc":
                            //if (cmd == null || cmd.Trim().Equals("") || cmd.Equals(test))
                            //{
                            //    Console.WriteLine("Path argument is required");
                            //    break;
                            //}
                            // Creates documents, assigns them to a temp holding area, then creates case/request folders
                            // and assigns the documents randomly. Cleans up the temp folder upon completion.
                            int numDocs = getInputNumber("How many documents do you want to create and assign?", 10);
                            int threadCount = getInputNumber("How many threads would you like to run?", 1);
                            long start = DateTime.Now.Ticks;
                            if (threadCount >= 1)
                            {
                                for (int i = 0; i < threadCount; i++)
                                {
                                    UseCaseTests aTest = new UseCaseTests(new ReSTController(username, password), 
                                            ReSTHomeUri, repositoryName, threadCount > 1 ? true : false, "/Temp/Test-" + DateTime.Now.Ticks, i, numDocs);
                                    ThreadStart job = new ThreadStart(aTest.Start);
                                    new Thread(job).Start();
                                }
                                Console.WriteLine("\n\n " + numDocs + " documents will be imported and randomly assigned to " + threadCount + " cases, 5 requests for each of "
                                + threadCount + " threads");
                            }
                            else
                            {
                                UseCaseTests aTest = new UseCaseTests(new ReSTController(username, password),
                                        ReSTHomeUri, repositoryName, threadCount > 1 ? true : false, "/Temp/Test-" + DateTime.Now.Ticks, 1, numDocs);
                                aTest.Start();
                            }
                            break;

                        case "getmimetype":
                            Console.WriteLine("Mime-Type for " + cmd + " is:"+ObjectUtil.getMimeTypeFromFileName(cmd));
                            break;
                        case "getformat":
                            Console.WriteLine("Format for " + cmd + " is:" + ObjectUtil.getDocumentumFormatForFile(cmd));
                            break;
                        case "cls":
                            Console.Clear();
                            break;
                        case "dupe":
                            //Object.DuplicateQuery(client, ReSTHomeUri, repositoryName, printResult);
                            Console.WriteLine("Unimplemented");
                            break;
                        case "batch":
                            Batch batch = new Batch();
                            batch.Description = "test";
                            batch.Transactional = false;
                            batch.Sequential = false;
                            batch.OnError = "CONTINUE";
                            batch.ReturnRequest = true;
                            Operation op = batch.GetNewOperation();
                            op.addHeader("HeaderName", "HeaderValue");
                            op.Id = "1";
                            op.URI = "http://something.here.com/blah";
                            List<Operation> ops = new List<Operation>();
                            ops.Add(op);
                            batch.Operations = ops;

                            JsonDotnetJsonSerializer ser = new JsonDotnetJsonSerializer();
                            Console.WriteLine(ser.Serialize(batch));
                            DefaultDataContractJsonSerializer dser = new DefaultDataContractJsonSerializer();
                            Console.WriteLine(dser.Serialize(batch));
                            break;
                        case "timezone":
                            Search options = new Search();
                            options.TimeZone = cmd;
                            break; 
                        default:
                            Console.WriteLine("Nothing entered");
                            break;

                    }
                }
                if(!test.Equals("cls")) 
                {
                    Console.ReadKey();
                }
                line = PrintMenu();
            }



        }

        private static int getInputNumber(string title, int value)
        {
            
            string input = "";
            try
            {
                Console.WriteLine("\t" + title + " ["+value+"]");
                Console.WriteLine();
                input = getLineOfInput();
                if (input != null && !input.Trim().Equals(""))
                {
                    value = int.Parse(input);
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Input: " + input + " cannot be parsed to an integer, using default value of " + value, e);
            }
            return value;
        }

        public static String PrintMenu()
        {

            Console.WriteLine("Enter the Test Name, then the parameters");
            Console.WriteLine("\tdql {dqlquery} - Executes a DQL query and prints the results");
            Console.WriteLine("\tcontent - creates content under the home cabinet, then "
                            + "\n\t\tdownloads and deletes");
            Console.WriteLine("\tfolder  - creates a folder and subfolder under the SystemA/Process"
                            + "\n\t\tstructure");
            Console.WriteLine("\tcopymove - create document under the home cabinet, copy "
                            + "\n\t\tit then move it");
            Console.WriteLine("\tdeletedocument {objectid} - delete the object by id");
            Console.WriteLine("\tasnewversion - create document under the home cabinet, check "
                            + "\n\t\tit out then create another document under home cabinet, \n\t\tand save second document as new minor version of first document");
            Console.WriteLine("\tgetproperties - get a document under the home cabinet,  "
                         + "\n\t\tand show its properties");
            Console.WriteLine("\tupdateproperties - get a document under the home cabinet,  "
                          + "\n\t\tand update its properties and show them again");
            Console.WriteLine("\tviewdocument - get a document by qualification,  "
                        + "\n\t\tand open it for view only");
            Console.WriteLine("\tcreatrendition - get a document by qualification,  "
                        + "\n\t\tand create a PDF rendition of it");
            Console.WriteLine("\tviewrendition - get a document rendtion by qualification,  "
                       + "\n\t\tand open it for view only passing the parent_id and full_format values");
            Console.WriteLine("\tgetmimetype {filename} - returns the mimetype of the given fileName");
            Console.WriteLine("\tgetformat {filename} - returns the documentum format name of the given fileName");
            Console.WriteLine("\tviewrendition - get a document rendtion by qualification, and open it"
                       + "\n\t\tfor view only passing the parent_id and full_format values");
            Console.WriteLine("\tdocumenthistory - get audit history of a document by qualification,  "
                      + "\n\t\tpassing the object_id");
            Console.WriteLine("\tcheckinout - create a test document, check it out, created a new \n\t\tversion of it and check that version in, check"
                      + "\n\t\t out the new version then cancle the check out. get version list");
            Console.WriteLine("\timportemail - create a test MSG document, import it into the system then delete it");
            Console.WriteLine("\texportcase - create a zip file by downloaded a case (passing case folder path or case folder ID)");
            Console.WriteLine("\texportdocuments - create a zip file of selected documents by passing their IDs");
            Console.WriteLine("\tfromtemplate - creates a new document from a template");
            Console.WriteLine("***********************************************************************");
            Console.WriteLine(" The above commands are developer-oriented unit tests. The below ");
            Console.WriteLine(" Commands are standalone test commands that are expected to work");
            Console.WriteLine(" on any machine");
            Console.WriteLine("***********************************************************************");
            Console.WriteLine("\tsearch - prompts for search criteria and location then runs the search query");
            Console.WriteLine("\ttest - Runs the end to end tests with optional"
                            + "\n\t\tthreads and number of documents. The old "
                            + "\n\t\tProcessdoc command will do the same test");
            


            //Console.WriteLine("\tcls - Clear the console");
            Console.WriteLine("\texit - Exit the Test");
            Console.Write("\r\n\nCommand > ");

            return getLineOfInput();
        }

        private static String getLineOfInput() {
            String line = Console.ReadLine();
            Console.WriteLine();
            return line;
        }
        private static void SetupTestData(bool useDefaults)
        {
            string defaultReSTHomeUri = @"http://localhost:8080/dctm-rest/services";
            string defaultUsername = "dmadmin";
            string defaultPassword = "password";
            string defaultRepositoryName = "Process";
            string defaultPrintResult = "false";

            var testConfig = Type.GetType("Mono.Runtime") == null ? ConfigurationManager.GetSection("windowsconfig") as NameValueCollection
                    : ConfigurationManager.GetSection("unixconfig") as NameValueCollection;
            if (testConfig != null)
            {
                defaultReSTHomeUri = testConfig["defaultReSTHomeUri"]; 
                defaultUsername = testConfig["defaultUsername"];
                defaultPassword = testConfig["defaultPassword"];
                defaultRepositoryName = testConfig["defaultRepositoryName"];
                defaultPrintResult =testConfig["defaultPrintResult"].ToString();
                useDefaults = Boolean.Parse(testConfig["useDefaults"].ToString());
            }
            
            
            if (useDefaults)
            {
                ReSTHomeUri = defaultReSTHomeUri;
                username = defaultUsername;
                password = defaultPassword;
                repositoryName = defaultRepositoryName;
                printResult = Boolean.Parse(defaultPrintResult);
            }
            else
            {
                readSetupParameters(defaultReSTHomeUri, defaultUsername, defaultPassword, defaultRepositoryName, defaultPrintResult);
            }
            client = new ReSTController(null, null); // new ReSTController(username, password);
            // alternatively, you can choose .net default data contract serializer: new DefaultDataContractJsonSerializer();
            client.JsonSerializer = new JsonDotnetJsonSerializer();
            Console.WriteLine();
        }

        public static void readSetupParameters(string defaultReSTHomeumentUri, string defaultUsername, string defaultPassword,
            string defaultRepositoryName, string defaultPrintResult)
        {
            Console.WriteLine("$$$$ Test for the Documentum REST .NET Client Reference Implementation$$$$\r\n");
            Console.Write("Set the home document URL [" + defaultReSTHomeumentUri + "] :");
            ReSTHomeUri = Console.ReadLine();
            if (String.IsNullOrEmpty(ReSTHomeUri)) ReSTHomeUri = defaultReSTHomeumentUri;

            Console.Write("Set the username [" + defaultUsername + "] :");
            username = Console.ReadLine();
            if (String.IsNullOrEmpty(username)) username = defaultUsername;

            Console.Write("Set the user password [**********] :");
            password = Console.ReadLine();
            if (String.IsNullOrEmpty(password)) password = defaultPassword;

            Console.Write("Set the repository name [" + defaultRepositoryName + "] :");
            repositoryName = Console.ReadLine();
            if (String.IsNullOrEmpty(repositoryName)) repositoryName = defaultRepositoryName;

            Console.Write("Whether to print the result [" + defaultPrintResult + "] :");
            string input = Console.ReadLine();
            printResult = String.IsNullOrEmpty(input) ? Boolean.Parse(defaultPrintResult) : Boolean.Parse(input);
        }
    }
}