using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using Utilities;

namespace CreateDomain
{
    class Program
    {

        enum FilenameOptions { OpenDomain, SaveDomain };
        [STAThread]
        static void Main(string[] args)
        {
            //System.Threading.Thread thisThread = System.Threading.Thread.CurrentThread;
            
            //thisThread.SetApartmentState(System.Threading.ApartmentState.STA);
            Console.WriteLine("Welcome to the Domain Editor");
            Console.WriteLine();

            bool exit = false;

            while (!exit)
            {
                Console.WriteLine("Would you like to Create or Edit a domain? (c|e|exit): ");
                string input = Console.ReadLine();

                switch (input.ToLower())
                {
                    case ("exit"):
                        exit = true;
                        break;
                    case ("c"):
                        CreateDomain();
                        break;
                    case ("e"):
                        EditDomain();
                        break;
                    default:
                        Console.WriteLine("Invalid entry, try again: ");
                        break;
                }
            }
        }

        static void CreateDomain()
        {
            Domain domain = new Domain();

            domain.Name = GetDomainName();

            int numClasses = GetNumClasses();

            // Get the classes
            for (int i = 1; i <= numClasses; i++)
                CreateClass(ref domain);

            WriteDomainToConsole(domain);

            Console.Write("Is this correct? (y|n): ");
            string input = Console.ReadLine();
            bool accepted = false;
            while (!accepted)
            {
                switch (input.ToLower())
                {
                    case ("n"):
                        EditDomain();
                        accepted = true;
                        break;
                    case ("y"):
                        SaveDomain(domain);
                        accepted = true;
                        break;
                    default:
                        Console.WriteLine("Invalid entry, try again: ");
                        break;
                }
            }
        }

        static void EditDomain()
        {
            string filename = GetFilename(FilenameOptions.OpenDomain);

            Domain domain = LoadDomain(filename);
            if (domain == null)
                return;

            WriteDomainToConsole(domain);

            bool finished = false;
            while (!finished)
            {
                Console.WriteLine("Which field would you like to change?");
                Console.WriteLine("(Name, Classes, Finished)");
                string input = Console.ReadLine();

                switch (input.ToLower())
                {
                    case ("name"):
                        EditDomainName(ref domain);
                        break;
                    case ("classes"):
                        EditClasses(ref domain);
                        break;
                    case ("finished"):
                        finished = true;
                        break;
                    default:
                        Console.WriteLine("Unable to understand input, try again: ");
                        break;
                }
            }

            SaveDomain(domain);
        }

        static void EditDomainName(ref Domain domain)
        {
            Console.WriteLine("Current Domain Name = " + domain.Name);
            Console.Write("Change Domain Name to: ");
            domain.Name = Console.ReadLine();
        }

        static void EditClasses(ref Domain domain)
        {
            Console.Write("Current Classes: ");
            foreach (string className in domain.Classes)
                Console.Write(className + ", ");
            Console.WriteLine();

            bool accepted = false;
            while (!accepted)
            {
                Console.WriteLine("What action would you like to take?");
                Console.WriteLine("Create New Class (c), Delete Class (d), Change Class Order (o), Modify Class (m), Go Back (b)");

                string input = Console.ReadLine();
                switch (input.ToLower())
                {
                    case ("c"):
                        CreateClass(ref domain);
                        accepted = true;
                        break;
                    case ("d"):
                        DeleteClass(ref domain);
                        accepted = true;
                        break;
                    case ("m"):
                        ModifyClass(ref domain);
                        accepted = true;
                        break;
                    case ("b"):
                        accepted = true;
                        break;
                    case ("o"):
                        ChangeClassOrder(ref domain);
                        accepted = true;
                        break;
                    default:
                        Console.WriteLine("Invalid entry, try again: ");
                        break;
                }
            }
        }

