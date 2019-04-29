# run-octopus-deployed-executable
A little console utility that runs console applications deployed by Octopus Deploy software.

Built with .NET Framework 4.7

I deploy my web-based applications onto my servers using Octopus www.octopus.com. Many times I write little console applications that accompany my applications by performing forms of automated activity such as sending notifications, etc. 


I maintain these console applications within the same repository as the web applications with which they are associated which allows me to take advantage of automated deployment using Octopus.

I typically run these console applications using the built-in Task Scheduler service built into Windows which provides a challenge that this program is designed to solve.

The challenge is that in order to schedule a program to run using Windows Task Scheduler, it requires you to enter a static file path, which makes sense. However the problem is that the deployment of packages using Octopus happens in a dynamic manner, so the path the executable changes with each release. For example, here might be an example of the executable path you want to run:

`c:\Octopus\Applications\<Environment>\<VersionNumber>\<ExecutableFileName>`

or more practically:

`c:\Octopus\Applications\Production\1.2.3.4\MyAutomatedTask.exe`

This program serves as a proxy-runner, if you will, which will find the most recent Octopus deployment directory and run the executable. Instead of pointing the Task Scheduler job to point to the executable, you would point it to this program instead.

Documentation for this project doesn't exist yet since I only really built this for my own personal use, but if this can save anyone else some time and they are interested, that could change.
