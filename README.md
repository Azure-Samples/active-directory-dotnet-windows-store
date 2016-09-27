---
services: active-directory
platforms: dotnet
author: dstrockis
---

# Integrating a Windows Universal application with Azure AD

This sample demonstrates a Windows Store application calling a web API that is secured using Azure AD.  The Windows Store application uses the Active Directory Authentication Library (ADAL) to obtain a JWT access token through the OAuth 2.0 protocol.  The access token is sent to the web API to authenticate the user.

For more information about how the protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](http://go.microsoft.com/fwlink/?LinkId=394414).

> Looking for previous versions of this code sample? Check out the tags on the [releases](../../releases) GitHub page.

## How To Run This Sample

To run this sample you will need:
- Visual Studio 2015
- Windows 10
- An Internet connection
- A Microsoft account
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, please see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/) 
- A user account in your Azure AD tenant. This sample will not work with a Microsoft account, so if you signed in to the Azure portal with a Microsoft account and have never created a user account in your directory before, you need to do that now.

### Step 1:  Clone or download this repository

From your shell or command line:

`git clone https://github.com/Azure-Samples/active-directory-dotnet-windows-store.git`

### Step 2:  Register the sample with your Azure Active Directory tenant

There are two projects in this sample.  Each needs to be separately registered in your Azure AD tenant.

#### Register the TodoListService web API

