using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using PubOp;

namespace QQRoboter
{
   
        public class InIHelper
        {
            private static string FileName = Application.StartupPath + "\\user.ini";
            /// <summary>
            /// 读取配置文件
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="section"></param>
            /// <param name="key"></param>
            /// <returns></returns>
            public static T ReadConfig<T>(string section, string key)
            {
                if (File.Exists(FileName))
                {
                    IniFile f = new IniFile(FileName);
                    string value = f.ReadContentValue(section, key);

                    if (String.IsNullOrWhiteSpace(value))
                        return default(T);

                    if (typeof(T).IsEnum)
                        return (T)Enum.Parse(typeof(T), value, true);

                    return (T)Convert.ChangeType(value, typeof(T));
                }
                else
                {
                    return default(T);
                }
            }

            /// <summary>
            /// 写配置文件
            /// </summary>
            /// <param name="section"></param>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public static void WriteConfig(string section, string key, string value)
            {
                //如果文件不存在，则创建
                if (!File.Exists(FileName))
                {
                    using (FileStream myFs = new FileStream(FileName, FileMode.Create)) { }
                }

                IniFile f = new IniFile(FileName);
                f.WriteContentValue(section, key, value);
            }
        }
    }

