using OpenQA.Selenium.Chrome;

namespace Automation
{
    public abstract class Base
    {
        protected ChromeOptions ChromeOptions = new ChromeOptions();
        protected string ChromeDriverFolder = @"resources/";
    }

}