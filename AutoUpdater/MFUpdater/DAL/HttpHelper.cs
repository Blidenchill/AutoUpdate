using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;

namespace MFUpdater
{
    public class HttpHelper
    {
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetCookie(string lpszUrlName, string lbszCookieName, string lpszCookieData);
        public static string cookie;
        public static Cookie mCookie;
        //线上
        public const string ServerDomain = "http://p.mofanghr.com/";
        //每日构建
        //public const string ServerDomain = "http://10.0.2.5:6688/";
        //idoss
        //public const string ServerDomain = "http://face.idoss.com/";
        //灰度
        //public const string ServerDomain = "http://p.mofangzp.top/";
        public const string ServerGetVersionJson = ServerDomain + "face/version";
        public static string HttpPost(string Url, string postDataStr, bool sign = false)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = postDataStr.Length;

                if (!sign)
                    request.Headers.Add(HttpRequestHeader.Cookie, cookie);
                request.Timeout = 10000;

                byte[] btBodys = Encoding.UTF8.GetBytes(postDataStr);
                request.ContentLength = btBodys.Length;
                request.GetRequestStream().Write(btBodys, 0, btBodys.Length);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                request.GetRequestStream().Flush();
                if (sign)
                {
                    cookie = response.Headers.Get("Set-Cookie");
                    mCookie = GetAllCookiesFromHeader(response.Headers.Get("Set-Cookie"), request.Host)[0];
                    InternetSetCookie(ServerDomain, HttpHelper.mCookie.Name, HttpHelper.mCookie.Value + ";expires=Sun,22-Feb-2099 00:00:00 GMT");
                }
                string encoding = response.ContentEncoding;
                if (encoding == null || encoding.Length < 1)
                {
                    encoding = "UTF-8"; //默认编码
                }
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
                string retString = reader.ReadToEnd();
                return retString;
            }
            catch (Exception ex)
            {
                return "连接失败：" + ex.ToString();

            }
        }

        public static string HttpGet(string Url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.ContentType = "application/json";
                request.Method = "GET";
                request.Timeout = 20000;
                request.Headers.Add(HttpRequestHeader.Cookie, cookie);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string encoding = response.ContentEncoding;
                if (encoding == null || encoding.Length < 1)
                {
                    encoding = "UTF-8"; //默认编码
                }
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
                string retString = reader.ReadToEnd();
                return retString;
            }
            catch (Exception ex)
            {
                return "连接失败：" + ex.ToString();

            }
        }

        public static CookieCollection GetAllCookiesFromHeader(string strHeader, string strHost)
        {
            ArrayList al = new ArrayList();
            CookieCollection cc = new CookieCollection();
            if (strHeader != string.Empty)
            {
                al = ConvertCookieHeaderToArrayList(strHeader);
                cc = ConvertCookieArraysToCookieCollection(al, strHost);
            }
            return cc;
        }

        private static ArrayList ConvertCookieHeaderToArrayList(string strCookHeader)
        {
            strCookHeader = strCookHeader.Replace("\r", "");
            strCookHeader = strCookHeader.Replace("\n", "");
            string[] strCookTemp = strCookHeader.Split(',');
            ArrayList al = new ArrayList();
            int i = 0;
            int n = strCookTemp.Length;
            while (i < n)
            {
                if (strCookTemp[i].IndexOf("expires=", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    al.Add(strCookTemp[i] + "," + strCookTemp[i + 1]);
                    i = i + 1;
                }
                else
                {
                    al.Add(strCookTemp[i]);
                }
                i = i + 1;
            }
            return al;
        }

        private static CookieCollection ConvertCookieArraysToCookieCollection(ArrayList al, string strHost)
        {
            CookieCollection cc = new CookieCollection();

            int alcount = al.Count;
            string strEachCook;
            string[] strEachCookParts;
            for (int i = 0; i < alcount; i++)
            {
                strEachCook = al[i].ToString();
                strEachCookParts = strEachCook.Split(';');
                int intEachCookPartsCount = strEachCookParts.Length;
                string strCNameAndCValue = string.Empty;
                string strPNameAndPValue = string.Empty;
                string strDNameAndDValue = string.Empty;
                string[] NameValuePairTemp;
                Cookie cookTemp = new Cookie();

                for (int j = 0; j < intEachCookPartsCount; j++)
                {
                    if (j == 0)
                    {
                        strCNameAndCValue = strEachCookParts[j];
                        if (strCNameAndCValue != string.Empty)
                        {
                            int firstEqual = strCNameAndCValue.IndexOf("=");
                            string firstName = strCNameAndCValue.Substring(0, firstEqual);
                            string allValue = strCNameAndCValue.Substring(firstEqual + 1, strCNameAndCValue.Length - (firstEqual + 1));
                            cookTemp.Name = firstName;
                            cookTemp.Value = allValue;
                        }
                        continue;
                    }
                    if (strEachCookParts[j].IndexOf("path", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strPNameAndPValue = strEachCookParts[j];
                        if (strPNameAndPValue != string.Empty)
                        {
                            NameValuePairTemp = strPNameAndPValue.Split('=');
                            if (NameValuePairTemp[1] != string.Empty)
                            {
                                cookTemp.Path = NameValuePairTemp[1];
                            }
                            else
                            {
                                cookTemp.Path = "/";
                            }
                        }
                        continue;
                    }

                    if (strEachCookParts[j].IndexOf("domain", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strPNameAndPValue = strEachCookParts[j];
                        if (strPNameAndPValue != string.Empty)
                        {
                            NameValuePairTemp = strPNameAndPValue.Split('=');

                            if (NameValuePairTemp[1] != string.Empty)
                            {
                                cookTemp.Domain = NameValuePairTemp[1];
                            }
                            else
                            {
                                cookTemp.Domain = strHost;
                            }
                        }
                        continue;
                    }
                }

                if (cookTemp.Path == string.Empty)
                {
                    cookTemp.Path = "/";
                }
                if (cookTemp.Domain == string.Empty)
                {
                    cookTemp.Domain = strHost;
                }
                cc.Add(cookTemp);
            }
            return cc;
        }


        #region "文件下载"

        public static Action<int> DonwloadSizeAction;
        public static bool Download(string picUrl, string savePath)
        {
            bool value = false;
            WebResponse response = null;
            Stream stream = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(picUrl);
                request.Timeout = 10000;
                response = request.GetResponse();
                stream = response.GetResponseStream();
                value = SaveBinaryFile(response, savePath);
            }
            catch { }
            finally
            {
                if (stream != null) stream.Close();
                if (response != null) response.Close();
            }
            return value;
            //try
            //{
            //    Stream stream = GetResponseStream(picUrl);
            //    byte[] buffer = new byte[1024];
            //    List<byte> bufferList = new List<byte>();
            //    using (BinaryReader sr = new BinaryReader(stream))
            //    {
            //        int index = 0;
            //        do
            //        {
            //            index = sr.Read(buffer, 0, buffer.Length);
            //            for (int i = 0; i < index; i++)
            //            {
            //                bufferList.Add(buffer[i]);
            //            }
            //        }
            //        while (index != 0);
            //    }
            //    using ( FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
            //    {
            //        using (StreamWriter sw = new StreamWriter(fs))
            //        {
            //            sw.Write(bufferList.ToArray());
            //        }
            //    }
            //    return true;
            //}
            //catch
            //{
            //    return false;
            //}
  
        }

        public static Stream GetResponseStream(string picUrl)
        {
            WebResponse response = null;
            Stream stream = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(picUrl);
                request.Timeout = 10000;
                response = request.GetResponse();
                stream = response.GetResponseStream();
            }
            catch { }
            return stream; ;
        }
        private static bool SaveBinaryFile(WebResponse response, string savePath)
        {
            bool value = false;
            byte[] buffer = new byte[1024];
            Stream outStream = null;
            Stream inStream = null;
            int countIndex = 1;
            try
            {
                if (File.Exists(savePath)) File.Delete(savePath);
                outStream = System.IO.File.Create(savePath);
                inStream = response.GetResponseStream();
                int l;
                do
                {
                    l = inStream.Read(buffer, 0, buffer.Length);
                    if (l > 0) outStream.Write(buffer, 0, l);
                    if(DonwloadSizeAction != null)
                    {
                        DonwloadSizeAction(countIndex * 1024);
                    }
                    countIndex++;
                }
                while (l > 0);
                value = true;
            }
            finally
            {
                if (outStream != null) outStream.Close();
                if (inStream != null) inStream.Close();
            }
            return value;
        }

        public static string GetFileNameFromUrl(string Url)
        {
            Uri u = new Uri(Url);
            string[] arrayurl = u.PathAndQuery.Split('?');
            string file = (arrayurl[0].Substring(arrayurl[0].LastIndexOf('/') + 1)).Replace(".aspx", "");
            return file;
        }

        #endregion
    }
}
