using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using Manager.Util;
using Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Util;

namespace Automation
{
    public class InternalAutomation : Base
    {
        public void Init(string Email, string Password, List<InternalItem> internalItems, string url, bool isEnableTicket)
        {
            if (internalItems == null && internalItems.Count == 0)
            {
                return;
            }

            bool isActivityUse = internalItems.Exists(x => x.Activity != null);

            using (var driver = new ChromeDriver(Path.GetDirectoryName(this.ChromeDriverFolder), this.ChromeOptions))
            {
                SeleniumHelper selenium = new SeleniumHelper(driver);
                driver.Navigate().GoToUrl(url);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(200);
                selenium.FindElement(By.XPath("//*[@data-original-title='Log in using your Google account']"), 50)
                    .Click();
                selenium.GoogleLogin(Email, Password);

                Thread.Sleep(4000);

                IWebElement Element =
                    selenium.FindElement(By.XPath("//table[@class='table table-hover']"), 100);
                IWebElement Table = Element.FindElement(By.TagName("tbody"));
                IJavaScriptExecutor js = driver as IJavaScriptExecutor;

                List<WebProject> listProject = new List<WebProject>();

                foreach (var tr in Table.FindElements(By.TagName("tr")))
                {
                    js.ExecuteScript("arguments[0].style='background-color: red;'", tr);

                    WebProject newProject = new WebProject();

                    int elementCount = 0;
                    foreach (var td in tr.FindElements(By.TagName("td")))
                    {
                        if (elementCount == 1)
                        {
                            newProject.Project = td.Text;
                        }

                        if (elementCount == 2)
                        {
                            newProject.Task = td.Text;
                        }

                        if (elementCount == 7)
                        {
                            newProject.Button = td.FindElements(By.TagName("button"))[1];
                            newProject.IdElement = td.FindElements(By.TagName("button"))[1].GetAttribute("data-id");
                        }

                        elementCount++;
                    }

                    listProject.Add(newProject);
                    js.ExecuteScript("arguments[0].style='background-color: gray;'", tr);
                }


                listProject = listProject.OrderBy(lp => lp.Project).ToList();

                List<InternalItem> histories = new List<InternalItem>();
                using (StreamReader sr = new StreamReader("TimeEntries.txt", false))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] words = line.Split(';');
                        histories.Add(new InternalItem()
                        {
                            Project = words[1],
                            Task = words[2],
                            Activity = words[3],
                            Comment = words[0]
                        });
                    }
                }

                InternalItem history = new InternalItem();

                foreach (InternalItem entry in internalItems)
                {
                    if (entry.Project == null && histories.Exists(h => h.Comment.Equals(entry.Comment)))
                    {

                        if (isActivityUse)
                        {
                            history = histories.FindLast(h => h.Comment.Equals(entry.Comment) && h.Activity == entry.Activity);
                        }
                        else
                        {
                            history = histories.FindLast(h => h.Comment.Equals(entry.Comment));
                        }
                        
                        
                        if (history != null && listProject.Any(x => x.Project.Equals(history.Project)))
                        {
                            entry.Project = history.Project;
                            entry.Task = history.Task;
                            entry.Activity = history.Activity;
                        }
                    }
                    
                    if (entry.Project == null)
                    {
                        InternalItem aux = InternalHelper.GetPredictedProjectValue(entry);
                        entry.Project = aux.Project;
                        entry.Task = aux.Task;
                    }

                    if (entry.Project == null)
                    {
                        var webProjectObject = GetProjectValue(listProject, entry);

                        //If select skip option
                        if (string.IsNullOrEmpty(webProjectObject.Project))
                        {
                            continue;
                        }
                        
                        entry.Project = webProjectObject.Project;
                        entry.Task = webProjectObject.Task;
                        Console.Clear();
                    }


                    if (entry.Activity == null || string.IsNullOrEmpty(entry.Activity))
                    {
                        entry.Activity = InternalHelper.GetPredictedActivityValue(entry.Comment);
                    }

                    if (entry.Activity == null || string.IsNullOrEmpty(entry.Activity))
                    {
                        entry.Activity = InternalHelper.GetActivityValue(entry);
                        Console.Clear();
                    }


                    if (!(histories.Exists(h => h.Comment.Equals(entry.Comment)) &&
                          histories.Exists(h => h.Activity.Equals(entry.Activity)) &&
                          histories.Exists(h => h.Project.Equals(entry.Project)) &&
                          histories.Exists(h => h.Task.Equals(entry.Task))))
                    {
                        using (StreamWriter outputFile = File.AppendText("TimeEntries.txt"))
                        {
                            outputFile.WriteLine($"{entry.Comment};{entry.Project};{entry.Task};{entry.Activity}");
                        }

                        histories.Add(new InternalItem()
                        {
                            Project = entry.Project,
                            Task = entry.Task,
                            Activity = entry.Activity,
                            Comment = entry.Comment
                        });
                    }
                }

                //Remove all skip entries
                internalItems.RemoveAll(x => x.IsSkip);
                
                internalItems = GroupByPerDay(internalItems);

