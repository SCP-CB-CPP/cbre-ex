using CBRE.Common.Extensions;
using CBRE.DataStructures.Geometric;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CBRE.Providers
{
    /// <summary>
    /// Holds values parsed from a generic Valve structure. It holds a single entity:
    /// entity_name
    /// {
    /// 	"property" "value"
    /// 	subentity_name
    /// 	{
    /// 		"property" "value
    /// 		subentity_name
    /// 		{
    /// 			...
    /// 		}
    /// 	}
    /// }
    /// </summary>
    public class GenericStructure
    {
        private class GenericStructureProperty
        {
            public string Key { get; set; }
            public string Value { get; set; }

            public GenericStructureProperty(string key, string value)
            {
                Key = key;
                Value = value;
            }
        }

        public string Name { get; private set; }
        private List<GenericStructureProperty> Properties { get; set; }
        public List<GenericStructure> Children { get; private set; }

        public string this[string key]
        {
            get
            {
                GenericStructureProperty prop = Properties.FirstOrDefault(x => x.Key == key);
                return prop == null ? null : prop.Value;
            }
            set
            {
                GenericStructureProperty prop = Properties.FirstOrDefault(x => x.Key == key);
                if (prop != null) prop.Value = value;
                else Properties.Add(new GenericStructureProperty(key, value));
            }
        }

        public void AddProperty(string key, string value)
        {
            Properties.Add(new GenericStructureProperty(key, value));
        }

        public void RemoveProperty(string key)
        {
            Properties.RemoveAll(x => x.Key == key);
        }

        public GenericStructure(string name)
        {
            Name = name;
            Properties = new List<GenericStructureProperty>();
            Children = new List<GenericStructure>();
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            return Properties.Select(x => x.Key).Distinct();
        }

        public IEnumerable<string> GetAllPropertyValues(string key)
        {
            return Properties.Where(x => x.Key == key).Select(x => x.Value);
        }

        public string GetPropertyValue(string name, bool ignoreCase)
        {
            GenericStructureProperty prop = Properties.FirstOrDefault(x => String.Equals(x.Key, name, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture));
            return prop == null ? null : prop.Value;
        }

        public bool PropertyBoolean(string name, bool defaultValue = false)
        {
            string prop = this[name];
            if (prop == "1") return true;
            if (prop == "0") return false;
            bool d;
            if (bool.TryParse(prop, out d))
            {
                return d;
            }
            return defaultValue;
        }

        public T PropertyEnum<T>(string name, T defaultValue = default(T)) where T : struct
        {
            string prop = this[name];
            T val;
            return Enum.TryParse(prop, true, out val) ? val : defaultValue;
        }

        public int PropertyInteger(string name, int defaultValue = 0)
        {
            string prop = this[name];
            int d;
            if (int.TryParse(prop, out d))
            {
                return d;
            }
            return defaultValue;
        }

        public long PropertyLong(string name, long defaultValue = 0)
        {
            string prop = this[name];
            long d;
            if (long.TryParse(prop, out d))
            {
                return d;
            }
            return defaultValue;
        }

        public decimal PropertyDecimal(string name, decimal defaultValue = 0)
        {
            string prop = this[name];
            decimal d;
            if (decimal.TryParse(prop, out d))
            {
                return d;
            }
            return defaultValue;
        }

        public decimal[] PropertyDecimalArray(string name, int count)
        {
            string prop = this[name];
            decimal[] defaultValue = Enumerable.Range(0, count).Select(i => 0m).ToArray();
            if (prop == null || prop.Count(c => c == ' ') != (count - 1)) return defaultValue;
            string[] split = prop.Split(' ');
            for (int i = 0; i < count; i++)
            {
                decimal d;
                if (decimal.TryParse(split[i], out d))
                {
                    defaultValue[i] = d;
                }
            }
            return defaultValue;
        }

        public Plane PropertyPlane(string name)
        {
            string prop = this[name];
            Plane defaultValue = new Plane(Coordinate.UnitZ, 0);
            if (prop == null || prop.Count(c => c == ' ') != 8) return defaultValue;
            string[] split = prop.Replace("(", "").Replace(")", "").Split(' ');
            decimal x1, x2, x3, y1, y2, y3, z1, z2, z3;
            if (decimal.TryParse(split[0], out x1)
                && decimal.TryParse(split[1], out y1)
                && decimal.TryParse(split[2], out z1)
                && decimal.TryParse(split[3], out x2)
                && decimal.TryParse(split[4], out y2)
                && decimal.TryParse(split[5], out z2)
                && decimal.TryParse(split[6], out x3)
                && decimal.TryParse(split[7], out y3)
                && decimal.TryParse(split[8], out z3))
            {
                return new Plane(
                    new Coordinate(x1, y1, z1).Round(),
                    new Coordinate(x2, y2, z2).Round(),
                    new Coordinate(x3, y3, z3).Round());
            }
            return defaultValue;
        }

        public Coordinate PropertyCoordinate(string name, Coordinate defaultValue = null)
        {
            string prop = this[name];
            if (defaultValue == null) defaultValue = Coordinate.Zero;
            if (prop == null || prop.Count(c => c == ' ') != 2) return defaultValue;
            string[] split = prop.Replace("[", "").Replace("]", "").Replace("(", "").Replace(")", "").Split(' ');
            decimal x, y, z;
            if (decimal.TryParse(split[0], out x)
                && decimal.TryParse(split[1], out y)
                && decimal.TryParse(split[2], out z))
            {
                return new Coordinate(x, y, z);
            }
            return defaultValue;
        }

        public Coordinate[] PropertyCoordinateArray(string name, int count)
        {
            string prop = this[name];
            Coordinate[] defaultValue = Enumerable.Range(0, count).Select(i => Coordinate.Zero).ToArray();
            if (prop == null || prop.Count(c => c == ' ') != (count * 3 - 1)) return defaultValue;
            string[] split = prop.Split(' ');
            for (int i = 0; i < count; i++)
            {
                decimal x, y, z;
                if (decimal.TryParse(split[i * 3], out x)
                    && decimal.TryParse(split[i * 3 + 1], out y)
                    && decimal.TryParse(split[i * 3 + 2], out z))
                {
                    defaultValue[i] = new Coordinate(x, y, z);
                }
            }
            return defaultValue;
        }

        public Tuple<Coordinate, decimal, decimal> PropertyTextureAxis(string name)
        {
            string prop = this[name];
            Tuple<Coordinate, decimal, decimal> defaultValue = Tuple.Create(Coordinate.UnitX, 0m, 1m);
            if (prop == null || prop.Count(c => c == ' ') != 4) return defaultValue;
            string[] split = prop.Replace("[", "").Replace("]", "").Split(' ');
            decimal x, y, z, sh, sc;
            if (decimal.TryParse(split[0], out x)
                && decimal.TryParse(split[1], out y)
                && decimal.TryParse(split[2], out z)
                && decimal.TryParse(split[3], out sh)
                && decimal.TryParse(split[4], out sc))
            {
                return Tuple.Create(new Coordinate(x, y, z), sh, sc);
            }
            return defaultValue;
        }

        public Color PropertyColour(string name, Color defaultValue)
        {
            string prop = this[name];
            if (prop == null || prop.Count(x => x == ' ') != 2) return defaultValue;
            string[] split = prop.Split(' ');
            int r, g, b;
            if (int.TryParse(split[0], out r)
                && int.TryParse(split[1], out g)
                && int.TryParse(split[2], out b))
            {
                return Color.FromArgb(r, g, b);
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets the immediate children of this structure
        /// </summary>
        /// <param name="name">Optional name filter</param>
        /// <returns>A list of children</returns>
        public IEnumerable<GenericStructure> GetChildren(string name = null)
        {
            return Children.Where(x => name == null || String.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Gets all descendants of this structure recursively
        /// </summary>
        /// <param name="name">Optional name filter</param>
        /// <returns>A list of descendants</returns>
        public IEnumerable<GenericStructure> GetDescendants(string name = null)
        {
            return Children.Where(x => name == null || String.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase))
                .Union(Children.SelectMany(x => x.GetDescendants(name)));
        }

        #region Serialise / Deserialise

        public static GenericStructure Serialise(object obj)
        {
            return SerialiseHelper(obj, new List<object>());
        }

        private static GenericStructure SerialiseHelper(object obj, List<object> encounteredObjects)
        {
            // Handle null
            if (Equals(obj, null)) return new GenericStructure("Serialise.Null") { Properties = { new GenericStructureProperty("Serialise.Null.Value", "null") } };

            if (encounteredObjects.Contains(obj))
            {
                GenericStructure rf = new GenericStructure("Serialise.Reference");
                rf.AddProperty("Serialise.Reference.Index", (encounteredObjects.IndexOf(obj) + 1).ToString());
                return rf;
            }

            Type ty = obj.GetType();

            // Handle primitive types
            if (ty.IsPrimitive || ty == typeof(string) || ty == typeof(decimal))
            {
                string name = "Primitives.";
                if (ty == typeof(bool)) name += "Boolean";
                else if (ty == typeof(char) || ty == typeof(string)) name += "String";
                else name += "Numeric";
                return new GenericStructure(name) { Properties = { new GenericStructureProperty("Primitive.Value", Convert.ToString(obj)) } };
            }

            if (ty == typeof(DateTime))
            {
                return new GenericStructure("Primitives.DateTime") { Properties = { new GenericStructureProperty("Primitive.Value", ((DateTime)obj).ToString("u")) } };
            }

            if (ty == typeof(Color))
            {
                Color color = (Color)obj;
                string col = String.Format("{0} {1} {2} {3}", color.R, color.G, color.B, color.A);
                return new GenericStructure("Primitives.Colour") { Properties = { new GenericStructureProperty("Primitive.Value", col) } };
            }

            if (ty == typeof(Coordinate))
            {
                return new GenericStructure("Primitives.Coordinate") { Properties = { new GenericStructureProperty("Primitive.Value", obj.ToString()) } };
            }

            if (ty == typeof(Box))
            {
                Box b = (Box)obj;
                return new GenericStructure("Primitives.Box") { Properties = { new GenericStructureProperty("Primitive.Value", b.Start + " " + b.End) } };
            }

            if (ty == typeof(Rectangle))
            {
                Rectangle r = (Rectangle)obj;
                return new GenericStructure("Primitives.Rectangle") { Properties = { new GenericStructureProperty("Primitive.Value", r.X + " " + r.Y + " " + r.Width + " " + r.Height) } };
            }

            if (ty == typeof(Plane))
            {
                Plane p = (Plane)obj;
                return new GenericStructure("Primitives.Plane") { Properties = { new GenericStructureProperty("Primitive.Value", p.Normal + " " + p.DistanceFromOrigin) } };
            }

            encounteredObjects.Add(obj);
            int index = encounteredObjects.Count;

            // Handle list
            IEnumerable enumerable = obj as IEnumerable;
            if (enumerable != null)
            {
                IEnumerable<GenericStructure> children = enumerable.OfType<object>().Select(x => SerialiseHelper(x, encounteredObjects));
                GenericStructure list = new GenericStructure("Serialise.List");
                list.AddProperty("Serialise.Reference", index.ToString());
                list.Children.AddRange(children);
                return list;
            }

            // Handle complex types
            GenericStructure gs = new GenericStructure(ty.FullName);
            gs.AddProperty("Serialise.Reference", index.ToString());
            foreach (PropertyInfo pi in ty.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!pi.CanRead) continue;
                object val = pi.GetValue(obj, null);
                GenericStructure pv = SerialiseHelper(val, encounteredObjects);
                if (pv.Name.StartsWith("Primitives."))
                {
                    gs.AddProperty(pi.Name, pv["Primitive.Value"]);
                }
                else
                {
                    pv.Name = pi.Name;
                    gs.Children.Add(pv);
                }
            }
            return gs;
        }

        public static T Deserialise<T>(GenericStructure structure)
        {
            object obj = DeserialiseHelper(typeof(T), structure, new Dictionary<int, object>());
            if (obj is T) return (T)obj;
            obj = Convert.ChangeType(obj, typeof(T));
            if (obj is T) return (T)obj;
            return default(T);
        }

        private static object DeserialiseHelper(Type bindingType, GenericStructure structure, Dictionary<int, object> encounteredObjects)
        {
            // Null values
            if (structure.Name == "Serialise.Null" || structure["Serialise.Null.Value"] == "null")
            {
                return bindingType.IsValueType ? Activator.CreateInstance(bindingType) : null;
            }

            // Referenced values
            GenericStructureProperty indexProp = structure.Properties.FirstOrDefault(x => x.Key == "Serialise.Reference.Index");
            if (indexProp != null) return encounteredObjects[int.Parse(indexProp.Value)];

            // Primitive objects
            if (structure.Name.StartsWith("Primitives.")) return ConvertPrimitive(structure);

            //var instance = Activator.CreateInstance(bindingType);
            GenericStructureProperty refProp = structure.Properties.FirstOrDefault(x => x.Key == "Serialise.Reference");
            int refVal = refProp != null ? int.Parse(refProp.Value) : -1;

            // List objects
            if (structure.Name == "Serialise.List" || typeof(IEnumerable).IsAssignableFrom(bindingType))
            {
                object list = Activator.CreateInstance(bindingType);
                if (refVal >= 0) encounteredObjects[refVal] = list;
                DeserialiseList(list, bindingType, structure, encounteredObjects);
                return list;
            }

            // Complex types
            ConstructorInfo ctor = bindingType.GetConstructor(Type.EmptyTypes) ?? bindingType.GetConstructors().First();
            object[] args = ctor.GetParameters().Select(x => x.ParameterType.IsValueType ? Activator.CreateInstance(x.ParameterType) : null).ToArray();
            object instance = ctor.Invoke(args);

            if (refVal >= 0) encounteredObjects[refVal] = instance;

            foreach (PropertyInfo pi in bindingType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!pi.CanWrite) continue;
                GenericStructureProperty prop = structure.Properties.FirstOrDefault(x => x.Key == pi.Name);
                GenericStructure child = structure.Children.FirstOrDefault(x => x.Name == pi.Name);
                if (prop != null)
                {
                    object prim = ConvertPrimitive(pi.PropertyType, prop.Value);
                    pi.SetValue(instance, Convert.ChangeType(prim, pi.PropertyType), null);
                }
                else if (child != null)
                {
                    object obj = DeserialiseHelper(pi.PropertyType, child, encounteredObjects);
                    pi.SetValue(instance, obj, null);
                }
            }

            return instance;
        }

        private static void DeserialiseList(object instance, Type bindingType, GenericStructure structure, Dictionary<int, object> encounteredObjects)
        {
            Type listType = null;
            if (bindingType.IsGenericType) listType = bindingType.GetGenericArguments()[0];
            List<object> children = structure.Children.Select(x =>
            {
                string name = x.Name;
                Type type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null) ?? (listType ?? typeof(object));
                object result = DeserialiseHelper(type, x, encounteredObjects);
                return Convert.ChangeType(result, type);
            }).ToList();
            if (typeof(IList).IsAssignableFrom(bindingType))
            {
                foreach (object child in children) ((IList)instance).Add(child);
            }
            else if (typeof(Array).IsAssignableFrom(bindingType))
            {
                object[] arr = (object[])instance;
                Array.Resize(ref arr, children.Count);
                children.CopyTo(arr);
            }
        }

        private static object ConvertPrimitive(GenericStructure structure)
        {
            string prim = structure.Name.Substring("Primitives.".Length);
            string value = structure["Primitive.Value"];
            return ConvertPrimitive(prim, value);
        }

        private static object ConvertPrimitive(Type type, string value)
        {
            return ConvertPrimitive(GetPrimitiveName(type), value);
        }

        private static object ConvertPrimitive(string primitiveType, string value)
        {
            string[] spl = value.Split(' ');
            switch (primitiveType)
            {
                case "Boolean":
                    return Boolean.Parse(value);
                case "String":
                    return value;
                case "Numeric":
                    return Decimal.Parse(value, NumberStyles.Float);
                case "DateTime":
                    return DateTime.ParseExact(value, "u", CultureInfo.InvariantCulture);
                case "Colour":
                    return Color.FromArgb(int.Parse(spl[3]), int.Parse(spl[0]), int.Parse(spl[1]), int.Parse(spl[2]));
                case "Coordinate":
                    return Coordinate.Parse(spl[0].TrimStart('('), spl[1], spl[2].TrimEnd(')'));
                case "Box":
                    return new Box(
                        Coordinate.Parse(spl[0].TrimStart('('), spl[1], spl[2].TrimEnd(')')),
                        Coordinate.Parse(spl[3].TrimStart('('), spl[4], spl[5].TrimEnd(')'))
                    );
                case "Plane":
                    return new Plane(Coordinate.Parse(spl[0].TrimStart('('), spl[1], spl[2].TrimEnd(')')), Decimal.Parse(spl[3]));
                case "Rectangle":
                    return new Rectangle(int.Parse(spl[0]), int.Parse(spl[1]), int.Parse(spl[2]), int.Parse(spl[3]));
                default:
                    throw new ArgumentException();
            }
        }

        private static string GetPrimitiveName(Type ty)
        {
            if (ty == typeof(bool)) return "Boolean";
            if (ty == typeof(char) || ty == typeof(string)) return "String";
            if (ty.IsPrimitive || ty == typeof(decimal)) return "Numeric";
            if (ty == typeof(DateTime)) return "DateTime";
            if (ty == typeof(Color)) return "Colour";
            if (ty == typeof(Coordinate)) return "Coordinate";
            if (ty == typeof(Box)) return "Box";
            if (ty == typeof(Plane)) return "Plane";
            if (ty == typeof(Rectangle)) return "Rectangle";
            throw new ArgumentException();
        }

        #endregion

        #region Printer
        public override string ToString()
        {
            StringWriter sw = new StringWriter();
            PrintToStream(sw);
            return sw.ToString();
        }

        public void PrintToStream(TextWriter tw)
        {
            Print(tw);
        }

        private static string LengthLimit(string str, int limit)
        {
            if (str.Length >= limit) return str.Substring(0, limit - 1);
            return str;
        }

        private void Print(TextWriter tw, int tabs = 0)
        {
            string preTabStr = new string(' ', tabs * 4);
            string postTabStr = new string(' ', (tabs + 1) * 4);
            tw.Write(preTabStr);
            tw.WriteLine(Name);
            tw.Write(preTabStr);
            tw.WriteLine("{");
            foreach (GenericStructureProperty kv in Properties)
            {
                tw.Write(postTabStr);
                tw.Write('"');
                tw.Write(LengthLimit(kv.Key, 32768));
                tw.Write('"');
                tw.Write(' ');
                tw.Write('"');
                tw.Write(LengthLimit((kv.Value ?? "").Replace('"', '`'), 32768));
                tw.Write('"');
                tw.WriteLine();
            }
            foreach (GenericStructure child in Children)
            {
                child.Print(tw, tabs + 1);
            }
            tw.Write(preTabStr);
            tw.WriteLine("}");
        }

        #endregion

        #region Parser
        /// <summary>
        /// Parse a structure from a file
        /// </summary>
        /// <param name="filePath">The file to parse from</param>
        /// <returns>The parsed structure</returns>
        public static IEnumerable<GenericStructure> Parse(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                return Parse(reader).ToList();
            }
        }

        /// <summary>
        /// Parse a structure from a stream
        /// </summary>
        /// <param name="reader">The TextReader to parse from</param>
        /// <returns>The parsed structure</returns>
        public static IEnumerable<GenericStructure> Parse(TextReader reader)
        {
            string line;
            while ((line = CleanLine(reader.ReadLine())) != null)
            {
                if (ValidStructStartString(line))
                {
                    yield return ParseStructure(reader, line);
                }
            }
        }

        /// <summary>
        /// Remove comments and excess whitespace from a line
        /// </summary>
        /// <param name="line">The unclean line</param>
        /// <returns>The cleaned line</returns>
        private static string CleanLine(string line)
        {
            if (line == null) return null;
            string ret = line;
            if (ret.Contains("//")) ret = ret.Substring(0, ret.IndexOf("//")); // Comments
            return ret.Trim();
        }

        /// <summary>
        /// Parse a structure, given the name of the structure
        /// </summary>
        /// <param name="reader">The TextReader to read from</param>
        /// <param name="name">The structure's name</param>
        /// <returns>The parsed structure</returns>
        private static GenericStructure ParseStructure(TextReader reader, string name)
        {
            string[] spl = name.SplitWithQuotes();
            GenericStructure gs = new GenericStructure(spl[0]);
            string line;
            if (spl.Length != 2 || spl[1] != "{")
            {
                do
                {
                    line = CleanLine(reader.ReadLine());
                } while (String.IsNullOrWhiteSpace(line));
                if (line != "{")
                {
                    return gs;
                }
            }
            while ((line = CleanLine(reader.ReadLine())) != null)
            {
                if (line == "}") break;

                if (ValidStructPropertyString(line)) ParseProperty(gs, line);
                else if (ValidStructStartString(line)) gs.Children.Add(ParseStructure(reader, line));
            }
            return gs;
        }

        /// <summary>
        /// Check if the given string is a valid structure name
        /// </summary>
        /// <param name="s">The string to test</param>
        /// <returns>True if this is a valid structure name, false otherwise</returns>
        private static bool ValidStructStartString(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            string[] split = s.SplitWithQuotes();
            return split.Length == 1 || (split.Length == 2 && split[1] == "{");
        }

        /// <summary>
        /// Check if the given string is a valid property string in the format: "key" "value"
        /// </summary>
        /// <param name="s">The string to test</param>
        /// <returns>True if this is a valid property string, false otherwise</returns>
        private static bool ValidStructPropertyString(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            string[] split = s.SplitWithQuotes();
            return split.Length == 2;
        }

        /// <summary>
        /// Parse a property string in the format: "key" "value", and add it to the structure
        /// </summary>
        /// <param name="gs">The structure to add the property to</param>
        /// <param name="prop">The property string to parse</param>
        private static void ParseProperty(GenericStructure gs, string prop)
        {
            string[] split = prop.SplitWithQuotes();
            gs.Properties.Add(new GenericStructureProperty(split[0], (split[1] ?? "").Replace('`', '"')));
        }
        #endregion
    }
}
