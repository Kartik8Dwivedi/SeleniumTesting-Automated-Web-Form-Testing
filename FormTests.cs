// This file is part of the Testing project
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;

namespace CloudQATest;

[TestFixture]
public class FormTests : IDisposable
{
    private IWebDriver driver;

    [SetUp]
    public void Setup(){
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");

        driver = new ChromeDriver(options);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(15);
        driver.Navigate().GoToUrl("http://app.cloudqa.io/home/AutomationPracticeForm");

        new WebDriverWait(driver, TimeSpan.FromSeconds(15))
            .Until(drv => drv.FindElement(By.TagName("body")));
    }

    [Test]
    public void TestFormFieldsResiliently()
    {
        Console.WriteLine("Testing text fields...");
        TestTextField("First Name", "Kartik");
        TestTextField("Last Name", "Dwivedi");
        TestTextField("Email", "kartikdwivedi2626@gmail.com");

        Console.WriteLine("Testing gender radio button...");
        TestRadioButton("Male");

        Console.WriteLine("Testing state dropdown...");
        TestDropdown("State", "India");

        Console.WriteLine("Testing Date of Birth input...");
        TestDateOfBirth("2002-05-26");
        
        var filePath = Path.Combine(AppContext.BaseDirectory, "DummyFiles", "Resume.pdf");
        Console.WriteLine($"Using file path: {filePath}");
        Console.WriteLine("Testing file upload...");
        TestFileUpload(filePath);

    }

    private void TestTextField(string fieldLabel, string testValue)
    {
        IWebElement inputField = FindInputByLabelText(fieldLabel);
        inputField.Clear();
        inputField.SendKeys(testValue);

        Assert.Multiple(() =>
        {
            Assert.That(inputField.Displayed, $"{fieldLabel} not displayed");
            Assert.That(inputField.Enabled, $"{fieldLabel} not enabled");
            Assert.That(inputField.GetAttribute("value"), Is.EqualTo(testValue), $"{fieldLabel} value mismatch");
        });

        Console.WriteLine($"{fieldLabel} field passed with value: {testValue}");
    }

    private void TestRadioButton(string value){
        var radioInput = driver.FindElement(By.CssSelector($"input[type='radio'][value='{value}']"));
        radioInput.Click();

        Assert.That(radioInput.Selected, Is.True, $"Radio button '{value}' was not selected");
        Console.WriteLine($"Radio button '{value}' selected successfully.");
    }

    private void TestDropdown(string labelText, string visibleOption){
        var label = driver.FindElement(By.XPath($"//label[contains(., '{labelText}')]"));
        string selectId = label.GetAttribute("for");

        var dropdown = driver.FindElement(By.Id(selectId));
        var select = new SelectElement(dropdown);

        select.SelectByText(visibleOption);

        Assert.That(select.SelectedOption.Text, Is.EqualTo(visibleOption), $"{labelText} dropdown selection mismatch");
        Console.WriteLine($"{labelText} dropdown selected option: {visibleOption}");
    }

    private void TestDateOfBirth(string dateValue){
        // Placeholder: "YYYY-MM-DD", not a date picker
        var input = FindInputByLabelText("Date of Birth");
        input.Clear();
        input.SendKeys(dateValue);

        Assert.That(input.GetAttribute("value"), Is.EqualTo(dateValue), "Date of Birth value mismatch");
        Console.WriteLine($"Date of Birth field set to: {dateValue}");
    }

    private IWebElement FindInputByLabelText(string labelText){
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        IWebElement label = wait.Until(drv =>
            drv.FindElement(By.XPath($"//label[contains(., '{labelText}')]")));

        string inputId = label.GetAttribute("for");
        if (!string.IsNullOrEmpty(inputId)){
            return wait.Until(drv => drv.FindElement(By.Id(inputId)));
        }

        try{
            return label.FindElement(By.XPath(".//input"));
        }
        catch (NoSuchElementException){
            throw new NotFoundException($"Input field for '{labelText}' not found");
        }
    }


    private void TestFileUpload(string filePath){
        // Finding the input[type="file"]
        var fileInput = driver.FindElement(By.CssSelector("input[type='file'][name='pic']"));

        fileInput.SendKeys(filePath);

        Assert.That(fileInput.GetAttribute("value"), Does.Contain(System.IO.Path.GetFileName(filePath)), "File upload failed");

        Console.WriteLine($"File uploaded successfully: {filePath}");
    }

    public void Dispose()
    {
        driver?.Quit();
        driver?.Dispose();
    }
}
