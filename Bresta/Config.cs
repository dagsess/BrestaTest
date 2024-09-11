using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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


    /*public class ScaleSection
    {
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }*/

    public class Config
    {

        const string REGEX_HEADER = @".*?(?<=\{)(.*?)(?=\}).*";
        const string REGEX_COMMENT = @"\s*?//(.*)";
        const string REGEX_SECTION = @".*?(?<=\[)(.+?)(?=\]).*";
        const string REGEX_KEYVALUE = @"(.*?):(.*)";

        const string SECTION_HEADER = "header";
        const string SECTION_ROOT = "root";
        const string SECTION_EMPTY ="";

        public Dictionary<string, Section> Sections { get; set; } = new Dictionary<string, Section>();

        public static bool TryGetScaleFromFile(string fileName, out Config config)
        {
            config = new Config();
            if(File.Exists(fileName))
            {
                Regex reComment = new Regex(REGEX_COMMENT);
                Regex reHeader = new Regex(REGEX_HEADER);
                Regex reSection = new Regex(REGEX_SECTION);
                Regex reKeyValue = new Regex(REGEX_KEYVALUE);


                using (var f = new StreamReader(fileName))
                {
                    string currentComment = string.Empty;
                    Section currentSection = new Section() { Name = SECTION_EMPTY };

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

                            var splitParameters = matchHeader.Groups[1].Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach ( var p in splitParameters )
                            {
                                header.AddParameter(KeyValueParameter.Parse(p));
                            }
                            config.Sections.Add(SECTION_HEADER, header);
                            currentSection = new Section() { Name = SECTION_ROOT };
                            continue;
                        }

                        //Если строка в квадратных скобках
                        var matchSection = reSection.Match(line);
                        if(matchSection.Success)
                        {
                            if(currentSection.Parameters.Count > 0) 
                            {
                                config.Sections.Add(currentSection.Name, currentSection);
                            }
                            currentSection = new Section() { Name = matchSection.Groups[1].Value.Trim(), Comment = currentComment };
                            currentComment = string.Empty;
                            continue;
                        }

                        //Если строка ключ - значение
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
                        config.Sections.Add(currentSection.Name, currentSection);
                    }
                }

            }


            return false;
        }
    }
}
