Record Solutions
================
## DEMONSTRATION PURPOSES ONLY ##
Please note, this repository has been added here for demonstration purposes only.

## Prerequisites ##
* Visual Studio 2013
* .NET Framework 4.5
* MVC 4

## Installation ##
1. Clone project to your local HDD `git clone https://github.com/eod696/Record-Solutions.git Record-Solutions`
2. Run the solution file `RecordSolutions.sln`
3. Install missing packages with NuGet Package Manager [NuGet Docs - Restore Packages](http://docs.nuget.org/docs/reference/package-restore) *IMPORTANT* Visual Studio needs to be Run as Administrator in order for NuGet Package Manager to do it's thing.
4. Build the solution and run it in Debug mode.  This should automagically generate a local development database file in `.mdf` format.  It will appear in the Server Explorer as `RSEntitiesConnection`.  After the first successful build, the solution is operational, and you should be able to browse it as a visitor with an internet browser of your choice.
5. Register by clicking the "Register" link in the upper right corner of the layout.
6. Go back to Visual Studio and stop the debugger.  This is necessary to declare yourself an administrator, because administrators have to be manually declared in the database and you can't edit the database while the solution is running...`.mdf`'s are weird like that.
7. Expand the `RSEntitiesConnection` database in the Server Explorer, then expand the tables folder, then right-click the `webpages_Roles` table, and click `Show Table Data`
8. Create a role called "Administrator" in the `webpages_Roles` table.  The role ID is an auto-increment field, so just put the name of the role (`Administrator`) in place and press Enter to let the ID be automated.
9. Perform the same steps to show table data for the `UserProfile` table.  Find your username, and make note of the ID.
10. Assign your user id to the `Administrators` role id by showing table data for `webpages_UsersInRoles`.  

At this point, you should be able to login to the site and access the MGMT administration interface via the drop-down menu which appears when you click on your avatar in the upper right corner of the layout.
It's a bit cumbersome, I know, but this is partly because I wanted to keep a strangle-hold on how a user might become an administrator, partly because I used automagic methods to add authentication to the project.

## Project Summary ##
This .NET 4.5 MVC 4 project is a concept for a records management company.  The site has authentication, and an administration interface for managing records online.  The workflow of the site for end-users is as follows:
* User fills out registration form to establish a user account.  Later, this account is granted permission to view specific records by administrators.
* Login to edit account details and view accessible records
* Users may also comment on records in order to convey questions or concerns about them to the administrators
* Administrators may upload, overwrite, and delete records stored in .PDF format on the site's HTTP server
* Administrators may also overview every action which occurs on the site, and the account responsible for the action, thanks to an event logging system integrated into every function.
* Administrators have access to create, update, and delete users as well

Maximizing security will be a primary goal moving forward with this project.  The project lacks some security features which would be necessary for it in a production environment.  Files should be encrypted before upload and decrypted after download, ideally.  Such an operation might be accomplished using secure WCF services, where files can be uploaded unencrypted, encrypted remotely, and moved to Record Solutions storage space.  The WCF service would have to be isolated, and retain nothing of these operations.  The WCF service could then receive a file from Record Solutions storage for decrypting before passing on to user.  

This project also features original design work by me.  Using Twitter Bootstrap, I created an easy-to-use and professional appearence for the front-end, and maintained those aspects in the backend, while being as informative and concise as possible.  The project also features 3 custom jQuery scripts I created; one for handling a calendar display, one for handling system message updates, and another for handling AJAX requests on the administration side.  The entire admin side of the site loads in a single page, then uses AJAX to load subsequent pages for brief and concise loading, maximizing the efficiency of the admin's workflow.  
