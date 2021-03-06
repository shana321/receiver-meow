﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Newbe.Mahua.Receiver.Meow.MahuaApis
{
    class Tools
    {
        //机器人qq号
        public static string qqNumber = XmlSolve.xml_get("settings","robot");
        //主人qq号
        public static string adminNumber = XmlSolve.xml_get("settings", "admin");
        //管理群的号码
        public static string mainGroupNumber = XmlSolve.xml_get("settings", "mainGroup");
        //开启专属功能？（一般不开）
        public static string special = XmlSolve.xml_get("settings", "special");
        //whatanime的api
        public static string whatanimeApi = XmlSolve.xml_get("settings", "whatanimeApi");
        //http代理的网址
        public static string proxyUrl = XmlSolve.xml_get("settings", "proxy");
        public static int messageCount = 0;
        public static string now = DateTime.Now.ToString();
        /// <summary>
        /// 判断是否发消息，消息速率限制函数
        /// </summary>
        /// <returns></returns>
        public static bool MessageControl(int count)
        {
            if (now == DateTime.Now.ToString())
            {
                if (messageCount < count)
                {
                    messageCount++;
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                now = DateTime.Now.ToString();
                messageCount = 0;
                return false;
            }
        }




        /// <summary>
        /// 获取at某人的整合格式
        /// </summary>
        /// <param name="qq"></param>
        /// <returns></returns>
        public static string At(string qq)
        {
            return "[CQ:at,qq=" + qq + "]";
        }

        /// <summary>
        /// 获取复读语句
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static string GetRepeatString(string msg, string group)
        {
            int sum = 0;
            try
            {
                sum = int.Parse(XmlSolve.xml_get("repeat_settings", group));
                if (GetRandomNumber(0, 100) < sum)
                {
                    if (sum <= 100)
                        return msg;
                    else
                    {
                        string result = "";
                        for (int i = 0; i < sum / 100; i++)
                            result += msg;
                        if (GetRandomNumber(0, 100) < sum % 100)
                            return result + msg;
                        else
                            return result;
                    }
                }
                else
                    return "";
            }
            catch
            {
                return "";
            }
        }



        /// <summary>
        /// 设置复读概率
        /// </summary>
        /// <param name="input"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static string SetRepeat(string input, string group)
        {
            int sum = 0;
            try
            {
                sum = int.Parse(input);
                if (sum > 1000)
                    sum = 1000;
                XmlSolve.del("repeat_settings", group);
                XmlSolve.insert("repeat_settings", group, sum.ToString());
                return "设置完成，复读概率更改为" + sum + "%";
            }
            catch
            {
                return "设置失败，请检查参数";
            }
        }


        /// <summary>
        /// 获取随机数
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetRandomNumber(int min, int max)
        {
            Random ran = new Random(System.DateTime.Now.Millisecond);
            return ran.Next(min, max);
        }

        /// <summary>
        /// 简化正则的操作
        /// </summary>
        /// <param name="str"></param>
        /// <param name="regstr"></param>
        /// <returns></returns>
        public static MatchCollection Reg_solve(string str, string regstr)
        {
            Regex reg = new Regex(regstr, RegexOptions.IgnoreCase);
            return reg.Matches(str);
        }

        /// <summary>
        /// 直接获取正则表达式的最后一组匹配值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="regstr"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string Reg_get(string str, string regstr, string name)
        {
            string result = "";
            MatchCollection matchs = Reg_solve(str, regstr);
            foreach (Match item in matchs)
            {
                if (item.Success)
                {
                    result = item.Groups[name].Value;
                }
            }
            return result;
        }

        /// <summary>
        /// 直接获取正则表达式的所有匹配值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="regstr"></param>
        /// <param name="name"></param>
        /// <param name="name">连接符</param>
        /// <returns></returns>
        public static string Reg_get_all(string str, string regstr, string name, string connect)
        {
            string result = "";
            MatchCollection matchs = Reg_solve(str, regstr);
            foreach (Match item in matchs)
            {
                if (item.Success)
                {
                    result += connect + item.Groups[name].Value;
                }
            }
            if (result != "")
                return result.Substring(connect.Length);
            else
                return result;
        }

        /// <summary>
        /// 获取字符串中的数字
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>数字</returns>
        public static string GetNumber(string str)
        {
            string result = "";
            if (str != null && str != string.Empty)
            {
                // 正则表达式剔除非数字字符（不包含小数点.）
                str = Regex.Replace(str, @"[^\d.\d]", "");
                // 如果是数字，则转换为decimal类型
                if (Regex.IsMatch(str, @"^[+-]?\d*[.]?\d*$"))
                {
                    result = str;
                }
            }
            return result;
        }



        /// <summary>
        /// 获取CMD执行结果
        /// </summary>
        /// <param name="command">命令</param>
        /// <returns></returns>
        public static string execCMD(string command)
        {
            System.Diagnostics.Process pro = new System.Diagnostics.Process();
            pro.StartInfo.FileName = "cmd.exe";
            pro.StartInfo.UseShellExecute = false;
            pro.StartInfo.RedirectStandardError = true;
            pro.StartInfo.RedirectStandardInput = true;
            pro.StartInfo.RedirectStandardOutput = true;
            pro.StartInfo.CreateNoWindow = true;
            //pro.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            pro.Start();
            pro.StandardInput.WriteLine(command);
            pro.StandardInput.WriteLine("exit");
            pro.StandardInput.AutoFlush = true;
            //获取cmd窗口的输出信息
            string output = pro.StandardOutput.ReadToEnd();
            pro.WaitForExit();//等待程序执行完退出进程
            pro.Close();
            output = output.Substring(output.IndexOf(command));
            return output.Substring(0, output.Length - 1);
        }


        /// <summary>
        /// GET 请求与获取结果
        /// </summary>
        public static string HttpGet(string Url, string postDataStr = "", int timeout = 5000)
        {
            try
            {
                //请求前设置一下使用的安全协议类型 System.Net
                if (Url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) =>
                    {
                        return true; //总是接受
                    });
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                }
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
                if(proxyUrl!="")
                    request.Proxy = new WebProxy(proxyUrl);
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";
                request.Timeout = timeout;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36 Vivaldi/2.2.1388.37";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string encoding = response.ContentEncoding;
                if (encoding == null || encoding.Length < 1)
                {
                    encoding = "UTF-8"; //默认编码
                }
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding(encoding));

                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();

                return retString;
            }
            catch { }
            return "";
        }

        /// <summary>
        /// POST请求与获取结果
        /// </summary>
        public static string HttpPost(string Url, string postDataStr, int timeout = 5000)
        {
            try
            {
                //请求前设置一下使用的安全协议类型 System.Net
                if (Url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) =>
                    {
                        return true; //总是接受
                    });
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                }
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                if (proxyUrl != "")
                    request.Proxy = new WebProxy(proxyUrl);
                request.Method = "POST";
                request.Timeout = timeout;
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.ContentLength = postDataStr.Length;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36 Vivaldi/2.2.1388.37";

                StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
                writer.Write(postDataStr);
                writer.Flush();
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
            catch (Exception e)
            { }
            return "";
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="Url">网址</param>
        /// <param name="fileName">文件名</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        public static bool FileDownload(string Url, string fileName, int timeout = 5000, bool change = true)
        {
            try
            {
                //请求前设置一下使用的安全协议类型 System.Net
                if (Url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) =>
                    {
                        return true; //总是接受
                    });
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                }
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                if (proxyUrl != "")
                    request.Proxy = new WebProxy(proxyUrl);
                request.ContentType = "text/html;charset=UTF-8";
                request.Timeout = timeout;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36 Vivaldi/2.2.1388.37";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                bool result = false;
                if (!response.ContentType.ToLower().StartsWith("text/") &&
                    response.ContentLength < 1024*1024*20)
                {
                    if (change)
                        result = SaveBinaryFile(response, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "data/image/download/" + fileName + "old");
                    else
                        result = SaveBinaryFile(response, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "data/image/download/" + fileName);
                }

                //防和谐处理
                if(result && change)
                {
                    FileStream fs = File.OpenRead(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "data/image/download/" + fileName + "old"); //OpenRead
                    int filelength = 0;
                    filelength = (int)fs.Length; //获得文件长度
                    Byte[] image = new Byte[filelength]; //建立一个字节数组
                    fs.Read(image, 0, filelength); //按字节流读取
                    System.Drawing.Image r = System.Drawing.Image.FromStream(fs);
                    fs.Close();
                    Bitmap bit = new Bitmap(r);//图片对象
                    bit.SetPixel(1, 1, bit.GetPixel(1, 1));
                    bit.Save(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "data/image/download/" + fileName,ImageFormat.Jpeg);
                }

                return result;
            }
            catch { }
            return false;
        }


        /// <summary>
        /// Save a binary file to disk.
        /// </summary>
        /// <param name="response">The response used to save the file</param>
        private static bool SaveBinaryFile(WebResponse response, string FileName)
        {
            bool Value = true;
            byte[] buffer = new byte[1024];

            try
            {
                if (File.Exists(FileName))
                    File.Delete(FileName);
                Stream outStream = System.IO.File.Create(FileName);
                Stream inStream = response.GetResponseStream();
                //inStream.ContentLength
                int l;
                do
                {
                    l = inStream.Read(buffer, 0, buffer.Length);
                    if (l > 0)
                        outStream.Write(buffer, 0, l);
                }
                while (l > 0);

                outStream.Close();
                inStream.Close();
            }
            catch
            {
                Value = false;
            }

            return Value;
        }

        ///<summary>
        ///生成随机字符串
        ///</summary>
        ///<param name="length">目标字符串的长度</param>
        ///<param name="useNum">是否包含数字，1=包含，默认为包含</param>
        ///<param name="useLow">是否包含小写字母，1=包含，默认为包含</param>
        ///<param name="useUpp">是否包含大写字母，1=包含，默认为包含</param>
        ///<param name="useSpe">是否包含特殊字符，1=包含，默认为不包含</param>
        ///<param name="custom">要包含的自定义字符，直接输入要包含的字符列表</param>
        ///<returns>指定长度的随机字符串</returns>
        public static string GetRandomString(int length, bool useNum, bool useLow, bool useUpp, bool useSpe, string custom)
        {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null, str = custom;
            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpe == true) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }


        ///  <summary>
        /// 获取指定驱动器的剩余空间总大小(单位为MB)
        ///  </summary>
        ///  <param name="str_HardDiskName">只需输入代表驱动器的字母即可 </param>
        ///  <returns> </returns>
        public static long GetHardDiskFreeSpace(string str_HardDiskName)
        {
            long freeSpace = new long();
            str_HardDiskName = str_HardDiskName + ":\\";
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.Name == str_HardDiskName)
                {
                    freeSpace = drive.TotalFreeSpace / (1024 * 1024);
                }
            }
            return freeSpace;
        }


        private static int[] pyValue = new int[]
{
                -20319,-20317,-20304,-20295,-20292,-20283,-20265,-20257,-20242,-20230,-20051,-20036,
                -20032,-20026,-20002,-19990,-19986,-19982,-19976,-19805,-19784,-19775,-19774,-19763,
                -19756,-19751,-19746,-19741,-19739,-19728,-19725,-19715,-19540,-19531,-19525,-19515,
                -19500,-19484,-19479,-19467,-19289,-19288,-19281,-19275,-19270,-19263,-19261,-19249,
                -19243,-19242,-19238,-19235,-19227,-19224,-19218,-19212,-19038,-19023,-19018,-19006,
                -19003,-18996,-18977,-18961,-18952,-18783,-18774,-18773,-18763,-18756,-18741,-18735,
                -18731,-18722,-18710,-18697,-18696,-18526,-18518,-18501,-18490,-18478,-18463,-18448,
                -18447,-18446,-18239,-18237,-18231,-18220,-18211,-18201,-18184,-18183, -18181,-18012,
                -17997,-17988,-17970,-17964,-17961,-17950,-17947,-17931,-17928,-17922,-17759,-17752,
                -17733,-17730,-17721,-17703,-17701,-17697,-17692,-17683,-17676,-17496,-17487,-17482,
                -17468,-17454,-17433,-17427,-17417,-17202,-17185,-16983,-16970,-16942,-16915,-16733,
                -16708,-16706,-16689,-16664,-16657,-16647,-16474,-16470,-16465,-16459,-16452,-16448,
                -16433,-16429,-16427,-16423,-16419,-16412,-16407,-16403,-16401,-16393,-16220,-16216,
                -16212,-16205,-16202,-16187,-16180,-16171,-16169,-16158,-16155,-15959,-15958,-15944,
                -15933,-15920,-15915,-15903,-15889,-15878,-15707,-15701,-15681,-15667,-15661,-15659,
                -15652,-15640,-15631,-15625,-15454,-15448,-15436,-15435,-15419,-15416,-15408,-15394,
                -15385,-15377,-15375,-15369,-15363,-15362,-15183,-15180,-15165,-15158,-15153,-15150,
                -15149,-15144,-15143,-15141,-15140,-15139,-15128,-15121,-15119,-15117,-15110,-15109,
                -14941,-14937,-14933,-14930,-14929,-14928,-14926,-14922,-14921,-14914,-14908,-14902,
                -14894,-14889,-14882,-14873,-14871,-14857,-14678,-14674,-14670,-14668,-14663,-14654,
                -14645,-14630,-14594,-14429,-14407,-14399,-14384,-14379,-14368,-14355,-14353,-14345,
                -14170,-14159,-14151,-14149,-14145,-14140,-14137,-14135,-14125,-14123,-14122,-14112,
                -14109,-14099,-14097,-14094,-14092,-14090,-14087,-14083,-13917,-13914,-13910,-13907,
                -13906,-13905,-13896,-13894,-13878,-13870,-13859,-13847,-13831,-13658,-13611,-13601,
                -13406,-13404,-13400,-13398,-13395,-13391,-13387,-13383,-13367,-13359,-13356,-13343,
                -13340,-13329,-13326,-13318,-13147,-13138,-13120,-13107,-13096,-13095,-13091,-13076,
                -13068,-13063,-13060,-12888,-12875,-12871,-12860,-12858,-12852,-12849,-12838,-12831,
                -12829,-12812,-12802,-12607,-12597,-12594,-12585,-12556,-12359,-12346,-12320,-12300,
                -12120,-12099,-12089,-12074,-12067,-12058,-12039,-11867,-11861,-11847,-11831,-11798,
                -11781,-11604,-11589,-11536,-11358,-11340,-11339,-11324,-11303,-11097,-11077,-11067,
                -11055,-11052,-11045,-11041,-11038,-11024,-11020,-11019,-11018,-11014,-10838,-10832,
                -10815,-10800,-10790,-10780,-10764,-10587,-10544,-10533,-10519,-10331,-10329,-10328,
                -10322,-10315,-10309,-10307,-10296,-10281,-10274,-10270,-10262,-10260,-10256,-10254
};

        private static string[] pyName = new string[]
                {
                "A","Ai","An","Ang","Ao","Ba","Bai","Ban","Bang","Bao","Bei","Ben",
                "Beng","Bi","Bian","Biao","Bie","Bin","Bing","Bo","Bu","Ba","Cai","Can",
                "Cang","Cao","Ce","Ceng","Cha","Chai","Chan","Chang","Chao","Che","Chen","Cheng",
                "Chi","Chong","Chou","Chu","Chuai","Chuan","Chuang","Chui","Chun","Chuo","Ci","Cong",
                "Cou","Cu","Cuan","Cui","Cun","Cuo","Da","Dai","Dan","Dang","Dao","De",
                "Deng","Di","Dian","Diao","Die","Ding","Diu","Dong","Dou","Du","Duan","Dui",
                "Dun","Duo","E","En","Er","Fa","Fan","Fang","Fei","Fen","Feng","Fo",
                "Fou","Fu","Ga","Gai","Gan","Gang","Gao","Ge","Gei","Gen","Geng","Gong",
                "Gou","Gu","Gua","Guai","Guan","Guang","Gui","Gun","Guo","Ha","Hai","Han",
                "Hang","Hao","He","Hei","Hen","Heng","Hong","Hou","Hu","Hua","Huai","Huan",
                "Huang","Hui","Hun","Huo","Ji","Jia","Jian","Jiang","Jiao","Jie","Jin","Jing",
                "Jiong","Jiu","Ju","Juan","Jue","Jun","Ka","Kai","Kan","Kang","Kao","Ke",
                "Ken","Keng","Kong","Kou","Ku","Kua","Kuai","Kuan","Kuang","Kui","Kun","Kuo",
                "La","Lai","Lan","Lang","Lao","Le","Lei","Leng","Li","Lia","Lian","Liang",
                "Liao","Lie","Lin","Ling","Liu","Long","Lou","Lu","Lv","Luan","Lue","Lun",
                "Luo","Ma","Mai","Man","Mang","Mao","Me","Mei","Men","Meng","Mi","Mian",
                "Miao","Mie","Min","Ming","Miu","Mo","Mou","Mu","Na","Nai","Nan","Nang",
                "Nao","Ne","Nei","Nen","Neng","Ni","Nian","Niang","Niao","Nie","Nin","Ning",
                "Niu","Nong","Nu","Nv","Nuan","Nue","Nuo","O","Ou","Pa","Pai","Pan",
                "Pang","Pao","Pei","Pen","Peng","Pi","Pian","Piao","Pie","Pin","Ping","Po",
                "Pu","Qi","Qia","Qian","Qiang","Qiao","Qie","Qin","Qing","Qiong","Qiu","Qu",
                "Quan","Que","Qun","Ran","Rang","Rao","Re","Ren","Reng","Ri","Rong","Rou",
                "Ru","Ruan","Rui","Run","Ruo","Sa","Sai","San","Sang","Sao","Se","Sen",
                "Seng","Sha","Shai","Shan","Shang","Shao","She","Shen","Sheng","Shi","Shou","Shu",
                "Shua","Shuai","Shuan","Shuang","Shui","Shun","Shuo","Si","Song","Sou","Su","Suan",
                "Sui","Sun","Suo","Ta","Tai","Tan","Tang","Tao","Te","Teng","Ti","Tian",
                "Tiao","Tie","Ting","Tong","Tou","Tu","Tuan","Tui","Tun","Tuo","Wa","Wai",
                "Wan","Wang","Wei","Wen","Weng","Wo","Wu","Xi","Xia","Xian","Xiang","Xiao",
                "Xie","Xin","Xing","Xiong","Xiu","Xu","Xuan","Xue","Xun","Ya","Yan","Yang",
                "Yao","Ye","Yi","Yin","Ying","Yo","Yong","You","Yu","Yuan","Yue","Yun",
                "Za", "Zai","Zan","Zang","Zao","Ze","Zei","Zen","Zeng","Zha","Zhai","Zhan",
                "Zhang","Zhao","Zhe","Zhen","Zheng","Zhi","Zhong","Zhou","Zhu","Zhua","Zhuai","Zhuan",
                "Zhuang","Zhui","Zhun","Zhuo","Zi","Zong","Zou","Zu","Zuan","Zui","Zun","Zuo"
                };

        /// <summary>
        /// 把汉字转换成拼音(全拼)
        /// </summary>
        /// <param name="hzString">汉字字符串</param>
        /// <returns>转换后的拼音(全拼)字符串</returns>
        public static string PinyinConvert(string hzString)
        {
            // 匹配中文字符
            Regex regex = new Regex("^[\u4e00-\u9fa5]$");
            byte[] array = new byte[2];
            string pyString = "";
            int chrAsc = 0;
            int i1 = 0;
            int i2 = 0;
            char[] noWChar = hzString.ToCharArray();

            for (int j = 0; j < noWChar.Length; j++)
            {
                // 中文字符
                if (regex.IsMatch(noWChar[j].ToString()))
                {
                    array = System.Text.Encoding.Default.GetBytes(noWChar[j].ToString());
                    i1 = (short)(array[0]);
                    i2 = (short)(array[1]);
                    chrAsc = i1 * 256 + i2 - 65536;
                    if (chrAsc > 0 && chrAsc < 160)
                    {
                        pyString += noWChar[j];
                    }
                    else
                    {
                        // 修正部分文字
                        if (chrAsc == -9254)  // 修正“圳”字
                            pyString += "Zhen";
                        else
                        {
                            for (int i = (pyValue.Length - 1); i >= 0; i--)
                            {
                                if (pyValue[i] <= chrAsc)
                                {
                                    pyString += pyName[i];
                                    break;
                                }
                            }
                        }
                    }
                }
                // 非中文字符
                else
                {
                    pyString += noWChar[j].ToString();
                }
            }
            return pyString;
        }



        public static string GetAir(string msg,string qq)
        {
            if (msg == "空气质量")
            {
                return At(qq) +
                    "\r\n使用帮助：\r\n发送空气质量加城市名称，即可查询\r\n如：空气质量New York";
            }
            string city = PinyinConvert(msg.Replace("空气质量", ""));
            string html;
            int city_code = 0;
            try
            {
                city_code = int.Parse(PinyinConvert(msg.Replace("空气质量", "")));
                try
                {
                    html = HttpGet("http://api.waqi.info/feed/@" + city_code + "/",
                        "token=737aa093c7d9c16b7c6fdc1b70af2fb02bf01e11");
                    JObject jo = (JObject)JsonConvert.DeserializeObject(html);

                    string station = (string)jo["data"]["city"]["name"];
                    string result = "";
                    string from = "";
                    try { result += "\r\n空气质量指数：" + (int)jo["data"]["aqi"]; } catch { }
                    try { result += "\r\npm2.5：" + (float)jo["data"]["iaqi"]["pm25"]["v"]; } catch { }
                    try { result += "\r\npm10：" + (float)jo["data"]["iaqi"]["pm10"]["v"]; } catch { }
                    try { result += "\r\nco：" + (float)jo["data"]["iaqi"]["co"]["v"]; } catch { }
                    try { result += "\r\nno2：" + (float)jo["data"]["iaqi"]["no2"]["v"]; } catch { }
                    try { result += "\r\no3：" + (float)jo["data"]["iaqi"]["o3"]["v"]; } catch { }
                    try { result += "\r\nso2：" + (float)jo["data"]["iaqi"]["so2"]["v"]; } catch { }
                    try { from = (string)jo["data"]["attributions"][0]["name"]; } catch { }
                    return At(qq) +
                        "\r\n" + station + "的空气质量如下：" + result +
                        "\r\n数据来源：" + from +
                        "\r\n数据更新时间：" + (string)jo["data"]["time"]["s"];
                }
                catch (Exception err)
                {
                    string aa = err.Message.ToString();
                    return At(qq) +
                        "\r\n机器人在查询数据时爆炸了，原因：" + aa;
                }
            }
            catch
            {
                try
                {
                    int station_count = 0;
                    string result = "";
                    html = HttpGet("http://api.waqi.info/search/",
                        "keyword=" + city + "&token=737aa093c7d9c16b7c6fdc1b70af2fb02bf01e11");
                    JObject jo = (JObject)JsonConvert.DeserializeObject(html);
                    foreach (var i in jo["data"])
                    {
                        result += (int)i["uid"] + "：" + (string)i["station"]["name"] + "\r\n";
                        station_count++;
                    }
                    return At(qq) +
                        "\r\n共找到" + station_count + "个监测站：" +
                        "\r\n" + result +
                        "\r\n使用指令“空气质量”加监测站编号查看数据";
                }
                catch (Exception err)
                {
                    string aa = err.Message.ToString();
                    return At(qq) +
                        "\r\n机器人在查找监测站时爆炸了，原因：" + aa;
                }
            }
        }

        /// <summary>
        /// 获取xml存储的数值
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static int GetXmlNumber(string xml,string info)
        {
            string resultStr = XmlSolve.xml_get(xml, info);
            int result = 0;
            try
            {
                result = int.Parse(resultStr);
            }
            catch { }
            return result;
        }

        /// <summary>
        /// 获取xml存储的字符串
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static string GetXmlString(string xml, string info)
        {
            return XmlSolve.xml_get(xml, info);
        }

        /// <summary>
        /// 将xml值增加指定数值
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="info"></param>
        /// <param name="add"></param>
        public static int AddXmlNumber(string xml,string info,int add)
        {
            string resultStr = XmlSolve.xml_get(xml, info);
            int result = 0;
            try
            {
                result = int.Parse(resultStr);
            }
            catch { }
            result += add;
            XmlSolve.del(xml, info);
            XmlSolve.insert(xml, info, result.ToString());
            return result;
        }


        /// <summary>
        /// 将xml值更改为指定值
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="info"></param>
        /// <param name="add"></param>
        public static void SetXmlString(string xml, string info, string str)
        {
            XmlSolve.del(xml, info);
            XmlSolve.insert(xml, info, str);
        }



        /// <summary>
        /// 获取网址前缀
        /// </summary>
        /// <returns></returns>
        public static string GetUrl()
        {
            string url = XmlSolve.ReplayGroupStatic("common", "javliburl");//获取保存的url
            return url;
        }

        /// <summary>
        /// 获取番号详情
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetAVInfo(string id)
        {
            string result = "";
            string html = HttpGet(GetUrl() + "vl_searchbyid.php", "keyword=" + id);
            Console.WriteLine(html);
            if (html.IndexOf("搜寻提示") != -1)
                return "无此影片，请重试";
            if (html.IndexOf("识别码搜寻结果") != -1)
            {
                string urlTemp = Reg_get(html, "<a href=\"\\./.v=javli(?<url>.*?)\" title=", "url");
                html = HttpGet(GetUrl(), "v=javli" + urlTemp);
            }
            if (html.Length == 0)
                return "加载失败";

            //匹配磁链
            string magnet = Reg_get(html,
                            "xt=urn:btih:(?<seed>........................................?)", "seed");
            //标题
            string title = Reg_get(html,
                            "<title>(?<title>.*?) - JAVLibrary", "title");
            //发行日期
            string time = Reg_get(html,
                            "\"text\">(?<time>\\d\\d\\d\\d-\\d\\d-\\d\\d?)</td>", "time");
            //封面图片地址
            string pic = Reg_get(html,
                            "video_jacket_img\" src=\"//(?<pic>.*?)\"", "pic");
            //视频时长
            string len = Reg_get(html,
                            "class=\"text\">(?<len>\\d*?)</span> 分钟", "len");
            //导演
            string director = Reg_get(html,
                            "\" rel=\"tag\">(?<director>.*?)</a> &nbsp;<span id=\"director", "director");
            //制片厂家
            string maker = Reg_get(html,
                            "\" rel=\"tag\">(?<maker>.*?)</a> &nbsp;<span id=\"maker", "maker");
            //发行商
            string label = Reg_get(html,
                            "\" rel=\"tag\">(?<label>.*?)</a> &nbsp;<span id=\"label", "label");
            //标签类别
            string tags = Reg_get_all(html,
                            "rel=\"category tag\">(?<tag>.*?)</a></span>", "tag", ",");
            //演员
            string act = Reg_get_all(html,
                            "vl_star.php.s=.....\" rel=\"tag\">(?<act>.*?)</a></span>", "act", ",");
            if (title == "")
                return "运行错误";
            result = "标题：\r\n" + title + "\r\n发行日期：" + time + "\r\n视频时长：" + len + "分种" +
                "\r\n导演：" + director + "\r\n制作商：" + maker + "\r\n发行商：" + label +
                "\r\n类型：" + tags + "\r\n演员：" + act;
            if (magnet != "")
                result += "\r\n"+magnet;
            result += "\r\n封面：http://" + pic;

            Bitmap bmp = new Bitmap(800, 160);
            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(Brushes.White, new Rectangle() { X = 0, Y = 0, Height = 160, Width = 800 });
            Font font = new Font("宋体", 9);
            g.DrawString(result, font, Brushes.Black, new PointF() { X = 0, Y = 0 });
            bmp.Save(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "data/image/javlib"+ id + ".png");

            return "[CQ:image,file=javlib" + id + ".png]";
        }


        /// <summary>
        /// 获取所有qq群的列表
        /// </summary>
        /// <returns>ArrayList列表</returns>
        public static ArrayList GetGroupList()
        {
            ArrayList list = new ArrayList();
            DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "data/");
            FileSystemInfo[] files = dir.GetFileSystemInfos();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i] as FileInfo;
                //是文件
                if (file != null)
                {
                    string group = file.Name.ToUpper().Replace(".XML", "");
                    string pattern = @"^[0-9]+$";
                    Match match = Regex.Match(group, pattern);
                    if (match.Success && group.Length > 6)
                    {
                        list.Add(group);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 统计字符串中某字符出现的次数
        /// </summary>
        /// <param name="str"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static int CharNum(string str, string search)
        {
            string[] resultString = Regex.Split(str, search, RegexOptions.IgnoreCase);
            return resultString.Length;
        }

        /// <summary>
        /// 运行lua脚本
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string RunLua(string text,string headRun = "")
        {
            LuaTimeout lua = new LuaTimeout();
            lua.code = text;
            lua.headRun = headRun;
            lua.CallWithTimeout(8000);
            return lua.result;
        }

        /// <summary>
        /// lua读取数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="qq"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string LuaGetXml(string qq, string name)
        {
            return GetXmlString("luaData", qq + "." + name);
        }

        /// <summary>
        /// lua存储数据
        /// </summary>
        /// <param name="qq"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static void LuaSetXml(string qq, string name, string str)
        {
            SetXmlString("luaData", qq + "." + name, str);
        }
    }


    /// <summary>
    /// 带超时处理的lua运行类
    /// </summary>
    class LuaTimeout
    {
        public string result = "lua脚本运行超时，请检查代码";
        public string code;
        public string headRun = "";

        public void Run(string code)
        {
            using (NLua.Lua lua = new NLua.Lua())
            {
                lua["lua_run_result_var"] = "";
                try
                {
                    lua.RegisterFunction("httpGet_row", null, typeof(Tools).GetMethod("HttpGet"));
                    lua.RegisterFunction("httpPost_row", null, typeof(Tools).GetMethod("HttpPost"));
                    lua.RegisterFunction("setData_row", null, typeof(Tools).GetMethod("LuaSetXml"));
                    lua.RegisterFunction("getData_row", null, typeof(Tools).GetMethod("LuaGetXml"));
                    lua.RegisterFunction("fileDownload", null, typeof(Tools).GetMethod("FileDownload"));
                    lua.RegisterFunction("getImg", null, typeof(LuaApi).GetMethod("GetBitmap"));
                    lua.RegisterFunction("setImgText", null, typeof(LuaApi).GetMethod("PutText"));
                    lua.RegisterFunction("putImgBlock", null, typeof(LuaApi).GetMethod("putBlock"));
                    lua.RegisterFunction("setImgImage", null, typeof(LuaApi).GetMethod("setImage"));
                    lua.RegisterFunction("getImgDir", null, typeof(LuaApi).GetMethod("GetDir"));
                    lua.RegisterFunction("getPath", null, typeof(LuaApi).GetMethod("GetPath"));
                    lua.DoFile(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "lua/head.lua");
                    lua.DoString(Encoding.UTF8.GetBytes(headRun));
                    lua.DoString(Encoding.UTF8.GetBytes(code));
                    if (Tools.CharNum(lua["lua_run_result_var"].ToString(), "\n") > 40 ||
                        Tools.CharNum(lua["lua_run_result_var"].ToString(), "\r") > 40)
                        result = "行数超过了20行，限制一下吧";
                    else if (lua["lua_run_result_var"].ToString().Length > 2000)
                        result = "字数超过了2000，限制一下吧";
                    else
                        result = lua["lua_run_result_var"].ToString();
                }
                catch (Exception e)
                {
                    string err = e.Message;
                    int l = err.IndexOf("lua/");
                    if (l >= 0)
                        err = err.Substring(l);
                    result = "代码崩掉啦\r\n" + err;
                }
            }
        }

        public void CallWithTimeout(int timeoutMilliseconds)
        {
            Thread threadToKill = null;
            Action wrappedAction = () =>
            {
                threadToKill = Thread.CurrentThread;
                Run(code);
            };

            IAsyncResult result = wrappedAction.BeginInvoke(null, null);
            if (result.AsyncWaitHandle.WaitOne(timeoutMilliseconds))
            {
                wrappedAction.EndInvoke(result);
            }
            else
            {
                threadToKill.Abort();
            }
        }
    }
}
