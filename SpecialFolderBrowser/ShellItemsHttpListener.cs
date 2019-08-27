//Sample: displaying contents of Windows Shell special folders (such as My Computer, Control Panel etc.) in WPF WebBrowser
// *** Helper web server class (HttpListener wrapper) ***
//Copyright (c) 2019, MSDN.WhiteKnight

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    public class ShellItemsHttpListener //возвращает содержимое каталога в виде веб-страницы
    {
        Dictionary<string, byte[]> images; //таблица изображений
        Dictionary<string, Uri> redirects; //таблица редиректов
        string html;
        string urlprefix;
        int httpport;
        HttpListener listener;
        Task task=null;

        public ShellItemsHttpListener(List<ShellItem> items, string prefix, int port)
        {
            StringBuilder sb = new StringBuilder();
            int n = 1;

            //генерируем веб-страницу...
            //устанавливаем режим IE8, чтобы работали стили
            sb.Append("<html><head><meta charset=\"utf-8\"><meta http-equiv=\"X-UA-Compatible\" content=\"IE=8\" /><style>");

            //устанавливаем стили, имитирующий внешний вид проводника (в режиме просмотра "Список")
            sb.Append("a { color: black; text-decoration: none;  } a:hover { color: black; text-decoration: underline;} ");
            sb.Append("tr:hover { background-color: lightblue;} img {border:none;}");
            sb.Append("</style></head><body>");

            //формируем список элементов
            sb.Append("<table cellspacing=\"0\" border=\"0\">");
            images = new Dictionary<string, byte[]>();
            redirects = new Dictionary<string, Uri>();

            foreach (ShellItem item in items)
            {
                sb.Append("<tr><td><a href=\"" + n.ToString() + ".html\" >");
                sb.Append("<img width=\"20\" height=\"20\" src=\"" + n.ToString() + ".png\"/></a></td>");
                sb.Append("<td><a href=\"" + n.ToString() + ".html\" >");
                sb.Append(item.DisplayName + "</a></td></tr>");
                images[n.ToString() + ".png"] = item.Image;
                redirects[n.ToString() + ".html"] = item.Path;
                n++;
            }
            sb.Append("</table></body></html>");
            html = sb.ToString();

            //инициализация HttpListener...
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:" + port.ToString() + "/" + prefix + "/");
            urlprefix = prefix;
            httpport = port;
        }

        public void Stop()
        {
            if (task == null) return;
            listener.Stop();
            task = null;
        }

        public void Start() //запускает HTTP-сервер     
        {
            if (task != null) return;

            task = Task.Run(() =>
            {
                try
                {
                    listener.Start();
                    while (true)
                    {
                        HttpListenerContext context = listener.GetContext(); //ожидаем запроса                        
                        HttpListenerRequest request = context.Request;
                        HttpListenerResponse response = context.Response;

                        //анализ URL...
                        byte[] buffer = new byte[0];
                        string urlbegin = "/" + urlprefix + "/";
                        string urlfile = request.RawUrl.Substring(urlbegin.Length);
                        if (urlfile == "" || urlfile.StartsWith("index") || urlfile.StartsWith("default"))
                        {
                            //отдаем основную страницу
                            buffer = System.Text.Encoding.UTF8.GetBytes(html);
                            response.ContentLength64 = buffer.Length;
                            response.ContentEncoding = Encoding.UTF8;
                        }
                        else if (urlfile.EndsWith(".png"))
                        {
                            //отдаем изображение
                            buffer = images[urlfile];
                            response.ContentLength64 = buffer.Length;
                            response.ContentType = "image/png";
                            response.AddHeader("Cache-Control", "no-store");
                        }
                        else
                        {
                            buffer = System.Text.Encoding.UTF8.GetBytes("Ошибка: Страница не найдена");
                            response.StatusCode = 404;
                            response.ContentType = "text/plain";
                        }

                        //возвращаем ответ
                        System.IO.Stream output = response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            });
        }

        public Uri ProcessRedirect(string inputurl)
        {
            //обработка перенаправлений...            
            string urlbegin = "http://localhost:" + httpport.ToString() + "/" + urlprefix + "/";

            if (inputurl.StartsWith(urlbegin))
            {
                string urlfile = inputurl.Substring(urlbegin.Length);
                if (!urlfile.EndsWith(".html")) return null;

                if (redirects.ContainsKey(urlfile))
                {
                    return redirects[urlfile];
                }
                else return null;
            }
            else return null;
        }
    }
}
