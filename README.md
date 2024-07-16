# FatsharkCodingTest

When I first saw the test, I noticed there were a lot of areas that I had not worked with before, such as SQLite and API calls. This made the task feel quite daunting but I decided to do everything one at a time, which helped out. Because of my new understanding of the areas I was not sure what was good and bad practices, especially with WPF. This became an enjoyable learning experience once I managed to piece the different puzzle pieces together.

I chose to do the test in JetBrains Rider because I'm more used to it from my previous job experience. Now after doing the test, I would have done it in Visual Studio because they seem to have a lot more tools for WPF.

One of the biggest problems I encountered was linking commands to the data grid events. After a lot of searching, I found a way that I felt was the cleanest. I spent some time figuring out and testing ideas for how to display the largest groups of people close to each other. I started with a heatmap but couldn't make the background map match the coordinates properly. So then I decided to just choose a clustering algorithm called K-Means which was quite easy to implement and gave decent results.

I decided to display most of my data via plots because I felt that analyzing data without any visuals was quite boring. Some of my time went into figuring out which plotting library fitted the needs of the program the best and ended up with OxyPlot.
I feel like I learned a lot of new things from this test which made the experience very valuable and fun. I would greatly appreciate feedback on how I could improve, especially in the WPF area.
