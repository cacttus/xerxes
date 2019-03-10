using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using HtmlAgilityPack;

namespace Proteus
{
    public class StringUtils
    {
        public static string FindAndEatSubstringRange(string substrBegin, string substrEnd, ref string strToEat, bool eatString = true)
        {
            int ind=0;
            string ret = "";

            ind = StringUtils.EatToFirst(substrBegin, ref strToEat, eatString);

            if(ind<0)
                return ret;

            ind = strToEat.IndexOf(substrEnd);
            if(ind<0)
                return ret;
            
            ret = strToEat.Substring(0, ind+substrEnd.Length);
            
            if(eatString)
                strToEat = strToEat.Substring(ind+substrEnd.Length, strToEat.Length - (ind+substrEnd.Length));

            return ret;
        }
        public static int EatToFirst(string strToFind, ref string strToEat, bool eatString = true)
        {
            // Finds the first occurrence of "strToFind" and returns the string
            // split at "strToFind";  Returns the position right before the start of strToFind, or -1 if not found
            
            int ind = 0;
            
            ind = strToEat.IndexOf(strToFind);
            if (ind < 0)
            {
                return -1;
            }

            if(eatString)
                strToEat = strToEat.Substring(ind, strToEat.Length - ind);

            return ind;
        }
        public static string MakeArg(string astrSwitch, string astrArg="")
        {
            if(!astrArg.Equals(String.Empty))
                return astrSwitch + ":" + astrArg + " ";
            return astrSwitch + " ";
        }
        public static bool ParseCmdArg(string strLine, string strSwitch, ref string strOut)
        {
            string[] a01 = StringUtils.SplitFirst(':', strLine);

            if (!a01[0].ToLower().Equals(strSwitch.ToLower()))
                return false;

            strOut = String.Empty;

            if (a01.Length==2)
            {
                strOut = a01[1];
                if (string.IsNullOrEmpty(strOut))
                    throw new Exception(@"Invalid argument for parameter " + strSwitch + "(line = " + strLine + ") ");

                strOut = BuildUtils.Dequote(strOut).Trim();
            }

            return true;
        }

        public static string[] SplitFirst(char c, string str)
        {
            int cind = str.IndexOf(':');
            string[] a01;

            if (cind < 0)
            {
                a01 = new string[1];
                a01[0] = str;
            }
            else
            {
                a01 = new string[2];
                a01[0] = str.Substring(0, cind);
                a01[1] = str.Substring(cind + 1, str.Length - (cind + 1));
            }
            return a01;
        }

        public static string WrapString(string str, int charsperLine = 100, bool blnWholeWordsOnly = true)
        {
            string ret = string.Empty;
            int numWraps = 0;
            int lastSpaceIdx = 0;
            int lastWrapIdx = -1;

            str = str.Replace("\r\n", "");
            str = str.Replace("\n", "");

            for (int n = 0; n < str.Length; n++)
            {
                ret += str[n];

                if (n % charsperLine == 0)
                    numWraps++;

                if (char.IsWhiteSpace(str[n]))
                    lastSpaceIdx = n;

                if (numWraps > 0)
                {
                    if (blnWholeWordsOnly == true)
                    {
                        /// if we haven't. then do it.
                        if(lastSpaceIdx==lastWrapIdx)
                        {
                            //we can't wrap. cut the word.
                            ret += "\n";
                            numWraps--;
                        }
                        else
                        {
                            //wrap at last space.
                            ret = ret.Insert(lastSpaceIdx, "\n");
                            numWraps--;
                            lastWrapIdx = n;
                        }
                    }
                    else
                    {
                        //cut mid - word. regardless
                        ret += "\n";
                        numWraps = 0;
                    }
                }
                

            }

            return ret;
        }
        public static string MakeParagraph(string str, int charsPerLine = 100, int indentCount = 1, int tabSpaceSize = 4)
        {
            string ret = string.Empty;

            // get space size
            string indentSize = string.Empty;
            for (int n = 0; n < indentCount; n++)
                for (int m = 0; m < tabSpaceSize; m++)
                    indentSize += " ";


            ret = WrapString(str, charsPerLine);

            ret = tabSpaceSize + ret;// - First line
            ret.Replace("\n", indentSize + "\n"); // Subsequent

            ret = WrapString(ret, charsPerLine);

            return ret;
        }
        public static string Enquote(string a)
        {
            return "\"" + a + "\"";
        }
        public static string Dequote(string a)
        {
            return a.Replace('"', ' ').Trim();
        }
        public static string CombinePath(string path1, string path2)
        {
            string ret = path1.Trim().TrimEnd('/').TrimEnd('\\')
            + "\\"
            + path2.Trim().TrimStart('/').TrimStart('\\');
            return ret;
        }
        public static byte[] SerializeUTF8Path(string path)
        {
            byte[] ret = new byte[0];
            byte[] utf8Path = System.Text.Encoding.UTF8.GetBytes(path);
            Int64 pathlen = utf8Path.Length;
            ret = BufferUtils.Combine(ret, BitConverter.GetBytes(pathlen));
            ret = BufferUtils.Combine(ret, utf8Path);
            return ret;
        }

    }
}
