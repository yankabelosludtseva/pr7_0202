using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpNewsPAT
{
    internal class Class1
    {
        static void Main(string[] args)
        {
            Debug.Listeners.Add(new TextWriterTraceListener("trace.log"));
            Debug.AutoFlush = true;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string html = GetContent("https://habr.com/ru/articles/");
            ParsingHtml(html);

            Console.Read();
        }

        public static string GetContent(string url)
        {
            Debug.WriteLine($"Выполняем запрос: {url}");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Debug.WriteLine($"Статус выполнения: {response.StatusCode}");

            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public static void ParsingHtml(string htmlCode)
        {
            var html = new HtmlDocument();
            html.LoadHtml(htmlCode);
            var document = html.DocumentNode;

            // Все карточки статей
            var articles = document.Descendants("article").Where(n => n.HasClass("tm-articles-list__item"));

            foreach (HtmlNode article in articles)
            {
                var titleNode = article.Descendants("a").FirstOrDefault(n => n.HasClass("tm-title__link"));
                var authorNode = article.Descendants("a").FirstOrDefault(n => n.HasClass("tm-user-info__username"));

                // Блок с текстом статьи
                var bodyNode = article.Descendants("div").FirstOrDefault(n => n.HasClass("article-formatted-body"));
                // Счётчик просмотров
                var viewsNode = article.Descendants("span").FirstOrDefault(n => n.HasClass("tm-icon-counter__value"));

                if (titleNode != null)
                {
                    string title = WebUtility.HtmlDecode(titleNode.InnerText.Trim());
                    string link = "https://habr.com" + titleNode.GetAttributeValue("href", "");
                    string author = WebUtility.HtmlDecode(authorNode?.InnerText.Trim() ?? "Неизвестен");
                    string views = viewsNode?.InnerText.Trim() ?? "0";

                    string text = "Нет описания";
                    if (bodyNode != null)
                    {
                        text = WebUtility.HtmlDecode(bodyNode.InnerText.Trim());
                        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
                    }

                    Console.WriteLine($"Статья: {title}");
                    Console.WriteLine($"Автор: {author} | Просмотров: {views}");
                    Console.WriteLine($"Текст: {text}");
                    Console.WriteLine($"Ссылка: {link}");
                    Console.WriteLine();
                }
            }
        }

    }
}