1. Sign in to the [Azure portal](https://portal.azure.com).
2. On the top bar, click on your account and under the **Directory** list, choose the Active Directory tenant where you wish to register your application.
3. Click on **More Services** in the left hand nav, and choose **Azure Active Directory**.
4. Click on **App registrations** and choose **Add**.
5. Enter a friendly name for the application, for example 'TodoListService' and select 'Web Application and/or Web API' as the Application Type. For the sign-on URL, enter the base URL for the sample, which is by default `https://localhost:44321`. Click on **Create** to create the application.
6. While still in the Azure portal, choose your application, click on **Settings** and choose **Properties**.
7. Find the Application ID value and copy it to the clipboard.

#### Find the TodoListClient app's redirect URI

Before you can register the TodoListClient application in the Azure portal, you need to find out the application's redirect URI.  Windows 8 provides each application with a unique URI and ensures that messages sent to that URI are only sent to that application.  To determine the redirect URI for your project:

1. Open the solution in Visual Studio 2015.
2. In the TodoListClient project, open the `MainPage.xaml.cs` file.
3. Find this line of code and set a breakpoint on it.

```C#
Uri redirectURI = Windows.Security.Authentication.Web.WebAuthenticationBroker.GetCurrentApplicationCallbackUri();
```

4. Right-click on the TodoListClient project and Debug --> Start New Instance.
5. When the breakpoint is hit, use the debugger to determine the value of redirectURI, and copy it aside for the next step.
6. Stop debugging, and clear the breakpoint.

The redirectURI value will look something like this:

```
ms-app://s-1-15-2-2123189467-1366327299-2057240504-936110431-2588729968-1454536261-950042884/
```

#### Register the TodoListClient app

1. Sign in to the [Azure portal](https://portal.azure.com).
2. On the top bar, click on your account and under the **Directory** list, choose the Active Directory tenant where you wish to register your application.
3. Click on **More Services** in the left hand nav, and choose **Azure Active Directory**.
4. Click on **App registrations** and choose **Add**.
5. Enter a friendly name for the application, for example 'TodoListClient-WindowsStore' and select 'Native' as the Application Type. Enter the Redirect URI value that you obtained during the previous step. Click on **Create** to create the application.
6. While still in the Azure portal, choose your application, click on **Settings** and choose **Properties**.
7. Find the Application ID value and copy it to the clipboard.
8. Configure Permissions for your application - in the Settings menu, choose the 'Required permissions' section, click on **Add**, then **Select an API**, and type 'TodoListService' in the textbox. Then, click on  **Select Permissions** and select 'Access TodoListService'.

### Step 3:  Configure the sample to use your Azure AD tenant

#### Configure the TodoListService project

1. Open the solution in Visual Studio 2013.
2. Open the `web.config` file.
3. Find the app key `ida:Tenant` and replace the value with your AAD tenant name.
4. Find the app key `ida:Audience` and replace the value with the App ID URI of the service.

#### Configure the TodoListClient project

1. Open `MainPage.xaml.cs'.
2. Find the declaration of `tenant` and replace the value with the name of your Azure AD tenant.
3. Find the declaration of `clientId` and replace the value with the Application ID from the Azure portal.
4. Find the declaration of `todoListResourceId` and `todoListBaseAddress` and ensure their values are set properly for your TodoListService project.

### Step 4:  Trust the IIS Express SSL certificate

Since the web API is SSL protected, the client of the API (the web app) will refuse the SSL connection to the web API unless it trusts the API's SSL certificate.  Use the following steps in Windows Powershell to trust the IIS Express SSL certificate.  You only need to do this once.  If you fail to do this step, calls to the TodoListService will always throw an unhandled exception where the inner exception message is:

"The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel."

To configure your computer to trust the IIS Express SSL certificate, begin by opening a Windows Powershell command window as Administrator.

Query your personal certificate store to find the thumbprint of the certificate for `CN=localhost`:

```
PS C:\windows\system32> dir Cert:\LocalMachine\My


    Directory: Microsoft.PowerShell.Security\Certificate::LocalMachine\My


Thumbprint                                Subject
----------                                -------
C24798908DA71693C1053F42A462327543B38042  CN=localhost
```

Next, add the certificate to the Trusted Root store:

```
PS C:\windows\system32> $cert = (get-item cert:\LocalMachine\My\C24798908DA71693C1053F42A462327543B38042)
PS C:\windows\system32> $store = (get-item cert:\Localmachine\Root)
PS C:\windows\system32> $store.Open("ReadWrite")
PS C:\windows\system32> $store.Add($cert)
PS C:\windows\system32> $store.Close()
```

You can verify the certificate is in the Trusted Root store by running this command:

`PS C:\windows\system32> dir Cert:\LocalMachine\Root`

### Step 5 (Optional):  Enable Windows Integrated Authentication when using a federated Azure AD tenant

Out of the box, this sample is not configured to work with Windows Integrated Authentication (WIA) when used with a federated Azure Active Directory domain.  To work with WIA the application manifest must enable additional capabilities.  These are not configured by default for this sample because applications requesting the Enterprise Authentication or Shared User Certificates capabilities require a higher level of verification to be accepted into the Windows Store, and not all developers may wish to perform the higher level of verification.

To enable Windows Integrated Authentication, in Package.appxmanifest, in the Capabilities tab, enable:
* Enterprise Authentication
* Private Networks (Client & Server)
* Shared User Certificates

Plus uncomment the following line of code: 
`authContext.UseCorporateNetwork = true;`

### Step 6:  Run the sample

Clean the solution, rebuild the solution, and run it.  You might want to go into the solution properties and set both projects as startup projects, with the service project starting first.

Explore the sample by signing in, adding items to the To Do list, removing the user account, and starting again.  Notice that if you stop the application without removing the user account, the next time you run the application you won't be prompted to sign-in again - that is because ADAL has a persistent cache, and remembers the tokens from the previous run.

## How To Deploy This Sample to Azure

To deploy the TodoListService to Azure Web Sites, you will create a web site, publish the TodoListService to the web site, and update the TodoListClient to call the web site instead of IIS Express.


### Create and Publish the TodoListService to an Azure Web Site

1. Sign in to the [Azure portal](https://portal.azure.com).
2. Click New in the top left hand corner, select Web + Mobile --> Web App, select the hosting plan and region, and give your web site a name, e.g. todolistservice-contoso.azurewebsites.net.  Click Create Web Site.
3. Once the web site is created, click on it to manage it.  For this set of steps, download the publish profile and save it.  Other deployment mechanisms, such as from source control, can also be used.
4. Switch to Visual Studio and go to the TodoListService project.  Right click on the project in the Solution Explorer and select Publish.  Click Import, and import the publish profile that you just downloaded.
5. On the Connection tab, update the Destination URL so that it is https, for example https://todolistservice-skwantoso.azurewebsites.net.  Click Next.
6. On the Settings tab, make sure Enable Organizational Authentication is NOT selected.  Click Publish.
7. Visual Studio will publish the project and automatically open a browser to the URL of the project.  If you see the default web page of the project, the publication was successful.

### Update the Active Directory Tenant Application Registration
1. Navigate to the [Azure management portal](https://manage.windowsazure.com).
2. In your Active Directory tenant, click on the TodoListService application in the Applications tab.
3. In the Configure tab, update the Sign-On URL and Reply URL fields to the address of your service, for example https://todolistservice-skwantoso.azurewebsites.net.

### Update the TodoListClient to call the TodoListService Running in Azure Web Sites

1. In Visual Studio, go to the TodoListClient project.
2. Open `MainPage.xaml.cs`.  Only one change is needed - update the `todo:TodoListBaseAddress` key value to be the address of the website you published, e.g. https://todolistservice-skwantoso.azurewebsites.net.
3. Run the client!  If you are trying multiple different client types (e.g. .Net, Windows Store, Android, iOS) you can have them all call this one published web API.

NOTE: Remember, the To Do list is stored in memory in this TodoListService sample. Azure Web Sites will spin down your web site if it is inactive, and your To Do list will get emptied. Also, if you increase the instance count of the web site, requests will be distributed among the instances and the To Do will not be the same on each instance.

## About The Code

Coming soon.
