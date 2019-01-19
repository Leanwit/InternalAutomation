using System;
using System.Collections.Generic;
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
        public void Init(string Email, string Password, List<TimecampItem> timecampItems, string url)
        {
            if (timecampItems == null && timecampItems.Count == 0)
            {
                return;
            }


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
                    selenium.FindElement(By.XPath("//table[@class='table table-bordered table-hover']"), 100);
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
                            newProject.Button = td.FindElements(By.TagName("a"))[1];
                            newProject.IdElement = td.FindElements(By.TagName("a"))[1].GetAttribute("data-id");
                        }

                        elementCount++;
                    }

                    listProject.Add(newProject);
                    js.ExecuteScript("arguments[0].style='background-color: gray;'", tr);
                }


                listProject = listProject.OrderBy(lp => lp.Project).ToList();

                List<TimecampItem> histories = new List<TimecampItem>();
                using (StreamReader sr = new StreamReader("TimeEntries.txt", false))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] words = line.Split(';');
                        histories.Add(new TimecampItem()
                        {
                            Project = words[1],
                            Task = words[2],
                            Activity = words[3],
                            Comment = words[0]
                        });
                    }
                }

                TimecampItem history = new TimecampItem();
                //Complete timecamp without data
                foreach (TimecampItem entry in timecampItems)
                {
                    if (entry.Project == null && histories.Exists(h => h.Comment.Equals(entry.Comment)))
                    {
                        history = histories.FindLast(h => h.Comment.Equals(entry.Comment));
                        Console.WriteLine($"Exist and comment '{entry.Comment}'");
                        Console.WriteLine(
                            $"{history.Project} - {history.Task} - {history.Activity} - {history.Comment}");
                        Console.WriteLine($"0-No | 1-Yes");
                        string option = Console.ReadLine();
                        if (option.Equals("1"))
                        {
                            entry.Project = history.Project;
                            entry.Task = history.Task;
                            entry.Activity = history.Activity;
                        }
                    }

                    if (entry.Project == null)
                    {
                        var webProjectObject = GetProjectValue(listProject, entry);
                        entry.Project = webProjectObject.Project;
                        entry.Task = webProjectObject.Task;
                        Console.Clear();
                    }

                    entry.Activity = InternalHelper.GetPredictedActivityValue(entry.Comment);

                    if (entry.Activity == null)
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

                        histories.Add(new TimecampItem()
                        {
                            Project = entry.Project,
                            Task = entry.Task,
                            Activity = entry.Activity,
                            Comment = entry.Comment
                        });
                    }
                }

                foreach (TimecampItem entry in timecampItems)
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

                        selenium.FindElement(By.XPath("//a[@data-id='" + test.IdElement + "']"), 50).Click();
                        Thread.Sleep(5000);
                        selenium.FindElement(By.XPath("//input[@name='WorkedHourDate']"), 50).Clear();
                        selenium.FindElement(By.XPath("//input[@name='WorkedHourDate']"), 50)
                            .SendKeys(myDate.ToString("dd/MM/yyyy"));
                        selenium.FindElement(By.XPath("//input[@name='WorkedHourDate']"), 50).SendKeys(Keys.Enter);

                        selenium.FindElement(By.XPath("//input[@name='Amount']"), 50).Clear();
                        selenium.FindElement(By.XPath("//input[@name='Amount']"), 50).SendKeys(entry.Time);
                        selenium.FindElement(By.XPath("//input[@name='Description']"), 50).SendKeys(entry.Comment);

                        if (!string.IsNullOrWhiteSpace(entry.Ticket))
                        {
                            selenium.FindElement(
                                By.XPath("//span[@class='select2-selection select2-selection--single']"), 50).Click();
                            IWebElement TicketSelect =
                                selenium.FindElement(By.XPath("//input[@class='select2-search__field']"), 50);
                            TicketSelect.SendKeys(entry.Ticket);
                            Thread.Sleep(10000);
                            TicketSelect.SendKeys(Keys.Enter);
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


        private static WebProject GetProjectValue(List<WebProject> newProject, TimecampItem entry)
        {
            WebProject projectValue;
            Console.WriteLine($"Complete Project for '{entry.Comment}'");
            int count = 0;
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
                    options.TryGetValue(value, out projectValue);
                    if (projectValue != null)
                    {
                        return projectValue;
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

        private static string GetTaskValue(List<WebProject> newProject, TimecampItem entry)
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