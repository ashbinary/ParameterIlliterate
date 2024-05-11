using LightByml.Lp.Byml;
using LightByml.Lp.Byml.Reader;
using System.Text.Json;
using System.Text;
using Force.Crc32;

namespace ParameterIlliterate {
    class Program {
        public static void Main(string[] args) {
            
            if (args.Length == 0)
            {
                Console.WriteLine("A .bprm file is needed as an argument for this to work!");
                Console.ReadLine(); // Pause
                return;
            }

            string[] hashesFile = File.ReadAllLines("ParamDictionary.txt");
            Dictionary<uint, string> hashes = new();

            foreach (string hash in hashesFile) {
                hashes.Add(Crc32Algorithm.Compute(Encoding.ASCII.GetBytes(hash)), hash);
            }

            byte[] paramFile = File.ReadAllBytes(args[0]);

            BymlIter bymlIter = new(paramFile);
            Object deserializedParamFile = VisitContainer(bymlIter);

            var paramJsonBIN = JsonSerializer.SerializeToUtf8Bytes(deserializedParamFile, new JsonSerializerOptions() { WriteIndented = true });
            var paramJson = Encoding.UTF8.GetString(paramJsonBIN);

            foreach (var hashedKey in hashes) {
                if (paramJson.Contains(hashedKey.Key.ToString("x"))) {
                    paramJson = paramJson.Replace($"\"{hashedKey.Key.ToString("x")}\"", $"\"{hashedKey.Value}\"");
                }
            }
            
            File.WriteAllText(Path.GetFileNameWithoutExtension(args[0]) + ".json", paramJson);
        }

        private static dynamic? VisitData(in BymlIter iter, in BymlData data)
        {
            switch (data.Type)
            {
                case BymlNodeId.String:
                    if (!iter.TryConvertString(out var vs, in data))
                        throw new Exception();
                    return vs;
                case BymlNodeId.Bin:
                    if (!iter.TryConvertBinary(out var vbi, in data))
                        throw new Exception();
                    return vbi.ToArray();
                case BymlNodeId.Bool:
                    if (!iter.TryConvertBool(out var vb, in data))
                        throw new Exception();
                    return vb;
                case BymlNodeId.Int:
                    if (!iter.TryConvertInt(out var vi, in data))
                        throw new Exception();
                    return vi;
                case BymlNodeId.Float:
                    if (!iter.TryConvertFloat(out var vf, in data))
                        throw new Exception();
                    return vf;
                case BymlNodeId.UInt:
                    if (!iter.TryConvertUInt(out var vui, in data))
                        throw new Exception();
                    return vui;
                case BymlNodeId.Int64:
                    if (!iter.TryConvertInt64(out var vl, in data))
                        throw new Exception();
                    return vl;
                case BymlNodeId.UInt64:
                    if (!iter.TryConvertUInt64(out var vul, in data))
                        throw new Exception();
                    return vul;
                case BymlNodeId.Double:
                    if (!iter.TryConvertDouble(out var vd, in data))
                        throw new Exception();
                    return vd;
                case BymlNodeId.Null:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static object VisitArray(in BymlIter iter)
        {
            var size = iter.Size;
            var array = new object?[size];
            for (var i = 0; i < size; i++)
            {
                var data = new BymlData();
                if (!iter.GetBymlDataByIndex(ref data, i))
                    throw new Exception();

                if (data.Type == BymlNodeId.Array || data.Type == BymlNodeId.Hash)
                    array[i] = VisitContainer(iter.GetIterByIndex(i));
                else
                    array[i] = VisitData(in iter, in data);
            }

            return array;
        }

        private static dynamic VisitHash(in BymlIter iter)
        {
            var obj = new System.Dynamic.ExpandoObject();
            var dict = (IDictionary<string, object?>)obj;
            var size = iter.Size;
            for (var i = 0; i < size; i++)
            {
                iter.GetKeyName(out var key, i);

                var data = new BymlData();
                if (!iter.GetBymlDataByIndex(ref data, i))
                    throw new Exception();

                if (data.Type == BymlNodeId.Array || data.Type == BymlNodeId.Hash) 
                    dict[key!] = VisitContainer(iter.GetIterByIndex(i));
                else
                    dict[key!] = VisitData(in iter, in data);
            }

            return obj;
        }

        private static dynamic VisitContainer(in BymlIter iter)
        {
            if (iter.IsTypeArray)
            {
                return VisitArray(in iter);
            }
            if (iter.IsTypeHash)
            {
                return VisitHash(in iter);
            }
            throw new Exception();
        }
    }
}