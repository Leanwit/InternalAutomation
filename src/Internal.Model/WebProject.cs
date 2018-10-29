using OpenQA.Selenium;

namespace Model
{
    public class WebProject
    {
        public string Project { get; set; }
        public string Task { get; set; }

        public IWebElement Button { get; set; }

        public string IdElement { get; set; }

        public WebProject()
        {

        }

    }
}