        static void ChangeClassOrder(ref Domain domain)
        {
            Console.WriteLine("Current Class Order:");
            for (int i = 0; i < domain.Classes.Count; i++)
                Console.WriteLine("{0}: {1}", i, domain.Classes[i]);
            Console.WriteLine();

            List<string> order = new List<string>();
            for (int i = 0; i < domain.Classes.Count; i++)
            {
                bool accepted = false;
                while (!accepted)
                {
                    Console.Write("Which class would you like to be #{0}?", i);
                    string name = Console.ReadLine();
                    Console.WriteLine();
                    if (domain.Classes.Contains(name))
                    {
                        order.Add(name);
                        accepted = true;
                    }
                    else
                        Console.Write("Invalid entry, try again:  ");
                }
            }

            foreach (string name in order)
                domain.Classes.Remove(name);

            foreach (string name in order)
                domain.Classes.Add(name);
        }

        static void ModifyClass(ref Domain domain)
        {
            while (true)
            {
                Console.Write("Current Classes: ");
                foreach (string name in domain.Classes)
                {
                    Console.Write(name + ", ");
                }
                Console.WriteLine("What class would you like to modify (case sensitive)? (or 'exit')");

                string className = "";
                while (true)
                {
                    className = Console.ReadLine();
                    if (className.ToLower() == "exit")
                        return;

                    if (domain.Classes.Contains(className))
                        break;
                    else
                        Console.WriteLine("The domain does not have the specified class, try again");
                }

                bool accepted = false;
                while (!accepted)
                {
                    Console.WriteLine("What would you like to do to class '" + className + "'?");
                    Console.Write("Change Color (c), Change Name (n), Modify Shapes (s), Go Back (b):");
                    string input2 = Console.ReadLine();
                    switch (input2.ToLower())
                    {
                        case ("c"):
                            Color color = GetClassColor(domain, className);
                            if (domain.Class2Color.ContainsKey(className))
                                domain.Class2Color[className] = color;
                            accepted = true;
                            break;
                        case ("n"):
                            ChangeClassName(ref domain, className);
                            accepted = true;
                            break;
                        case ("s"):
                            ModifyShapes(ref domain, className);
                            accepted = true;
                            break;
                        case ("b"):
                            accepted = true;
                            break;
                        default:
                            Console.WriteLine("Unable to understand your selection, try again: ");
                            break;
                    }
                }
            }
        }

        static void ChangeClassName(ref Domain domain, string className)
        {
            bool accepted = false;
            while (!accepted)
            {
                Console.Write("New name for class " + className + ": ");
                string newName = Console.ReadLine();
                if (domain.Classes.Contains(newName))
                {
                    domain.Classes.Remove(className);
                    domain.Classes.Add(newName);
                }

                if (domain.Class2Color.ContainsKey(className))
                {
                    Color c = domain.Class2Color[className];
                    domain.Class2Color.Remove(className);
                    if (domain.Class2Color.ContainsKey(newName))
                        domain.Class2Color.Add(newName, c);
                }

                if (domain.Class2Shapes.ContainsKey(className))
                {
                    List<string> shapes = domain.Class2Shapes[className];
                    domain.Class2Shapes.Remove(className);
                    domain.Class2Shapes.Add(newName, shapes);
                }
            }
        }

