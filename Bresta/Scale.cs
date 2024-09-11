using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BrestaTest.Bresta
{

    public class ScaleSection
    {
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }

    public class Scale
    {
        const string REGEX_HEADER = @".*(?<=\{)(.+)(?=\}).*";
        const string REGEX_COMMENT = @"\s*?//.*";
        const string REGEX_SECTION_BEGIN = @".*(?<=\[)(.+)(?=\]).*";

        public Dictionary<string, string> Header { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, ScaleSection> Sections { get; set; } = new Dictionary<string, ScaleSection>();

        public static bool TryGetScaleFromFile(string fileName, out Scale[] scales)
        {
            List<Scale> listScales = new List<Scale>();
            scales = new Scale[] { };

            if(File.Exists(fileName))
            {

                using (var f = new StreamReader(fileName))
                {
                    bool headerIsFound = false;
                    bool headerIsCorrect = true;

                    string currentSection = "Root";
                    while (!f.EndOfStream)
                    {
                        string headerBody = "";
                        var scale = new Scale();

                        var line = f.ReadLine().Trim();

                        if (!headerIsFound)
                        {
                            Regex re = new Regex(REGEX_HEADER);

                            var m = re.Match(line);
                            if (m.Success)
                            {
                                headerIsFound = true;
                                headerBody = m.Groups[1].Value;

                                var parameters = headerBody.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                                if (parameters.Length > 0)
                                {
                                    currentSection = "Root";
                                    foreach (var parameter in parameters)
                                    {
                                        var keyValue = parameter.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                                        if (keyValue.Length == 2)
                                        {

                                            var key = keyValue[0].Trim();

                                            if (scale.Header.ContainsKey(key))
                                            {
                                                headerIsCorrect = false;
                                                break;
                                            }

                                            scale.Header.Add(key, keyValue[1].Trim());

                                        }
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                                continue;
                            }
                        }

                        if (!headerIsFound)
                        {
                            return false;
                        }


                        Regex reComment = new Regex(REGEX_COMMENT);
                        Regex reSectionBegin = new Regex(REGEX_SECTION_BEGIN);

                        var matchComment = reComment.Match(line);

                        if (matchComment.Success) continue;

                        var mathComment = reComment.Match(line);
                        var matchSectionBegin = reSectionBegin.Match(line);

                        if(matchSectionBegin.Success)
                        {
                            currentSection = matchSectionBegin.Groups[1].Value;
                            continue;
                        }

                        if(line.Contains(":"))
                        {
                            var keyValue = line.Split(new char[] { ':' });

                            if(!scale.Sections.ContainsKey(currentSection))
                            {
                                scale.Sections.Add(currentSection, new ScaleSection() { });
                            }

                            var key = keyValue[0].Trim();

                            if (scale.Sections[currentSection].Parameters.ContainsKey(key))
                            {
                                scale.Sections[currentSection].Parameters[key] += $"\n{keyValue[1]}";
                            }
                            else
                            {
                                scale.Sections[currentSection].Parameters.Add(key, keyValue[1] );
                            }

                            
                        }
                    }


                }


            }


            return false;
        }
    }
}
