---
layout: docs-default
---

# Terminology

The specs, documentation and object model use a certain terminology that you should be aware of.

![modern application architecture]({{ site.baseurl }}/assets/images/terminology.png)

## OpenID Connect Provider (OP)
IdentityServer is an OpenID Connect provider - it implements the OpenID Connect protocol (and OAuth2 as well).

Different literature uses different terms for the same role - you probably also find security token service,
identity provider, authorization server, IP-STS and more.

But they are in a nutshell all the same: a piece of software that issues security tokens to clients.

IdentityServer has a number of jobs and features - including:

* authenticate users using a local account store or via an external identity provider

* provide session management and single sign-on

* manage and authenticate clients

* issue identity and access tokens to clients

* validate tokens

## Client
A client is a piece of software that requests tokens from IdentityServer - either for authenticating a user or
for accessing a resource (also often called a relying party or RP). A client must be registered with the OP.

Examples for clients are web applications, native mobile or desktop applications, SPAs, server processes etc.

## User
A user is a human that is using a registered client to access his or her data.

## Scope
Scopes are identifiers for resources that a client wants to access. This identifier is sent to the OP during an
authentication or token request.

By default every client is allowed to request tokens for every scope, but you can restrict that.

They come in two flavours.

### Identity scopes
Requesting identity information (aka claims) about a user, e.g. his name or email address is modeled as a scope in OpenID Connect.

There is e.g. a scope called `profile` that includes first name, last name, preferred username, gender, profile picture and more.
You can read about the standard scopes [here](http://openid.net/specs/openid-connect-core-1_0.html#ScopeClaims) and you can create your own scopes in IdentityServer to model your own requirements.

### Resource scopes
Resource scopes identify web APIs (also called resource servers) - you could have e.g. a scope named `calendar` that represents your calendar API.

## Authentication/Token Request
Clients request tokens from the OP. Depending on the scopes requested, the OP will return an identity token, an access token, or both.

## Identity Token
An identity token represents the outcome of an authentication process. It contains at a bare minimum an identifier for the user 
(called the `sub` aka subject claim).  It can contain additional information about the user and details on how the user authenticated at the OP.

## Access Token
An access token allows access to a resource. Clients request access tokens and forward them to an API. Access tokens contain information about the client and the user (if present).
APIs use that information to authorize access to their data.