        static void ModifyShapes(ref Domain domain, string className)
        {
            while (true)
            {
                Console.Write("Current Shapes: ");
                foreach (string name in domain.Class2Shapes[className])
                {
                    Console.Write(name + ", ");
                }
                Console.WriteLine("What shape would you like to modify (case sensitive)? (or 'exit')");

                string shapeName = "";
                while (true)
                {
                    shapeName = Console.ReadLine();
                    if (shapeName.ToLower() == "exit")
                        return;

                    if (domain.Shapes.Contains(shapeName) && domain.Class2Shapes[className].Contains(shapeName))
                        break;
                    else
                        Console.WriteLine("The domain does not have the specified class, try again");
                }

                bool accepted = false;
                while (!accepted)
                {
                    Console.WriteLine("What would you like to do to class '" + shapeName + "'?");
                    Console.Write("Change Color (c), Change Name (n), Modify Shapes (s), Go Back (b):");
                    string input2 = Console.ReadLine();
                    switch (input2.ToLower())
                    {
                        case ("c"):
                            Color color = GetShapeColor(domain, shapeName);
                            if (domain.Shape2Color.ContainsKey(shapeName))
                                domain.Shape2Color[shapeName] = color;
                            accepted = true;
                            break;
                        case ("n"):
                            ChangeShapeName(ref domain, shapeName);
                            accepted = true;
                            break;
                        case ("b"):
                            accepted = true;
                            break;
                        default:
                            Console.WriteLine("Unable to understand your selection, try again: ");
                            break;
                    }
                }
            }
        }

        static void ChangeShapeName(ref Domain domain, string shapeName)
        {
            bool accepted = false;
            while (!accepted)
            {
                Console.Write("New name for shape " + shapeName + ": ");
                string newName = Console.ReadLine();
                if (domain.Shapes.Contains(newName))
                {
                    domain.Shapes.Remove(shapeName);
                    domain.Shapes.Add(newName);
                }

                if (domain.Shape2Color.ContainsKey(shapeName))
                {
                    Color c = domain.Shape2Color[shapeName];
                    domain.Shape2Color.Remove(shapeName);
                    if (domain.Shape2Color.ContainsKey(newName))
                        domain.Shape2Color.Add(newName, c);
                }

                if (domain.Class2Shapes.ContainsKey(shapeName))
                {
                    string className = domain.Shape2Class[shapeName];
                    domain.Class2Shapes[className].Remove(shapeName);
                    domain.Class2Shapes[className].Add(newName);
                }
            }
        }

        static void DeleteClass(ref Domain domain)
        {
            bool accepted = false;
            while (!accepted)
            {
                Console.Write("Current Classes: ");
                foreach (string className in domain.Classes)
                    Console.Write(className + ", ");
                Console.WriteLine("What class would you like to delete (case sensitive)? (or 'exit')");

                string input = Console.ReadLine();
                if (input.ToLower() == "exit")
                {
                    accepted = true;
                    return;
                }
                else
                {
                    bool deleted = domain.DeleteClass(input);
                    if (!deleted)
                        Console.WriteLine("Unable to delete class, try again (or 'exit')");
                }
            }
        }

