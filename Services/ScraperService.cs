using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        var url = "https://wadowice24.pl/kultura/wadowice-juz-zapowiedzialy-dozynki-gdzie-i-kiedy-w-tym-roku/";
        
        // Konfiguracja Chrome
        var options = new ChromeOptions();
        options.AddArgument("--headless"); // Uruchom w tle bez okna
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        
        IWebDriver driver = new ChromeDriver(options);
        
        try
        {
            Console.WriteLine("Ładowanie strony...");
            driver.Navigate().GoToUrl(url);
            
            // Czekamy na załadowanie Disqus
            Console.WriteLine("Czekam na załadowanie komentarzy Disqus...");
            Thread.Sleep(5000); // Czekamy 5 sekund na załadowanie JavaScript
            
            // Próbujemy znaleźć iframe Disqus
            try
            {
                var disqusFrame = driver.FindElement(By.Id("disqus_thread"));
                Console.WriteLine("Znaleziono kontener Disqus!");
                
                // Szukamy iframe wewnątrz kontenera Disqus
                var iframes = driver.FindElements(By.TagName("iframe"));
                Console.WriteLine($"Znaleziono {iframes.Count} iframe(s)");
                
                foreach (var iframe in iframes)
                {
                    try
                    {
                        driver.SwitchTo().Frame(iframe);
                        
                        // Używamy selektorów z Twojego przykładu
                        var comments = driver.FindElements(By.CssSelector("div[data-role='post-content']"));
                        
                        if (comments.Count > 0)
                        {
                            Console.WriteLine($"Znaleziono {comments.Count} komentarzy!");
                            
                            foreach (var comment in comments)
                            {
                                try
                                {
                                    // Wyciągamy autora
                                    var authorElement = comment.FindElement(By.CssSelector("span.author"));
                                    string author = authorElement.Text.Trim();

                                    // Wyciągamy treść komentarza
                                    var messageElement = comment.FindElement(By.CssSelector("div.post-message"));
                                    string message = messageElement.Text.Trim();

                                    // Wyświetlamy wynik
                                    Console.WriteLine($"Autor: {author}");
                                    Console.WriteLine($"Komentarz: {message}");
                                    Console.WriteLine(new string('-', 50));
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Błąd przy przetwarzaniu komentarza: {ex.Message}");
                                }
                            }
                            break; // Znaleźliśmy komentarze, kończymy
                        }
                        
                        driver.SwitchTo().DefaultContent(); // Wracamy do głównej strony
                    }
                    catch (Exception ex)
                    {
                        driver.SwitchTo().DefaultContent(); // Upewniamy się, że wracamy do głównej strony
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

                            Console.WriteLine($"Autor: {author}");
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
                    
                    // Debug - wyświetlamy dostępne elementy
                    var allDivs = driver.FindElements(By.TagName("div"));
                    Console.WriteLine($"Znaleziono {allDivs.Count} elementów div na stronie");
                    
                    var disqusElements = driver.FindElements(By.CssSelector("*[id*='disqus'], *[class*='disqus']"));
                    Console.WriteLine($"Znaleziono {disqusElements.Count} elementów związanych z Disqus");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ogólny błąd: {ex.Message}");
        }
        finally
        {
            driver.Quit();
            Console.WriteLine("\nNaciśnij Enter aby zakończyć...");
            Console.ReadLine();
        }
    }
}
