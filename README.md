# ClickOnce loader for Citrix XenApp & XenDesktop
Loads a ClickOnce application on a Citrix XenApp - XenDesktop host that uses a shared desktop and startmenu for all users.

# Why was this program created?
Windows Remote Desktop Server does not support the use of ClickOnce applications. 
The Windows Remote Desktop Server role is used on Citrix XenApp servers.

A workaround is to place a copy of the application on the XenApp drive and open the application from this location.
Unfortunately some ClickOnce applications require the application to be launched from the ClickOnce/Application website.

Upon starting there is an error because the shared desktop/startmenu folder on the XenApp server is readonly (best-practice).
dispite the error the application does run on a second launch.

This application loads the application on login and automatically responds to the pop-up dialogs owned by the ClickOnce process.

# Getting started
Modify the private strings in ClickOnceLoaderForCitrix/MainWindow.xaml.cs to math the dialog titles in your language.
Note: Current values are in dutch.

Place the build project in a shared folder on the network with read permissions for all users.
Create a shortcut in the startup folder of your XenApp/XenDesktop image/machines which points towards the build *.exe.

Upon load the application will start minimized, an iexplore window will pop-up and automatically navigate towards the .application URL. After navigation the IExplore window will be minimized.

The ClickOnce dialog shall open and the buttons will be pressed automatically. These actions include the initial install and error dialog. 

Upon completion both the Internet Explorer and application process will be stopped.

# Demonstration
![Alt Text](https://github.com/scara1701/ClickOnce-loader-for-Citrix-XenApp-and-XenDesktop/blob/master/Screenshots/demo.gif)
