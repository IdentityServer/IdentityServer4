Extension Grants
================

OAuth 2.0 defines a couple of standard grant types like ``password``, ``authorization_code`` and ``refresh_token`` for the token endpoint.
Extension grants are a way to add support for non-standard token issuance scenarios like token translation, delegation, custom credentials etc.

You can add support for additional grant types by implementing the ``IExtensionGrantValidator`` interface::

    /// <summary>
    /// Handles validation of token requests using custom grant types
    /// </summary>
    public interface IExtensionGrantValidator
    {
        /// <summary>
        /// Validates the custom grant request.
        /// </summary>
        /// <param name="request">The validation context.</param>
        Task ValidateAsync(ExtensionGrantValidationContext context);

        /// <summary>
        /// Returns the grant type this validator can deal with
        /// </summary>
        /// <value>
        /// The type of the grant.
        /// </value>
        string GrantType { get; }
    }

...and registering the validator in DI - either directly::

    services.AddTransient<IExtensionGrantValidator, MyExtensionsGrantValidator>()

...or using our builder::

    builder.AddExtensionGrantValidator<MyExtensionsGrantValidator>();


Example: Simple delegation using an extension grant
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Imagine the following scenario - a front end client calls a middle tier API using a token acquired via an interactive flow (e.g. hybrid flow).
This middle tier API now wants to call a back end API on behalf of the interactive user:

.. image:: images/delegation.png

In other words - API 1 needs an access token containing the user's identity, but having the scope of the back end API.

.. note:: You might have heard of the term "poor man's delegation" where the access token from the front end is simply forwarded to the back end. This has some short comings, e.g. API 2 must now accept the the API 1 scope which would allow the user to call API 2 directly. Also - you might want to add some delegation specific claims into the token, e.g. the fact that the call path is via API 1.

