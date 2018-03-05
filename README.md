# TGC-Ripper

This is a ripper for the great courses plus made on C#

# Requisites

- You will need a valid subscription
- You will need to get Selenium WebDriver from the nuget packages
- Put [aria2c](https://aria2.github.io/) and [chromedriver](https://sites.google.com/a/chromium.org/chromedriver/downloads) binaries in the same folder where you compiled this program.
- [Export your cookies](https://chrome.google.com/webstore/detail/cookiestxt/njabckikapfpffapmjgojcnbfjonfjfg) (cookies.txt) from the TGCP session and the links (links.txt) of courses and put them in the same folder where you compiled this program.

# How it works

It grabs all info from the course you want using selenium webdriver.
It proceeds to scrap every lecture and select the best stream based on the biggest size given by content-length.
Using aria2c, the program will save according to {Course Name}/Lecture {number} - {Lecture Title}