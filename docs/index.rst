Welcome to IdentityServer4
==========================

IdentityServer4 is an OpenID Connect and OAuth 2.0 framework for ASP.NET Core.

It enables the following features in your applications:

Authentication as a Service
^^^^^^^^^^^^^^^^^^^^^^^^^^^
Centralized login logic and workflow for all of your applications (web, native, mobile, services).

Single Sign-on / Sign-out
^^^^^^^^^^^^^^^^^^^^^^^^^
Single sign-on (and out) over multiple application types.

Access Control for APIs
^^^^^^^^^^^^^^^^^^^^^^^
Issue access tokens for APIs for various types of clients, e.g. server to server, web applications, SPAs and
native/mobile apps.

Federation Gateway
^^^^^^^^^^^^^^^^^^
Support for external identity providers like Azure Active Directoy, Google, Facebook etc.
This shields your applicatios from the details of how to connect to these external providers.

Focus on Customization
^^^^^^^^^^^^^^^^^^^^^^
The most important part - many aspect of IdentityServer can be customized to fit **your** needs.
Since IdentityServer is a framework and not a boxed product or a SaaS, you can write code to adapt the system the way it makes sense for your scenarios.

.. toctree::
   :maxdepth: 2
   :hidden:
   :caption: Introduction

   intro/bigPicture
   intro/terminology
   intro/specs
   intro/packaging

.. toctree::
   :maxdepth: 2
   :hidden:
   :caption: Getting started

   start/scopes
   start/clients

.. toctree::
   :maxdepth: 2
   :hidden:
   :caption: Reference

   reference/scope
   reference/client