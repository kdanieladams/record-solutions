Record Solutions
================
This .NET 4.5 MVC 4 project is a concept for a records management company.  The site has authentication, and an administration interface for managing records online.  The workflow of the site for end-users is as follows:
* User fills out registration form to establish a user account.  Later, this account is granted permission to view specific records by administrators.
* Login to edit account details and view accessible records
* Users may also comment on records in order to convey questions or concerns about them to the administrators
* Administrators may upload, overwrite, and delete records stored in .PDF format on the site's HTTP server
* Administrators may also overview every action which occurs on the site, and the account responsible for the action, thanks to an event logging system integrated into every function.
* Administrators have access to create, update, and delete users as well

Maximizing security will be a primary goal moving forward with this project.  The project lacks some security features which would be necessary for it in a production environment.  Files should be encrypted before upload and decrypted after download, ideally.  Such an operation might be accomplished using secure WCF services, where files can be uploaded unencrypted, encrypted remotely, and moved to Record Solutions storage space.  The WCF service would have to be isolated, and retain nothing of these operations.  The WCF service could then receive a file from Record Solutions storage for decrypting before passing on to user.  

This project also features original design work by me.  Using Twitter Bootstrap, I created an easy-to-use and professional appearence for the front-end, and maintained those aspects in the backend, while being as informative and concise as possible.  The project also features 3 custom jQuery scripts I created; one for handling a calendar display, one for handling system message updates, and another for handling AJAX requests on the administration side.  The entire admin side of the site loads in a single page, then uses AJAX to load subsequent pages for brief and concise loading, maximizing the efficiency of the admin's workflow.  
