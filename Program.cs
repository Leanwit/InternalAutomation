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

        var methodTimeEntry = Config.GetSection("MethodTimeEntry");
        List<InternalItem> intenalItems = new List<InternalItem>();
        if (methodTimeEntry["Timecamp"].Equals("true"))
        {
            intenalItems =
                new TimecampManager("2019-01-18", "2019-01-18", GetTimecampConfig("Token")).GetInfoAsync().Result;
        }

        if (methodTimeEntry["Text"].Equals("true"))
        {
            var entriesTextManager = new EntriesTextManager();
            intenalItems = entriesTextManager.InternalItem;
        }

        new InternalAutomation().Init(GetInternalConfig("Email"), GetInternalConfig("Password"), intenalItems,
            GetInternalConfig("Url"));
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