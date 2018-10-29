using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace Automation
{
    public class SeleniumHelper
    {
        private IWebDriver Driver { get; set; }
        private IJavaScriptExecutor Js;
        public SeleniumHelper(IWebDriver driver)
        {
            this.Driver = driver;
            this.Js = driver as IJavaScriptExecutor;
        }
        public IWebElement FindElement(By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = GetWait(this.Driver, timeoutInSeconds);
                return wait.Until(drv => drv.FindElement(by));
            }

            return this.Driver.FindElement(by);
        }

        private static WebDriverWait GetWait(IWebDriver driver, int timeoutInSeconds) => new WebDriverWait(driver, timeout: TimeSpan.FromSeconds(timeoutInSeconds));

        public void GoogleLogin(string email, string password)
        {
            this.FindElement(By.XPath("//input[@class='whsOnd zHQkBf']"), 5).SendKeys(email);
            this.FindElement(By.XPath("//div[@id='identifierNext']"), 5).Click();
            Thread.Sleep(3000);

            this.FindElement(By.XPath("//input[@class='whsOnd zHQkBf']"), 5).SendKeys(password);
            this.FindElement(By.XPath("//div[@id='passwordNext']"), 5).Click();
            Thread.Sleep(4000);
        }

        internal System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> FindElements(By by, int v)
        {
            if (v > 0)
            {
                var wait = GetWait(this.Driver, v);
                return wait.Until(drv => drv.FindElements(by));
            }

            return this.Driver.FindElements(by);
        }

        internal void Paint(IWebElement item)
        {
            this.Js.ExecuteScript("arguments[0].style='background-color: red;'", args: item);
        } 
    }
}