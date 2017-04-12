Contributing
============
We are very open to community contributions, but there are a couple of guidelines you should follow so we can handle this without too much effort.

How to contribute?
^^^^^^^^^^^^^^^^^^
The easiest way to contribute is to open an issue and start a discussion. 
Then we can decide if and how a feature or a change could be implemented. 
If you should submit a pull request with code changes, start with a description, only make the minimal changes to start with and provide tests that cover those changes.

Also read this first: `Being a good open source citizen <https://hackernoon.com/being-a-good-open-source-citizen-9060d0ab9732#.x3hocgw85>`_

General feedback and discussions?
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Please start a discussion on the `core repo issue tracker <https://github.com/IdentityServer/IdentityServer4/issues>`_.

Platform
^^^^^^^^
IdentityServer is built against ASP.NET Core 1.1.0 using the RTM tooling that ships with Visual Studio 2017. 
This is the only configuration we accept.

Bugs and feature requests?
^^^^^^^^^^^^^^^^^^^^^^^^^^
Please log a new issue in the appropriate GitHub repo:

* `Core <https://github.com/IdentityServer/IdentityServer4>`_
* `Samples <https://github.com/IdentityServer/IdentityServer4.Samples>`_
* `AccessTokenValidation <https://github.com/IdentityServer/IdentityServer4.AccessTokenValidation>`_

Other discussions
^^^^^^^^^^^^^^^^^
https://gitter.im/IdentityServer/IdentityServer4

Contributing code and content
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
You will need to sign a Contributor License Agreement before you can contribute any code or content.
This is an automated process that will start after you opened a pull request. 

**We only accept PRs to the dev branch.**

Contribution projects
^^^^^^^^^^^^^^^^^^^^^
We very much appreciate if you start a contribution project (e.g. support for Database X or Configuration Store Y). 
Tell us about it so we can tweet and link it in our docs.

We generally don't want to take ownership of those contribution libraries, we are already really busy supporting the core projects.

**Naming conventions**

If you publish nuget packages that contribute to IdentityServer, we would like to ask you to **not** use the IdentityServer4 prefix - rather use a suffix, e.g.

**good** MyProject.MongoDb.IdentityServer4

**bad** IdentityServer4.MongoDb
