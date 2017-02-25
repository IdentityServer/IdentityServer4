Welcome to IdentityServer4
==========================

.. image:: images/logo.png
   :align: center

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
Support for external identity providers like Azure Active Directory, Google, Facebook etc.
This shields your applications from the details of how to connect to these external providers.

Focus on Customization
^^^^^^^^^^^^^^^^^^^^^^
The most important part - many aspect of IdentityServer can be customized to fit **your** needs.
Since IdentityServer is a framework and not a boxed product or a SaaS, you can write code to adapt the system the way it makes sense for your scenarios.

Free and Commercial Support
^^^^^^^^^^^^^^^^^^^^^^^^^^^
If you need help building or running your identity platform, :ref:`let us know <refSupport>`.
There are several way we can help you out.

IdentityServer is officially certified by the OpenID Foundation and part of the .NET Foundation.

.. toctree::
   :maxdepth: 2
   :hidden:
   :caption: Introduction

   intro/big_picture
   intro/terminology
   intro/specs
   intro/packaging
   intro/support
   intro/test

.. toctree::
   :maxdepth: 2
   :hidden:
   :caption: Quickstarts

   quickstarts/0_overview
   quickstarts/1_client_credentials
   quickstarts/2_resource_owner_passwords
   quickstarts/3_interactive_login
   quickstarts/4_external_authentication
   quickstarts/5_hybrid_and_api_access
   quickstarts/6_aspnet_identity
   quickstarts/7_javascript_client
   quickstarts/8_entity_framework

.. toctree::
   :maxdepth: 2
   :hidden:
   :caption: Configuration

   configuration/startup
   configuration/resources
   configuration/clients
   configuration/mvc
   configuration/apis

.. toctree::
   :maxdepth: 2
   :hidden:
   :caption: Topics

   topics/grant_types
   topics/secrets
   topics/extension_grants
   topics/resource_owner
   topics/crypto
   topics/deployment
   topics/signin
   topics/signin_external_providers
   topics/consent
   topics/signout_external_providers
   topics/signout_federated
   topics/signout
   topics/logging
   topics/refresh_tokens
   topics/reference_tokens
   topics/windows
   topics/cors

.. toctree::
   :maxdepth: 2
   :hidden:
   :caption: Endpoints

   endpoints/discovery
   endpoints/authorize
   endpoints/token
   endpoints/userinfo
   endpoints/introspection
   endpoints/revocation

.. toctree::
   :maxdepth: 2
   :hidden:
   :caption: Reference

   reference/identity_resource
   reference/api_resource
   reference/client
   reference/grant_validation_result
   reference/interactionservice
   reference/options

.. toctree::
   :maxdepth: 2
   :hidden:
   :caption: Misc

   misc/training
   misc/blogs
   misc/videos