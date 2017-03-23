Events
======
While logging is more low level "printf" style - events represent higher level information about certain operations in IdentityServer.
Events are structured data and include event IDs, success/failure information, categories and details.
This makes it easy to query and analyze them and extract useful information that can be used for further processing.

Events work great with event stores like `ELK <https://www.elastic.co/webinars/introduction-elk-stack>`_, `Seq <https://getseq.net/>`_ or `Splunk <https://www.splunk.com/>`_.

Emitting events
^^^^^^^^^^^^^^^
Events are not turned on by default - but can be globally configured in the ``ConfigureServices`` method, e.g.::

    services.AddIdentityServer(options =>
    {
        options.Events.RaiseSuccessEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseErrorEvents = true;
    });

To emit an event use the ``IEventService`` from the DI container and call the ``RaiseAsync`` method, e.g.::

    public async Task<IActionResult> Login(LoginInputModel model)
    {
        if (_users.ValidateCredentials(model.Username, model.Password))
        {
            // issue authentication cookie with subject ID and username
            var user = _users.FindByUsername(model.Username);
            await _events.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.SubjectId, user.Username));
        }
        else
        {
            await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));
        }
    }

Our default event sink will simply serialize the event class to JSON and forward it to the ASP.NET Core logging system.
If you want to connect to a custom event store, implement the ``IEventSink`` interface and register it with DI.


todo: create custom events

todo: events reference