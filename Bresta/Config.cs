using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BrestaTest.Bresta
{
    public class KeyValueParameter
    {
        public string Comment { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public static KeyValueParameter Parse(string s)
        {
            KeyValueParameter parameter = new KeyValueParameter { };
            var spl = s.Split(':');

            if(spl.Length > 0)
            {
                parameter.Key = spl[0].Trim();
            }
            if (spl.Length > 1)
            {
                parameter.Value = spl[1].Trim();
            }
            return parameter;
        }
    }

    public class Section
    {
        public string Name { get; set; } = string.Empty;
        public string Comment { set; get; }

        public Dictionary<string , KeyValueParameter> Parameters { get; set;} = new Dictionary<string, KeyValueParameter>();

        public void AddParameter(KeyValueParameter keyValue)
        {
            if(Parameters.ContainsKey(keyValue.Key))
            {
                Parameters[keyValue.Key].Value += keyValue.Value;
            }
            else
            {
                Parameters.Add(keyValue.Key, keyValue);
            }
        }

    }

    public class Object
    {
        public Dictionary<string, Section> Sections { get; set; } = new Dictionary<string, Section>();

        public KeyValueParameter FindKeyValue(string paramName)
        {
            foreach (var section in Sections)
            {
                if(section.Value.Parameters.ContainsKey(paramName))
                {
                    return section.Value.Parameters[paramName];
                }
            }

            return null;
        }

        public VisualObject VisualObject { get; set; }

        public void MakeVisualObject()
        {

        }

    }

    public class Config
    {

        const string REGEX_HEADER = @".*?(?<=\{)(.*?)(?=\}).*";
        const string REGEX_COMMENT = @"\s*?//(.*)";
        const string REGEX_SECTION = @".*?(?<=\[)(.+?)(?=\]).*";
        const string REGEX_KEYVALUE = @"(.*?):(.*)";

        const string SECTION_HEADER = "header";
        const string SECTION_ROOT = "root";
        const string SECTION_EMPTY ="";
        const string SECTION_LH = "lh";

        const string PARAM_POS = "pos";
        const string PARAM_SIZE = "size";
        const string PARAM_BACKGROUND = "background";
        const string PARAM_STROKE = "stroke";
        const string PARAM_SIGN = "sign";
        const string PARAM_NAME = "name";
        const string PARAM_TYPE = "type";

        public string ConfigName { get; set; }
        public string Name
        {
            get
            {
                var names = Objects.Select(o=>o.FindKeyValue(PARAM_NAME)).ToArray();

                if(names.Length > 0)
                {
                    return names.First().Value;
                }


                return "NO HEADER";
            }
        }

        public int Width => Right - Left;
        public int Left => Objects.Min(o => o.VisualObject.Left);
        public int Right => Objects.Max(o => o.VisualObject.Left + o.VisualObject.Width);

        public List<Object> Objects { get; set; } = new List<Object>();

        private static IEnumerable<int> SplitStrToIntegerValues(string str)
        {
            List<int> values = new List<int>();

            var split = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var s in split)
            {
                if(int.TryParse(s, out int v))
                {
                    values.Add(v);
                }
                else
                {
                    values.Add(int.MinValue);
                }
            }

            return values;
        }

        private static VisualObject ToVisualObjects(Object obj)
        {
            if (!obj.Sections.ContainsKey(SECTION_HEADER)) return null;

            var header = obj.Sections[SECTION_HEADER];

            if (!header.Parameters.ContainsKey(PARAM_POS)) return null;

            var posValues = SplitStrToIntegerValues(header.Parameters[PARAM_POS].Value).ToArray();
            if (posValues.Length < 2) return null;

            var sizeValues = SplitStrToIntegerValues(header.Parameters[PARAM_SIZE].Value).ToArray();
            if (sizeValues.Length < 2) return null;

            VisualObject visualObject = new VisualObject();
            visualObject.Left = posValues[0];
            visualObject.Top = posValues[1];

            visualObject.Width = sizeValues[0];
            visualObject.Height = sizeValues[1];

            if (header.Parameters.ContainsKey(PARAM_NAME))
            {
                visualObject.Name = header.Parameters[PARAM_NAME].Value;
            }

            var background = obj.FindKeyValue(PARAM_BACKGROUND);
            if (background != null)
            {
                var backgroundStr = background.Value;

                var splBackground = backgroundStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (splBackground.Length > 1)
                {
                    visualObject.Color1 = (Color)ColorConverter.ConvertFromString(splBackground[0]);
                    visualObject.Color2 = (Color)ColorConverter.ConvertFromString(splBackground[1]);
                }
            }

            var stroke = obj.FindKeyValue(PARAM_STROKE);
            if (stroke != null)
            {
                visualObject.Stroke = (Color)ColorConverter.ConvertFromString(stroke.Value);
            }

            var type = obj.FindKeyValue(PARAM_TYPE);
            if (type != null)
            {
                visualObject.VisualType = type.Value == "button" ? TypeVisualObject.Button : TypeVisualObject.Path;
            }

            return visualObject;

        }

        public static bool TryLoadFile(string fileName, out Config config)
        {
            config = new Config();
            if(File.Exists(fileName))
            {
                config.ConfigName = Path.GetFileName(fileName);

                Regex reComment = new Regex(REGEX_COMMENT);
                Regex reHeader = new Regex(REGEX_HEADER);
                Regex reSection = new Regex(REGEX_SECTION);
                Regex reKeyValue = new Regex(REGEX_KEYVALUE);


                using (var f = new StreamReader(fileName))
                {
                    string currentComment = string.Empty;
                    Section currentSection = new Section() { Name = SECTION_EMPTY };

                    Object obj = new Object();

                    while (!f.EndOfStream)
                    {
                        var line = f.ReadLine();

                        //Если комментарий, то добавляем к текущему комментарию (если вдруг многострочный комментарий)
                        var matchComment = reComment.Match(line);
                        if(matchComment.Success)
                        {
                            currentComment += matchComment.Groups[1].Value.Trim();
                        }

                        //Если строка в фигурных скобках
                        var matchHeader = reHeader.Match(line);
                        if (matchHeader.Success)
                        {
                            Section header = new Section
                            {
                                Name = SECTION_HEADER,
                                Comment = currentComment
                            };
                            currentComment = string.Empty;

                            //Если в рабочей секции есть параметры, то добавить
                            if (currentSection.Parameters.Count > 0)
                            {
                                obj.Sections.Add(currentSection.Name, currentSection);
                            }

                            //Если в рабочем объекте есть параметры, то добавить
                            if (obj.Sections.Count > 0)
                            {
                                obj.VisualObject = ToVisualObjects(obj);
                                config.Objects.Add(obj);
                            }

                            //Работам с новым объектом
                            obj = new Object();

                            var splitParameters = matchHeader.Groups[1].Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach ( var p in splitParameters )
                            {
                                header.AddParameter(KeyValueParameter.Parse(p));
                            }
                            obj.Sections.Add(SECTION_HEADER, header);

                            //Текущая секция - root
                            currentSection = new Section() { Name = SECTION_ROOT };
                            continue;
                        }

                        //Если строка в квадратных скобках, то меняем текущую секцию
                        var matchSection = reSection.Match(line);
                        if(matchSection.Success)
                        {
                            if(currentSection.Parameters.Count > 0) 
                            {
                                obj.Sections.Add(currentSection.Name, currentSection);
                            }


                            currentSection = new Section() { Name = matchSection.Groups[1].Value.Trim(), Comment = currentComment };
                            currentComment = string.Empty;
                            continue;
                        }

                        //Если строка ключ - значение, внести в текущую секцию
                        var matchKeyValue = reKeyValue.Match(line);
                        if (matchKeyValue.Success)
                        {
                            var keyValue = KeyValueParameter.Parse(line);
                            keyValue.Comment = currentComment;
                            currentComment = string.Empty;

                            currentSection.AddParameter(keyValue);
                        }
                    }

                    if(currentSection.Parameters.Count > 0)
                    {
                        obj.Sections.Add(currentSection.Name, currentSection);
                    }

                    if(obj.Sections.Count > 0)
                    {
                        obj.VisualObject = ToVisualObjects(obj);
                        config.Objects.Add(obj);
                    }
                }

                if(config.Objects.Count > 0)
                {
                    return true;
                }
            }


            return false;
        }

    }
}
