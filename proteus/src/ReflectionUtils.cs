using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace Proteus
{
    public class ReflectionUtils
    {
        public static List<string> GetPublicStringField(string fieldName, object obj)
        {
            List<string> ret = new List<string>();

            Type type = obj.GetType();
            foreach (var f in type.GetFields().Where(f => f.IsPublic))
            {
                if (f.MemberType != MemberTypes.Field)
                    continue;
                if (f.FieldType != typeof(string))
                    continue;
                if (!f.Name.Equals(fieldName))
                    continue;
                string str = (string)f.GetValue(obj);
                ret.Add(str);
            }

            return ret;
        }
        public static List<string> GetAllPublicStringFields(object obj)
        {
            List<string> ret = new List<string>();

            Type type = obj.GetType();
            foreach (var f in type.GetFields().Where(f => f.IsPublic))
            {
                if (f.MemberType != MemberTypes.Field)
                    continue;
                if (f.FieldType != typeof(string))
                    continue;

                string str = (string)f.GetValue(obj);
                ret.Add(str);
            }

            return ret;
        }
    }
}
