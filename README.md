
# AnarQ - An Open Source alternative to Facebook, Reddit, Disqus, etc

AnarQ is an Open Source social media platform that you can install on your local server,
providing you with feeds, posts, the ability to moderate posts, etc. Its most basic functionality
is that of serving as a forum HTTP backend web API.

## Configuring

In order to configure AnarQ you'll need to configure an SMTP server, since the systems frequently sends emails
to its registered users, to among other things have users verify their email address during registrations, etc.
To see how to configure this, visit the link below

* [magic.lambda.mail](https://github.com/polterguy/magic.lambda.mail)

In addition AnarQ requires its own configuration settings. You can find an example below.

```
  "anarq": {
    "frontend": {
      "url": "https://anarq.org"
    }
  },
```

Paste in the above into your configuration file, just above the `magic` parts, and modify it as needed.
Modify the actual frontend `url` according to where you have your frontend running.

## REST endpoints

AnarQ is a pure HTTP REST web API, allowing you to integrate it with any frontend client system you wish.
Below is te documentation for each endpoint in the system, grouped by section.

### Profile

This section contains everything related to authentication, registration, and public retrieval of profiles
for existing registered users in the system.

#### magic/modules/anarq/profile/authenticate - GET

This endpoint allows you to authenticate a user with a username/password combination, provided as URL
encoded query parameters. Below is an exmaple.

```
https://your-api-domain.com/magic/modules/anarq/profile/authenticate?username=foo4&password=bar
```

The above will return a JWT token back to the caller, without an expiration date, or more specifically an
expiration date 5 years into the future. This allows you to store the token in your client's persistent storage,
such as for instance `localStorage` in your browser, and use the token to authorize future requests towards
your backend.

#### magic/modules/anarq/profile/register - POST

This endpoint registers a new user in the system. It takes the following payload.

```json
{
  "username": "username_on_site",
  "password": "password_on_site",
  "email": "john@doe.com",
  "full_name": "John Doe"
}
```

If the username or email address is already registered on the site, the endpoint will return failure,
explaining you exactly what went wrong.

The system will attempt to send an email to the registered email address, using configuration settings found
from your configuration file, allowing the user to confirm his email address. If you wish to edit this
template email, you can find it in _"/anarq/data/emails/confirm-email.txt"_. In order to send a valid email,
you need to configure your appSettings.json file having a value of anarq/frontend/url pointing to your
frontend's URL/confirm-email page, and you'll need to have a frontend URL being `/confirm-email`, accepting
the following query parameters.

* email
* secret

The secret needs to be supplied to the `confirm-email` endpoint later before the user can post, comment or
like posts/comments in the system.

#### magic/modules/anarq/profile/username-available - GET

This endpoint returns true if the specified username is available. An example invocation can be found below.

```
https://your-api-domain.com/magic/modules/anarq/profile/username-available?username=foo3
```

If the username is registered from before, the above will result in the following result.

```json
{
  "result": false,
  "message": "Username already registered"
}
```

If the username is available, the above `result` field will have a value of `true`.

#### magic/modules/anarq/profile/email-available - GET

This endpoint works exactly as the above username-available endpoint, except it of course
expects an `email` query parameter, and checks to see if the specified email address is registered
from before. If the email address is already registered from before, it will return something resembling
the following.

```json
{
  "result": false,
  "message": "Email address already registered"
}
```

If the email address is available, the above `result` field will have a value of `true`.
