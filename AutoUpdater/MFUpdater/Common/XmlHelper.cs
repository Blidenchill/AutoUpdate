using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace MFUpdater.Common
{
    /// <summary> 只支持class类型 因为此项在struct返回默认的之后无法判断是否成功 </summary>
    public static class XmlClassData
    {

        /// <summary> 从XML读取数据 </summary>
        /// <typeparam name="C"> 读取的数据类型 </typeparam>
        /// <param name="fileInfo"> 包含数据的文件 FileInfo 信息 </param>
        /// <returns> 返回为 null 的时候读取失败 </returns>
        public static C ReadDataFromXml<C>(this FileInfo fileInfo) where C : class
        {
            return ReadDataFromXml<C>(fileInfo.FullName);
        }

        /// <summary> 从XML读取数据 </summary>
        /// <typeparam name="C"> 读取的数据类型 </typeparam>
        /// /// <param name="FullPath"> 包含数据的文件路径</param>
        /// <returns> Tpye = C ， 返回为null的时候读取失败 </returns>
        public static C ReadDataFromXml<C>(this string FullPath) where C : class
        {
            return XmlAction.Read(FullPath, typeof(C)) as C;
        }

        /// <summary> 从XML读取数据 </summary>
        /// <typeparam name="C">  读取的数据类型 </typeparam>
        /// <param name="stream">包含数据的数据流 </param>
        /// <returns> Tpye = C ， 返回为null的时候读取失败 </returns>
        public static C ReadDataFromXml<C>(this Stream stream) where C : class
        {
            return XmlAction.Read(stream, typeof(C)) as C;
        }

        /// <summary> 写入数据到 XML </summary>
        /// <typeparam name="C"> 读取的数据类型 </typeparam>
        /// <param name="obj"> 将要写入的数据</param>
        /// <param name="fullpath"> 写 =入的文件路径 </param>
        /// <returns> 返回为 null 的时候写入成功 </returns>
        public static string WriteDataToXml<C>(this C obj, string fullpath) where C : class
        {
            try
            {
                XmlAction.Save(obj, fullpath);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static Stream WriteDataToStream<C>(this C obj) where C : class
        {
            try
            {

                return XmlAction.SaveStream(obj);
            }
            catch
            {

            }
            return null;
        }
    }

    /// <summary> struct 类型进行读取时，不确保一定成功 如果读取不成功会返回 default(S) </summary>
    public static class XmlStructData
    {

        /// <summary> 从XML读取数据 </summary>
        /// <typeparam name="S"> 读取的数据类型 </typeparam>
        /// <param name="fileInfo"> 包含数据的文件 FileInfo 信息 </param>
        /// <returns> 返回为默认值的时候读取失败 </returns>
        public static S ReadStructFromXml<S>(this FileInfo fileInfo) where S : struct
        {
            return ReadStructFromXml<S>(fileInfo.FullName);
        }

        /// <summary> 从XML读取数据 </summary>
        /// <typeparam name="S"> 读取的数据类型 </typeparam>
        /// <param name="fileInfo"> 包含数据的文件 FileInfo 信息 </param>
        /// <returns> 返回为默认值的时候读取失败 </returns>
        public static S ReadStructFromXml<S>(this string FullPath) where S : struct
        {
            var data = XmlAction.Read(FullPath, typeof(S));
            return data != null ? (S)data : default(S);
        }

        /// <summary> 从XML读取数据 ，返回为默认值的时候读取失败 </summary>
        /// <typeparam name="S"> 读取的数据类型 </typeparam> 
        /// <param name="FullPath"> 读取的路径 </param>
        /// <returns> 返回为默认值的时候读取失败 </returns>
        public static S ReadStructFromXml<S>(this Stream stream) where S : struct
        {
            var data = XmlAction.Read(stream, typeof(S));
            return data != null ? (S)data : default(S);
        }

        /// <summary> 写入数据到XML </summary>
        /// <typeparam name="S"> 写入的数据类型 </typeparam> 
        /// <param name="obj"></param>
        /// <param name="fullpath"> 写入的路径 </param>
        /// <returns> 返回为 null 的时候写入成功 </returns>
        public static string WriteStructToXml<S>(this S obj, string fullpath) where S : struct
        {
            try
            {
                XmlAction.Save(obj, fullpath);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }

    /// <summary> 封装对与Xml读写操作的类 </summary>
    internal static class XmlAction
    {

        internal static void Save(object obj, string filePath)
        {
            Save(obj, filePath, obj.GetType());
        }

        internal static void Save(object obj, string filePath, System.Type type)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    var xs = new XmlSerializer(type);
                    xs.Serialize(writer, obj);
                    writer.Close();
                }
            }
            catch { }
        }

        internal static Stream SaveStream(object obj)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            var xs = new XmlSerializer(obj.GetType());
            xs.Serialize(writer, obj);
            writer.Close();

            return stream as Stream;
        }

        internal static object Read(string filePath, System.Type type)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return null;
            }
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    var xs = new XmlSerializer(type);
                    object obj = xs.Deserialize(reader);
                    reader.Close();
                    return obj;
                }
            }
            catch { }
            return null;
        }

        internal static object Read(Stream stream, System.Type type)
        {
            if (stream == null || stream.Length == 0)
            {
                return null;
            }
            try
            {
                var xs = new XmlSerializer(type);
                object obj = xs.Deserialize(stream);
                return obj;
            }
            catch { }
            return null;
        }
    }
}
