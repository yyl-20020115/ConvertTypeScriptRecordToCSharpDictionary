namespace ConvertTypeScriptRecordToCSharpDictionary;

internal class Program
{
    const string DefaultDictionaryName = "Default";
    static void Main(string[] args)
    {
        if (args.Length >= 2 && File.Exists(args[0]))
        {
            var started = false;
            var dictname = DefaultDictionaryName;
            using var reader = new StreamReader(args[0]);
            using var writer = new StreamWriter(args[1]);
            string? line;
            writer.WriteLine("public class DictClass{");
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                var result = "";
                if ((line.StartsWith("export") || line.StartsWith("const")) && (line.EndsWith('{') || line.EndsWith('=') || line.EndsWith('>')))
                {
                    int i = line.IndexOf(':');
                    if (i >= 0)
                    {
                        int s = line.LastIndexOf(' ', i - 1);
                        if (s >= 0 && s < i)
                        {
                            dictname = line.Substring(s + 1, i - s - 1);
                        }
                        if (dictname.Length == 0)
                        {
                            dictname = DefaultDictionaryName;
                        }
                    }
                    result = $"public readonly Dictionary<string,string> {dictname} = {{";
                }
                else if (line.Contains(':') && (started || !line.EndsWith('=') && !line.EndsWith('}') && !line.EndsWith('>')))
                {
                    started = true;
                    line = line.TrimEnd(',');
                    int i = 0;

                    while ((i = line.IndexOf(':', i + 1)) >= 0)
                    {
                        int p = line.IndexOfAny(['\'', '\"'], i + 1);
                        int q = line.LastIndexOfAny(['\'', '\"'], i - 1);
                        //q>0 要求第一个字符不能为'或者"
                        if (p > i && q > 0)
                        {
                            var failed = false;
                            for (int t = i + 1; t < p; t++)
                            {
                                if (line[t] == ' ' || line[t] == '\t') continue;
                                else
                                {
                                    failed = true;
                                    break;
                                }
                            }
                            if (!failed) break;
                        }
                    }
                    if (i < 0) continue;

                    string[] parts = [line[..i], line[(i + 1)..]];

                    for (i = 0; i < parts.Length; i++)
                    {
                        var p = parts[i].Trim();
                        if (p.Length > 0)
                        {
                            if (p.StartsWith('"') && p.EndsWith('\"'))
                            {
                                //如果双引号开头结尾，里面的单引号都要加"\\"
                                parts[i] = p.Replace("\'", "\\'");
                            }
                            else if (p.StartsWith('\'') && p.EndsWith('\''))
                            {
                                //如果单引号开头结尾，替换为双引号
                                parts[i] = $"\"{p[1..^1]}\"";
                            }
                            if (!p.StartsWith('\'')) parts[i] = "\"" + p;
                            if (!p.EndsWith('\'')) parts[i] += "\"";
                        }
                    }
                    result = $"\t[{parts[0]}]={parts[1]},";

                }
                if (result.Length > 0)
                {
                    writer.WriteLine(result);
                }
            }
            if (started)
            {
                writer.WriteLine("};");
            }
            writer.WriteLine("}");
        }
    }
}
