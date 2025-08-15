using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        var todaysArticles = GetTodaysArticles("https://wadowice24.pl/najnowsze/");
        foreach (var article in todaysArticles)
        {
            ScrapeComments(article);
        }

    }

    public static void ScrapeComments(string url)
    {
        // Konfiguracja Chrome
        var options = new ChromeOptions();
        options.AddArgument("--headless"); // Uruchom w tle bez okna
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-plugins");
        options.AddArgument("--disable-images");
        options.AddArgument("--disable-javascript-harmony-shipping");
        options.AddArgument("--disable-logging");
        options.AddArgument("--disable-login-animations");
        options.AddArgument("--disable-notifications");
        options.AddArgument("--no-default-browser-check");
        options.AddArgument("--no-first-run");
        options.AddArgument("--disable-default-apps");
        options.AddArgument("--disable-popup-blocking");
        options.AddArgument("--disable-translate");
        options.AddArgument("--disable-background-timer-throttling");
        options.AddArgument("--disable-renderer-backgrounding");
        options.AddArgument("--disable-backgrounding-occluded-windows");
        options.AddArgument("--disable-ipc-flooding-protection");
        options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        options.AddArgument("--log-level=3");
        options.AddArgument("--silent");

        var service = ChromeDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;
        service.SuppressInitialDiagnosticInformation = true;

        IWebDriver driver = new ChromeDriver(service, options);

        try
        {
            driver.Navigate().GoToUrl(url);
            Console.WriteLine("Ladowanie stronki");
            Thread.Sleep(5000);

            try
            {
                // szukamy kontenerow z komentarzami
                var disqusFrame = driver.FindElement(By.Id("disqus_thread"));
                var iframes = driver.FindElements(By.TagName("iframe"));


                foreach (var iframe in iframes)
                {
                    try
                    {
                        driver.SwitchTo().Frame(iframe);


                        var comments = driver.FindElements(By.CssSelector("div[data-role='post-content']"));

                        if (comments.Count > 0)
                        {
                            Console.WriteLine($"Znaleziono {comments.Count} komentarzy!");

                            foreach (var comment in comments)
                            {
                                try
                                {
                                    var authorElement = comment.FindElement(By.CssSelector("span.author"));
                                    string author = authorElement.Text.Trim();
                                    var messageElement = comment.FindElement(By.CssSelector("div.post-message"));
                                    string message = messageElement.Text.Trim();

                                    string timeAgo = "Brak czasu";
                                    try
                                    {
                                        var timeElement = comment.FindElement(By.CssSelector("a.time-ago"));
                                        timeAgo = timeElement.Text.Trim();
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            var timeElement = comment.FindElement(By.CssSelector(".post-meta a[data-role='relative-time']"));
                                            timeAgo = timeElement.Text.Trim();
                                        }
                                        catch
                                        {
                                            try
                                            {
                                                var timeElement = comment.FindElement(By.CssSelector("[data-role='relative-time']"));
                                                timeAgo = timeElement.Text.Trim();
                                            }
                                            catch
                                            {
                                                timeAgo = "Brak czasu";
                                            }
                                        }
                                    }

                                    // Wyświetlamy wynik
                                    Console.WriteLine($"Autor: {author}");
                                    Console.WriteLine($"Czas: {timeAgo}");
                                    Console.WriteLine($"Komentarz: {message}");
                                    Console.WriteLine(new string('-', 50));
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Błąd przy przetwarzaniu komentarza: {ex.Message}");
                                }
                            }
                            break;
                        }

                        driver.SwitchTo().DefaultContent();
                    }
                    catch (Exception ex)
                    {
                        driver.SwitchTo().DefaultContent();
                        Console.WriteLine($"Nie udało się przełączyć na iframe: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Nie znaleziono kontenera Disqus: {ex.Message}");
                Console.WriteLine("Próbuję alternatywne metody...");

                // Alternatywna metoda - szukamy bezpośrednio komentarzy
                var comments = driver.FindElements(By.CssSelector("div[data-role='post-content']"));

                if (comments.Count > 0)
                {
                    Console.WriteLine($"Znaleziono {comments.Count} komentarzy (metoda alternatywna)!");

                    foreach (var comment in comments)
                    {
                        try
                        {
                            var authorElement = comment.FindElement(By.CssSelector("span.author"));
                            string author = authorElement.Text.Trim();

                            var messageElement = comment.FindElement(By.CssSelector("div.post-message"));
                            string message = messageElement.Text.Trim();

                            // Wyciągamy czas dodania komentarza
                            string timeAgo = "Nieznany czas";
                            try
                            {
                                var timeElement = comment.FindElement(By.CssSelector("a.time-ago"));
                                timeAgo = timeElement.Text.Trim();
                            }
                            catch
                            {
                                try
                                {
                                    var timeElement = comment.FindElement(By.CssSelector(".post-meta a[data-role='relative-time']"));
                                    timeAgo = timeElement.Text.Trim();
                                }
                                catch
                                {
                                    try
                                    {
                                        var timeElement = comment.FindElement(By.CssSelector("[data-role='relative-time']"));
                                        timeAgo = timeElement.Text.Trim();
                                    }
                                    catch
                                    {
                                        timeAgo = "Nieznany czas";
                                    }
                                }
                            }

                            Console.WriteLine($"Autor: {author}");
                            Console.WriteLine($"Czas: {timeAgo}");
                            Console.WriteLine($"Komentarz: {message}");
                            Console.WriteLine(new string('-', 50));
                        }
                        catch (Exception commentEx)
                        {
                            Console.WriteLine($"Błąd przy przetwarzaniu komentarza: {commentEx.Message}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Nie znaleziono komentarzy.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ogólny błąd: {ex.Message}");
        }
    }


    public static List<string> GetTodaysArticles(string url)
    {
        var articles = new List<string>();

        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-plugins");
        options.AddArgument("--disable-images");
        options.AddArgument("--disable-javascript-harmony-shipping");
        options.AddArgument("--disable-logging");
        options.AddArgument("--disable-login-animations");
        options.AddArgument("--disable-notifications");
        options.AddArgument("--no-default-browser-check");
        options.AddArgument("--no-first-run");
        options.AddArgument("--disable-default-apps");
        options.AddArgument("--disable-popup-blocking");
        options.AddArgument("--disable-translate");
        options.AddArgument("--disable-background-timer-throttling");
        options.AddArgument("--disable-renderer-backgrounding");
        options.AddArgument("--disable-backgrounding-occluded-windows");
        options.AddArgument("--disable-ipc-flooding-protection");
        options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        options.AddArgument("--log-level=3");
        options.AddArgument("--silent");

        var service = ChromeDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;
        service.SuppressInitialDiagnosticInformation = true;

        try
        {
            using (var driver = new ChromeDriver(service, options))
            {
                driver.Navigate().GoToUrl(url);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                Thread.Sleep(3000);
                var articleElements = driver.FindElements(By.CssSelector("article.entry-card"));

                foreach (var article in articleElements)
                {
                    try
                    {
                        // sprawdzamy date
                        var timeElement = article.FindElement(By.CssSelector("time.ct-meta-element-date"));
                        var dateTimeAttr = timeElement.GetAttribute("datetime");
                        
                        if (!string.IsNullOrEmpty(dateTimeAttr) && DateTime.TryParse(dateTimeAttr, out var publishDate))
                        {
                            if (publishDate.Date == new DateTime(2025, 8, 14))
                            {
                                var titleElement = article.FindElement(By.CssSelector("h3.entry-title a"));
                                var link = titleElement.GetAttribute("href");

                                articles.Add(link);
                            }
                        }
                    }
                    catch (Exception articleEx)
                    {
                        Console.WriteLine($"Błąd przy przetwarzaniu artykułu: {articleEx.Message}");
                    }
                }

                Console.WriteLine($"Znaleziono {articles.Count} artykułów z dzisiejszą datą");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas pobierania artykułów: {ex.Message}");
        }

        return articles;
    }
}
