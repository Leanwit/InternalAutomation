using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Manager;
using Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Util;
using Automation;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;

public class Program
{
    public static IConfiguration Config;
    static void Main(string[] args)
    {
        Console.WriteLine(Directory.GetCurrentDirectory());

        Config = new ConfigurationBuilder()
          .AddJsonFile("appsettings.json", true, true)
          .Build();

        Console.WriteLine(Config.GetSection("Credentials").GetSection("Timecamp")["Password"]);

        List<TimecampItem> timecampItems = new TimecampManager("2018-10-23", "2018-10-28", GetTimecampConfig("Token")).GetInfoAsync().Result;
        new InternalAutomation().Init(GetInternalConfig("Email"), GetInternalConfig("Password"),timecampItems);

    }

    private static string GetInternalConfig(string key)
    {
        return Config.GetSection("Credentials").GetSection("Internal")[key];
    }

    private static string GetTimecampConfig(string key)
    {
        return Config.GetSection("Credentials").GetSection("Timecamp")[key];
    }
}

