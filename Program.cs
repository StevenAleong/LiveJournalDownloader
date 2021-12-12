// We can do all this beacuse there is no captcha on the login page
using PuppeteerSharp;

// You can find your starting and ending year by going to https://YOURUSERNAME.livejournal.com/calendar
var startingYear = 2004;
var maxYear = 2010;

var loginUrl = "https://www.livejournal.com/login.bml?returnto=https:%2F%2Fwww.livejournal.com%2Fexport.bml";

// Replace with your login information
var username = "REPLACE WITH YOUR USERNAME";
var password = "REPLACE WITH YOUR PASSWORD";

// Where we downloading from.
var exportUrl = $"https://www.livejournal.com/export.bml";

// Lets go
Console.WriteLine("Starting the headless browser");
await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

var browser = await Puppeteer.LaunchAsync(new LaunchOptions {
    Headless = true
});

var page = await browser.NewPageAsync();

// LOGIN ------------------------------------------
Console.WriteLine("Loading up the login page");

// Go to the login page
await page.GoToAsync(loginUrl);

// Fill the username and password
await page.TypeAsync("#user", username);
await page.TypeAsync("#lj_loginwidget_password", password);

// Submit the form
await page.Keyboard.PressAsync("Enter");

// Wait for the redirect to export page
await page.WaitForNavigationAsync();

// EXPORT ------------------------------------------
// Change to XML format
await page.SelectAsync($"select[name='format']", "xml");

// now go through each month year
for (var year = startingYear; year <= maxYear; year++) {
    var currentYearContent = new System.Text.StringBuilder();

    // Clear out the input and replace with the current year
    await page.FocusAsync("input[name='year']");
    for (var i = 0; i < 4; i++)
    {
        await page.Keyboard.PressAsync("Backspace");
    }
    await page.TypeAsync($"input[name='year']", year.ToString());
    
    for(var month = 1; month <= 12; month++)
    {

        // Clear out the input and replace with the current month
        await page.FocusAsync("input[name='month']");
        for (var i = 0; i < 2; i++)
        {
            await page.Keyboard.PressAsync("Backspace");
        }
        await page.TypeAsync("input[name='month']", month.ToString());

        Console.WriteLine($"Checking {year}-{month}");

        //await page.ScreenshotAsync($"./export-page-{year}-{month}.png");

        // Submit the form
        await page.ClickAsync("input[type='submit']");

        // Wait for page navigation isn't working for some reason =/
        await page.WaitForTimeoutAsync(10000);

        //await page.ScreenshotAsync($"./xml-{year}-{month}.png");

        // Wait for the redirect to results page
        //await page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = new WaitUntilNavigation[] { WaitUntilNavigation.Load, WaitUntilNavigation.Networkidle2 } });

        // Read xml content
        var text = await page.EvaluateExpressionAsync<string>("document.querySelector('livejournal').innerHTML");

        text = text.Replace("<?xml version=\"1.0\" encoding='utf-8'?>", "").Replace("<livejournal>", "").Replace("</livejournal>", "");
        currentYearContent.Append(text);

        // Go back one page to return to the export page.
        await page.GoBackAsync();

        // Wait for page navigation isn't working for some reason =/
        await page.WaitForTimeoutAsync(10000);
    }

    currentYearContent.Insert(0, "<livejournal>").Append("</livejournal>");

    Console.WriteLine($"Saving {username}-{year}.txt");
    await File.WriteAllTextAsync($"./{username}-{year}.txt", currentYearContent.ToString());
}

// Screenshot it for testing
//await page.ScreenshotAsync("./export.png");