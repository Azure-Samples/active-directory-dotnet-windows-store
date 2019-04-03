---
services: active-directory
platforms: dotnet
author: jmprieur
level: 200
client: Mobile (UWP)
service: ASP.NET Web API
endpoint: Azure AD v1.0
---
# Integrating a Windows Universal application the Microsoft identity platform

![Build badge](https://identitydivision.visualstudio.com/_apis/public/build/definitions/a7934fdd-dcde-4492-a406-7fad6ac00e17/29/badge)

> There's a newer version of this sample! Check it out: https://github.com/azure-samples/ms-identity-dotnet-native-uwp
>
> This newer sample takes advantage of the Microsoft identity platform (formerly Azure AD v2.0).
>
> While still in public preview, every component is supported in production environments

This sample demonstrates a Windows Store or UWP application calling a web API that is secured using Azure AD.
The Windows Store application uses the Active Directory Authentication Library (ADAL) to obtain a JWT access token through the OAuth 2.0 protocol.  The access token is sent to the web API to authenticate the user.

![Overview](ReadmeFiles/topology.png)

## Scenario

After starting the TodoListService, run the TodoListClient project, and you'll be prompted to sign in with an account
in the Tenant where you registered the application.
Then add items in the textbox and press the "**Add To Do" button. The items appear in the list below the button.

The list is maintained in the TodoList Service. if you run a second instance of the client, you'll see it has the same content

## How to run this sample

To run this sample, you'll need:

- [Visual Studio 2017](https://aka.ms/vsdownload)
- An Internet connection
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/)
- A user account in your Azure AD tenant. This sample will not work with a Microsoft account (formerly Windows Live account). Therefore, if you signed in to the [Azure portal](https://portal.azure.com) with a Microsoft account and have never created a user account in your directory before, you need to do that now.

### Step 1:  Clone or download this repository

From your shell or command line:

`git clone https://github.com/Azure-Samples/active-directory-dotnet-windows-store.git`

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet pacakges, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

### Step 2:  Register the sample with your Azure Active Directory tenant

There are two projects in this sample. Each needs to be separately registered in your Azure AD tenant. To register these projects, you can:

- either follow the steps in the paragraphs below ([Step 2](#step-2--register-the-sample-with-your-azure-active-directory-tenant) and [Step 3](#step-3--configure-the-sample-to-use-your-azure-ad-tenant))
- or use PowerShell scripts that:
  - **automatically** create for you the Azure AD applications and related objects (passwords, permissions, dependencies)
  - modify the Visual Studio projects' configuration files.

If you want to use this automation, read the instructions in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)

#### First step: choose the Azure AD tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com).
1. On the top bar, click on your account and under the **Directory** list, choose the Active Directory tenant where you wish to register your application.
1. Click on **All services** in the left-hand nav, and choose **Azure Active Directory**.

> In the next steps, you might need the tenant name (or directory name) or the tenant ID (or directory ID). These are presented in the **Properties**
of the Azure Active Directory window respectively as *Name* and *Directory ID*

#### Register the service app (TodoListService-StoreApp)

1. Navigate to the [Azure Portal > App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) to register your app. Sign in using a work or school account, or a personal Microsoft account.
1. Select **New registration**.
1. Enter a meaningful name for the application that will be displayed to users of the app, for example 'TodoListService-StoreApp' and leave *Supported account types* on the default setting of *Accounts in this organizational directory only*.
1. For the *Redirect URI (optional)*, select "Web" from the dropdown, and enter the base URL for the sample. By default, this sample uses `https://localhost:44321/`.
1. Click **Register** to create the application.
1. In the application's **Overview** page, find the *Application (client) ID* value and record it for later. You'll need it to configure the Visual Studio configuration file for this project.
1. Then click on **Expose an API**, and set the **Application ID URL** at the top of the page to `'https://<your_tenant_name>/<your_application_name>'` (replacing `<your_tenant_name>` with the name of your Azure AD tenant and `<your_application_name>` with the name of your service app, for example 'TodoListService-StoreApp').
1. Select **Add a scope**.
1. Set the values for the following parameters:

   | Parameter | Value to use |
   |-----------|--------------|
   | **Scope name** | `user_impersonation` |
   | **Who can consent** | `Admins and users` |
   | **Admin consent display name** | `Access TodoListService-StoreApp as a user` |
   | **Admin consent description** | `Accesses the TodoListService-StoreApp Web API as a user` |
   | **User consent display name** | `Access TodoListService-StoreApp as a user` |
   | **User consent description** | `Accesses the TodoListService-StoreApp Web API as a user` |
   | **State** | `Enabled` |

1. Select **Add scope**.

#### Find the TodoListClient app's redirect URI

Before you can register the TodoListClient application in the Azure portal, you need to find out the application's redirect URI.  Windows 8 provides each application with a unique URI and ensures that messages sent to that URI are only sent to that application.  To determine the redirect URI for your project:

1. Open the solution in Visual Studio 2017.
2. In the TodoListClient project, open the `MainPage.xaml.cs` file.
3. Find this line of code and set a breakpoint on it.

```C#
Uri redirectURI = Windows.Security.Authentication.Web.WebAuthenticationBroker.GetCurrentApplicationCallbackUri();
```

4. Right-click on the TodoListClient project and Debug --> Start New Instance.
5. When the breakpoint is hit, use the debugger to determine the value of redirectURI, and copy it aside for the next step.
6. Stop debugging, and clear the breakpoint.

The redirectURI value will look something like the following URI:

```Text
ms-app://s-1-15-2-2123189467-1366327299-2057240504-936110431-2588729968-1454536261-950042884/
```

#### Register the TodoListClient app

1. In the **Azure Active Directory** pane, click on **[App registrations](https://go.microsoft.com/fwlink/?linkid=2083908)** and choose **New registration**.
1. Enter a meaningful name for the application, for example 'TodoListClient-StoreApp'
1. Leave **Supported account types** on the default setting of **Accounts in this organizational directory only**.
1. For the *Redirect URI*, enter value that you obtained during the previous step with the debugger and select "Public client (mobile & desktop)" from the dropdown.
1. Click **Register** to create the application.
1. In the application's **Overview** page, find the *Application ID* value and record it for later. You'll need it to configure the Visual Studio configuration file for this project.

### Step 3:  Configure the sample to use your Azure AD tenant

In the steps below, "ClientID" is the same as "Application ID" or "AppId".

Open the solution in Visual Studio to configure the projects

#### Configure the TodoListService project

1. Open the `TodoListService\Web.Config` file
1. Find the app key `ida:Tenant` and replace the existing value with your Azure AD tenant name.
1. Find the app key `ida:Audience` and replace the existing value with the App ID URI you registered earlier for the TodoListService-StoreApp app. For instance use `https://<your_tenant_name>/TodoListService-StoreApp`, where `<your_tenant_name>` is the name of your Azure AD tenant.

#### Configure the TodoListClient project

1. Open `TodoListClient\MainPage.xaml.cs'.
2. Find the declaration of `tenant` and replace the value with the name of your Azure AD tenant.
3. Find the declaration of `clientId` and replace the value with the Application ID from the Azure portal.
4. Find the declaration of `todoListResourceId` and `todoListBaseAddress`, and ensure their values are set properly for your TodoListService project copied from the Azure portal.

### Step 4 (Optional):  Enable Windows Integrated Authentication when using a federated Azure AD tenant

Out of the box, this sample is not configured to work with Windows Integrated Authentication (WIA) when used with a federated Azure Active Directory domain.  To work with WIA the application manifest must enable additional capabilities.  These capabilities are not configured by default for this sample because applications requesting the Enterprise Authentication or Shared User Certificates capabilities require a higher level of verification to be accepted into the Windows Store, and not all developers may wish to perform the higher level of verification.

To enable Windows Integrated Authentication, in Package.appxmanifest, in the Capabilities tab, enable:

- Enterprise Authentication
- Private Networks (Client & Server)
- Shared User Certificates

### Step 5:  Run the sample

Clean the solution, rebuild the solution, and run it.  You might want to go into the solution properties and set both projects as startup projects, with the service project starting first.

Explore the sample by signing in, adding items to the To Do list, removing the user account, and starting again.  Notice that if you stop the application without removing the user account, the next time you run the application you won't be prompted to sign in again - that is because ADAL has a persistent cache, and remembers the tokens from the previous run.

## How To Deploy This Sample to Azure

This project has one Web API projects. To deploy it to Azure Web Sites, you'll need, for each one, to:

- create an Azure Web Site
- publish the Web App / Web APIs to the web site, and
- update its client(s) to call the web site instead of IIS Express.

### Create and publish the `TodoListService-StoreApp` to an Azure Web Site

1. Sign in to the [Azure portal](https://portal.azure.com).
2. Click **Create a resource** in the top left-hand corner, select **Web + Mobile** --> **Web App**, select the hosting plan and region, and give your web site a name, for example, `TodoListService-StoreApp-contoso.azurewebsites.net`.  Click Create Web Site.
3. Once the web site is created, click on it to manage it.  For this set of steps, download the publish profile by clicking **Get publish profile** and save it.  Other deployment mechanisms, such as from source control, can also be used.
4. Switch to Visual Studio and go to the TodoListService project.  Right click on the project in the Solution Explorer and select **Publish**.  Click **Import Profile** on the bottom bar, and import the publish profile that you downloaded earlier.
5. Click on **Settings** and in the `Connection tab`, update the Destination URL so that it is https, for example [https://TodoListService-StoreApp-contoso.azurewebsites.net](https://TodoListService-StoreApp-contoso.azurewebsites.net). Click Next.
6. On the Settings tab, make sure `Enable Organizational Authentication` is NOT selected.  Click **Save**. Click on **Publish** on the main screen.
7. Visual Studio will publish the project and automatically open a browser to the URL of the project.  If you see the default web page of the project, the publication was successful.

### Update the Active Directory tenant application registration for `TodoListService-StoreApp`

1. Navigate to the [Azure portal](https://portal.azure.com).
2. On the top bar, click on your account and under the **Directory** list, choose the Active Directory tenant containing the `TodoListService-StoreApp` application.
3. On the applications tab, select the `TodoListService-StoreApp` application.
4. In the 'Manage' section select 'Authentication' and update the Sign-On URL and Reply URL fields to the address of your service, for example [https://TodoListService-StoreApp-contoso.azurewebsites.net](https://TodoListService-StoreApp-contoso.azurewebsites.net). Save the configuration.

### Update the `TodoListClient-StoreApp` to call the `TodoListService-StoreApp` Running in Azure Web Sites

1. In Visual Studio, go to the `TodoListClient-StoreApp` project.
2. Open `TodoListClient\MainPage.xaml.cs`.  Only one change is needed - update the `todo:TodoListBaseAddress` key value to be the address of the website you published,
   for example, [https://TodoListService-StoreApp-contoso.azurewebsites.net](https://TodoListService-StoreApp-contoso.azurewebsites.net).
3. Run the client! If you are trying multiple different client types (for example, .Net, Windows Store, Android, iOS) you can have them all call this one published web API.

> NOTE: Remember, the To Do list is stored in memory in this TodoListService sample. Azure Web Sites will spin down your web site if it is inactive, and your To Do list will get emptied.
Also, if you increase the instance count of the web site, requests will be distributed among the instances. To Do will, therefore, not be the same on each instance.

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/adal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`adal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## More information

For more information, see ADAL.NET's conceptual documentation:

- [Acquiring tokens interactively in public client applications](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/Acquiring-tokens-interactively---Public-client-application-flows)

For more information about how OAuth 2.0 protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](http://go.microsoft.com/fwlink/?LinkId=394414).