        static void WriteDomainToConsole(Domain domain)
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Domain: ");
            Console.WriteLine(" Name: " + domain.Name);
            Console.WriteLine(" Classes: ");
            foreach (string cls in domain.Classes)
            {
                Console.WriteLine("  " + cls + ": Color = " + domain.Class2Color[cls].ToString() + ", Shapes:");
                foreach (string shape in domain.Class2Shapes[cls])
                {
                    Console.WriteLine("   " + shape + ": Color = " + domain.Shape2Color[shape].ToString());
                }
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        static void SaveDomain(Domain domain)
        {
            bool accepted = false;
            while (!accepted)
            {
                string filename = GetFilename(FilenameOptions.SaveDomain);

                try
                {
                    Stream stream = File.Open(filename, FileMode.Create);
                    BinaryFormatter bformatter = new BinaryFormatter();

                    Console.WriteLine("Writing Domain Information");
                    bformatter.Serialize(stream, domain);
                    stream.Close();
                    accepted = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Unable to save domain, try again.");
                }
            }
        }

        static Domain LoadDomain(string filename)
        {
            Domain domain = null;
            bool accepted = false;
            while (!accepted)
            {
                try
                {
                    Stream stream = File.Open(filename, FileMode.Open);
                    BinaryFormatter bformatter = new BinaryFormatter();

                    Console.WriteLine("Reading Domain Information");
                    domain = (Domain)bformatter.Deserialize(stream);
                    stream.Close();
                    accepted = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Unable to open domain.");
                    break;
                }
            }

            return domain;
        }

        static string GetFilename(FilenameOptions option)
        {
            string filename = "";

            if (option == FilenameOptions.SaveDomain)
            {
                SaveFileDialog saveDlg = new SaveFileDialog();
                saveDlg.Filter = "Domain Files (*.dom)|*.dom";
                saveDlg.Title = "Select file to save domain to...";

                if (saveDlg.ShowDialog() == DialogResult.OK)
                {
                    filename = saveDlg.FileName;
                }
            }
            else if (option == FilenameOptions.OpenDomain)
            {
                OpenFileDialog openDlg = new OpenFileDialog();
                openDlg.Title = "Select Domain file to open...";
                openDlg.Filter = "Domain Files (*.dom)|*.dom";

                if (openDlg.ShowDialog() == DialogResult.OK)
                {
                    filename = openDlg.FileName;
                }
            }

            return filename;
        }

        static string GetDomainName()
        {
            // Get the domain name
            Console.Write("Domain Name: ");

            return Console.ReadLine();
        }

        static int GetNumClasses()
        {
            // Get the number of classes
            Console.Write("Number of Classes: ");
            int numClasses = 0;
            bool accepted = false;
            while (!accepted)
            {
                string input = Console.ReadLine();
                try
                {
                    numClasses = Convert.ToInt32(input);
                    accepted = true;
                }
                catch
                {
                    Console.WriteLine();
                    Console.Write("Unable to convert input to an integer, try again: ");
                }
            }

            return numClasses;
        }

        static void CreateClass(ref Domain domain)
        {
            string className = GetClassName(domain);

            Color color = GetClassColor(domain, className);

            domain.AddClass(className, color);

            bool finishedClass = false;
            while (!finishedClass)
            {
                string shapeName = GetShapeName(domain, className, ref finishedClass);
                if (shapeName == "..")
                    break;

                Color shapeColor = GetShapeColor(domain, shapeName);

                domain.AddShape(shapeName, shapeColor, className);
            }
        }

        static string GetClassName(Domain domain)
        {
            Console.WriteLine();
            Console.Write("Name of Class: ");

            string className = "None";
            bool accepted = false;
            while (!accepted)
            {
                className = Console.ReadLine();
                if (domain.Classes.Contains(className))
                {
                    Console.Write("Class already exists, try again: ");
                }
                else
                    accepted = true;
            }

            return className;
        }

        static Color GetClassColor(Domain domain, string className)
        {
            Console.WriteLine();
            Console.Write("Color for class \"" + className
              + "\" (Red, Orange, Blue, Green, Magenta, Cyan, Brown, Pink, "
              + "Gold, Lime, Purple, Turquoise, or Olive): ");
            Color color = Color.Black;
            bool accepted = false;
            while (!accepted)
            {
                string input = Console.ReadLine();
                try
                {
                    color = GetColor(input);
                    if (color != Color.Black)
                    {
                        if (!domain.ClassColors.Contains(color))
                            accepted = true;
                        else
                        {
                            Console.WriteLine();
                            Console.Write("Class \"" + domain.Color2Class(color)
                                + "\" already has the Color \"" + color.ToString() + "\", try again: ");
                        }
                    }
                }
                catch
                {
                    Console.WriteLine();
                    Console.Write("Unable to recognize color, try again: ");
                }
            }

            return color;
        }

        static string GetShapeName(Domain domain, string className, ref bool finishedClass)
        {
            Console.WriteLine();
            Console.Write("Add Shape to Class \"" + className + "\" (.. to finish class): ");

            string shapeName = Console.ReadLine();

            if (shapeName == "..")
                finishedClass = true;

            return shapeName;
            
        }

        static Color GetShapeColor(Domain domain, string shapeName)
        {
            Console.Write("Color for shape \"" + shapeName
                      + "\" (Red, Orange, Blue, Green, Magenta, Cyan, Brown, Pink, "
                      + "Gold, Lime, Purple, Turquoise, or Olive): ");
            Color color = Color.Black;
            bool accepted = false;
            while (!accepted)
            {
                string input = Console.ReadLine();
                try
                {
                    color = GetColor(input);
                    if (color != Color.Black)
                    {
                        if (!domain.ShapeColors.Contains(color))
                            accepted = true;
                        else
                        {
                            Console.WriteLine("Class \"" + domain.Color2Shape(color)
                                + "\" already has Color \"" + color.ToString() + "\", try again: ");
                        }
                    }
                }
                catch
                {
                    Console.WriteLine();
                    Console.Write("Unable to recognize color, try again: ");
                }
            }

            return color;
        }    

        static Color GetColor(string input)
        {
            Color color = Color.Black;
            switch (input.ToLower())
            {
                #region Cases
                case ("red"):
                    color = Color.Red;
                    break;
                case ("orange"):
                    color = Color.Orange;
                    break;
                case ("blue"):
                    color = Color.Blue;
                    break;
                case ("green"):
                    color = Color.Green;
                    break;
                case ("magenta"):
                    color = Color.Magenta;
                    break;
                case ("cyan"):
                    color = Color.Cyan;
                    break;
                case ("brown"):
                    color = Color.Brown;
                    break;
                case ("pink"):
                    color = Color.Pink;
                    break;
                case ("gold"):
                    color = Color.Gold;
                    break;
                case ("lime"):
                    color = Color.Lime;
                    break;
                case ("purple"):
                    color = Color.Purple;
                    break;
                case ("turquoise"):
                    color = Color.Turquoise;
                    break;
                case ("olive"):
                    color = Color.Olive;
                    break;
                case ("aqua"):
                    color = Color.Aqua;
                    break;
                case ("darkcyan"):
                    color = Color.DarkCyan;
                    break;
                case ("darkolivegreen"):
                    color = Color.DarkOliveGreen;
                    break;
                case ("navy"):
                    color = Color.Navy;
                    break;
                case ("darkgreen"):
                    color = Color.DarkGreen;
                    break;
                case ("darkred"):
                    color = Color.DarkRed;
                    break;
                case ("violet"):
                    color = Color.Violet;
                    break;
                case ("coral"):
                    color = Color.Coral;
                    break;
                case ("gray"):
                    color = Color.Gray;
                    break;
                case ("greenyellow"):
                    color = Color.GreenYellow;
                    break;
                case ("hotpink"):
                    color = Color.HotPink;
                    break;
                case ("darkblue"):
                    color = Color.DarkBlue;
                    break;
                case ("maroon"):
                    color = Color.Maroon;
                    break;
                case ("lawngreen"):
                    color = Color.LawnGreen;
                    break;
                case ("lightblue"):
                    color = Color.LightBlue;
                    break;
                case ("olivedrab"):
                    color = Color.OliveDrab;
                    break;
                case ("plum"):
                    color = Color.Plum;
                    break;
                case ("tan"):
                    color = Color.Tan;
                    break;
                case ("teal"):
                    color = Color.Teal;
                    break;
                case ("chartreuse"):
                    color = Color.Chartreuse;
                    break;
                case ("cornsilk"):
                    color = Color.Cornsilk;
                    break;
                case ("lavender"):
                    color = Color.Lavender;
                    break;
                case ("lightgreen"):
                    color = Color.LightGreen;
                    break;
                case ("orangered"):
                    color = Color.OrangeRed;
                    break;
                case ("dodgerblue"):
                    color = Color.DodgerBlue;
                    break;
                default:
                    Console.WriteLine(input + " is an Invalid Color, try again:  ");
                    break;

                #endregion
            }

            return color;
        }
    }
}