                foreach (InternalItem entry in internalItems)
                {
                    WebProject test = listProject.Find(l =>
                        l.Project.ToLower().Equals(entry.Project.ToLower()) &&
                        l.Task.ToLower().Equals(entry.Task.ToLower()));

                    if (test != null)
                    {
                        DateTime myDate = DateTime.ParseExact(entry.Date, "yyyy-MM-dd",
                            System.Globalization.CultureInfo.InvariantCulture);
                        //string timeDaily = (double.Parse(Time.GetInternalTime(entry.Time).Replace(".", ",")) / 2).ToString().Replace(",", ".");
                        string timeDaily = TimeHelper.GetInternalTime(entry.Time);

                        selenium.FindElement(By.XPath("//button[@data-id='" + test.IdElement + "']"), 50).Click();
                        Thread.Sleep(5000);
                        selenium.FindElement(By.XPath("//input[@name='WorkedHourDate']"), 50).Clear();
                        selenium.FindElement(By.XPath("//input[@name='WorkedHourDate']"), 50)
                            .SendKeys(myDate.ToString("dd/MM/yyyy"));
                        selenium.FindElement(By.XPath("//input[@name='WorkedHourDate']"), 50).SendKeys(Keys.Enter);

                        selenium.FindElement(By.XPath("//input[@name='Amount']"), 50).Clear();
                        selenium.FindElement(By.XPath("//input[@name='Amount']"), 50).SendKeys(entry.Time);
                        selenium.FindElement(By.XPath("//input[@name='Description']"), 50).SendKeys(entry.Comment);

                        if (!string.IsNullOrWhiteSpace(entry.Ticket) && isEnableTicket)
                        {
                            selenium.FindElement(
                                By.XPath("//span[@class='select2-selection select2-selection--single']"), 50).Click();
                            IWebElement TicketSelect =
                                selenium.FindElement(By.XPath("//input[@class='select2-search__field']"), 50);
                            TicketSelect.SendKeys(entry.Ticket);
                            Thread.Sleep(15000);
                            try
                            {
                                TicketSelect.SendKeys(Keys.Enter);
                            }
                            catch (Exception e)
                            {
                                Thread.Sleep(15000);
                                TicketSelect.SendKeys(Keys.Enter);
                            }
                        }

                        IWebElement ActivitySelect =
                            selenium.FindElement(By.XPath("//*[@id='addDashboardHoursInputActivities']"), 50);
                        ActivitySelect.Click();
                        ActivitySelect.FindElement(By.XPath(".//option[contains(text(),'" + entry.Activity + "')]"))
                            .Click();
                        selenium.FindElement(By.XPath("//button[@value='submit']"), 50).Click();
                    }
                    else
                    {
                        Console.WriteLine(string.Format(
                            "Project {0} doesn't exist - Task {1} - Activity {2} - Date {3}", entry.Project.ToLower(),
                            entry.Task, entry.Activity, entry.Date));
                    }
                }
            }
        }

        private List<InternalItem> GroupByPerDay(List<InternalItem> internalItems)
        {
            internalItems.ForEach(x => x.Time.Replace(".", ","));
            List<InternalItem> items = internalItems.GroupBy(c => new
                {
                    c.Date,
                    c.Project,
                    c.Task,
                    c.Comment,
                    c.Activity,
                    c.Ticket
                })
                .Select(gcs => new InternalItem()
                {
                    Date = gcs.Key.Date,
                    Project = gcs.Key.Project,
                    Activity = gcs.Key.Activity,
                    Task = gcs.Key.Task,
                    Comment = gcs.Key.Comment,
                    Ticket = gcs.Key.Ticket,
                    Time = gcs.Sum(g => double.Parse(g.Time)).ToString(),
                }).ToList();
            items.ForEach(x => x.Time.Replace(",", "."));
            return items;
        }


        private static WebProject GetProjectValue(List<WebProject> newProject, InternalItem entry)
        {
            WebProject projectValue;
            Console.WriteLine($"Complete Project for '{entry.Comment}'");
            Console.WriteLine($"0. Skip Project");

            int count = 1;
            Dictionary<int, WebProject> options = new Dictionary<int, WebProject>();
            foreach (var project in newProject)
            {
                Console.WriteLine($"{count}. {project.Project} - {project.Task}");
                options.Add(count, project);
                count++;
            }

            string option = Console.ReadLine();
            while (true)
            {
                try
                {
                    int value = Int16.Parse(option);
                    
                    //Skip option
                    if (value == 0)
                    {
                        return new WebProject();
                    }
                    options.TryGetValue(value, out projectValue);
                    if (projectValue != null)
                    {
                        return projectValue;
                    }
                }
                catch (System.Exception)
                {
                    Console.WriteLine("Error option");
                    option = Console.ReadLine();

                    continue;
                }

                option = Console.ReadLine();
            }
        }

        private static string GetTaskValue(List<WebProject> newProject, InternalItem entry)
        {
            string taskValue;
            Console.WriteLine($"Complete Task for '{entry.Comment}'");
            int count = 0;
            Dictionary<int, string> options = new Dictionary<int, string>();
            foreach (var project in newProject)
            {
                Console.WriteLine($"{count}. {project.Project} - {project.Task}");
                options.Add(count, project.Task);
                count++;
            }

            string option = Console.ReadLine();
            while (true)
            {
                try
                {
                    int value = Int16.Parse(option);
                    options.TryGetValue(value, out taskValue);
                    if (!string.IsNullOrEmpty(taskValue))
                    {
                        return taskValue;
                    }
                }
                catch (System.Exception)
                {
                    Console.WriteLine("Error option");
                    continue;
                }

                option = Console.ReadLine();
            }
        }
    }
}