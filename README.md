
# AnarQ - An Open Source distributed alternative to Facebook

AnarQ is an Open Source alternative to Facebook that you can install on your local server,
providing you with a feed, posts, the ability to moderate posts, etc.

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
    },
    "emails": {
      "verify-email-subject": "Please verify your email address at AnarQ"
    }
  },
```

Paste in the above into your configuration file, just above the `magic` parts, and modify it as needed.
