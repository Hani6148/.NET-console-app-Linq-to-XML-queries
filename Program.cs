using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
namespace document_8
    public class Program
    {
        // course class
        public class course
        {
            public string courseid;
            public string title;
            public string code;
            public string subject;
            public string location;
            public string instructor;
        }
        // instructor class
        class instructor
        {
            public string name;
            public string email;
            public string phone;
        }
        static void Main(string[] args)
        { // creating a in-memory xml document
            XDocument doc = new XDocument();
            // instructor list
            List<instructor> instructorList = new List<instructor>();
            // paths
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            String cpath = path + "/Courses.csv";
            String ipath = path + "/Instructors.csv";
            //creating a file stream
            FileStream fs = File.Open(path + "/courses.xml", FileMode.Create, FileAccess.Write);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            // memory stream
            MemoryStream ms = new MemoryStream();
            // crearing a writer that will write to the memory stream
            XmlWriter writer = XmlWriter.Create(ms, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("Courses");
            //reading from the course.csv file and writing the lines into xml elements into the memory streams
            char _Delimiter = ','; // .csv comma separate values 
            string line;
            using (var textReader = new StreamReader(cpath))
            { //using System.IO  
                // skipping the first line
                line = textReader.ReadLine();
                line = textReader.ReadLine();
                while ((line = textReader.ReadLine()) != null) {
                    string[] columns = line.Split(_Delimiter);
                    course courseInstance = new course { subject = columns[0].Substring(0, 3), code = columns[0].Substring(4), title = columns[1], courseid = columns[2], instructor = columns[3], location = columns[7] };
                    writer.WriteStartElement("Course");
                    writer.WriteElementString("Subject", courseInstance.subject);
                    writer.WriteElementString("Code", courseInstance.code);
                    writer.WriteElementString("Title", courseInstance.title);
                    writer.WriteElementString("CourseID", courseInstance.courseid);
                    writer.WriteElementString("Instructor", courseInstance.instructor);
                    writer.WriteElementString("Location", courseInstance.location);
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            ms.WriteTo(fs);
            fs.Close();
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            // transforming the memory stream into an xelment
            XElement Courses = XDocument.Load(ms).Root;
            // 1st query
            IEnumerable<XElement> query1 =
                from c in Courses.Elements("Course")
                where (int)c.Element("Code") >= 200
                orderby (string)c.Element("Instructor")
                select new XElement("Course", c.Element("Code"),c.Element("Title"), c.Element("Instructor"));
            Console.WriteLine("Reatrieving IEE courses with codes >=300 Please press any key \n ");
            Console.ReadKey();
            foreach (XElement c in query1)
            {
                Console.WriteLine(c);
            }
            //2nd query
            var query2 = 
                from c in Courses.Elements("Course")
                group c by c.Element("Subject").Value into subjectGroup 
                select new { key = subjectGroup.Key, count = subjectGroup.Count(), codeGroupe = from c in subjectGroup group c by c.Element("Code").Value into codeGroupe select codeGroupe };
            
            Console.WriteLine("\n Retrieve and deliver courses in groups. Please press any key \n ");
            Console.ReadKey();
            foreach (var c in query2)
            {
                Console.WriteLine(c.key);
                foreach (var cg in c.codeGroupe)
                {
                    if (cg.Count() >= 2)
                    {
                        Console.WriteLine("----" + cg.Key);
                        foreach (var crs in cg)
                        {
                            Console.WriteLine("-----------" + crs.Element("Title").Value);
                        }
                    }
                }
            }
            // reading the inscrtuctor.csv and creating the saving the data into the instructorList
            using (var textReader = new StreamReader(ipath))
            { //using System.IO  
                string line2 = textReader.ReadLine();
                line = textReader.ReadLine();
                while (line2 != null)
                {
                    string[] columns = line2.Split(_Delimiter);
                    line2 = textReader.ReadLine();
                    instructorList.Add(new instructor { name = columns[0], email = columns[1], phone = columns[2] });
                }
                Console.WriteLine("-------------------------------------------------------" + "\n");
            }
            // loading the courses.xml into XElemnt 
            XElement myCourses = XElement.Load(path + "/courses.xml");
            //3rd query
            IEnumerable<XElement> query3 =
                 from c in myCourses.Elements("Course") 
                 join ins in instructorList
                 on (string)c.Element("Instructor") equals ins.name
                 orderby (int)c.Element("Code")
                 select new XElement("Course", c.Element("Subject"), c.Element("Code"), new XElement("Email",ins.email) );
            Console.WriteLine("query to find the course  and instructor’s email address for each course with code range in the 200s .Please enter any key \n");
            Console.ReadKey();
            foreach (XElement course in query3)
            {
                if (Int32.Parse(course.Element("Code").Value) >= 200 && Int32.Parse(course.Element("Code").Value) <= 300)
                    Console.WriteLine(course);
            }
            
        }
    }
