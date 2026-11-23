using HtmlAgilityPack;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace HttpNewsPAT
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Debug.Listeners.Add(new TextWriterTraceListener("trace.log"));
            Debug.AutoFlush = true;

            Cookie Token = SingIn("user", "user");
            GetContent(Token);

            Console.Read();
        }

        public static Cookie SingIn(string Login, string Password)
        {
            string url = "http://news.permaviat.ru/ajax/login.php";
            Debug.WriteLine($"Выполняем запрос: {url}");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = new CookieContainer();
            string postData = $"login={Login}&password={Password}";
            byte[] Data = Encoding.ASCII.GetBytes(postData);
            request.ContentLength = Data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(Data, 0, Data.Length);
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Debug.WriteLine($"Статус выполнения: {response.StatusCode}");

            return response.Cookies["token"];
        }

        public static void GetContent(Cookie Token)
        {
            string url = "http://news.permaviat.ru/main";
            Debug.WriteLine($"Выполняем запрос: {url}");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(Token);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseFromServer = new StreamReader(response.GetResponseStream()).ReadToEnd();

            ParsingHtml(responseFromServer);
        }

        public static void ParsingHtml(string htmlCode)
        {
            var html = new HtmlDocument();
            html.LoadHtml(htmlCode);
            var Document = html.DocumentNode;
            IEnumerable DivsNews = Document.Descendants(0).Where(n => n.HasClass("news"));
            foreach (HtmlNode DivNews in DivsNews)
            {
                var src = DivNews.ChildNodes[1].GetAttributeValue("src", "none");
                var name = DivNews.ChildNodes[3].InnerText;
                var description = DivNews.ChildNodes[5].InnerText;
                Console.WriteLine(name + "\n" + "Изображение: " + src + "\n" + "Описание: " + description + "\n");
            }
        }
    }
}
