
# AnarQ - An Open Source Social Media Platform

AnarQ is an Open Source social media platform that you can install on your local server,
providing you with feeds, posts, profiles, the ability to moderate posts, etc. Its most basic functionality
is that of serving as a forum HTTP backend web API, but it goes way beyond the capabilities of a plain
forum, by including social gaming elements, such as likes, profiles, voting, etc. It's arguably in such a regard
more a social media platform, than a traditional forum, competing with social media platforms such
as Reddit, Facebook and Twitter.

Its name is a play upon the two Greek words _"an"_ and _"archy"_ implying _"no leaders"_ or _"no hierarchy"_,
adding the Q in its name implies U. It is Open Source, but you will need a license
of [Magic](https://github.com/polterguy/magic) to run it.

## Implementation details

The system is implemented in Hyperlambda, on top of .Net 5, and uses MySQL or MariaDB as its backing
database. The system is a 100% pure HTML Web API, allowing you to implement any client technology you see fit
on top of it.

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
    "cooldown-minues": 5
  },
```

Paste in the above into your configuration file, just above the `magic` parts, and modify it as needed.
Modify the actual frontend `url` according to where you have your frontend running. The `cooldown-minutes`
part above is the number of minutes users must wait between each OP post they submit to the site. If it
is -1, users can post as frequently as they wish.

The system also contains a whole range of email templates, intended for you to edit and modify as you
see fit, according to your specific needs. These can be found in the folder _"/anarq/data/emails/"_.

You can see a complete example configuration file below.

```json
{
  "anarq": {
    "frontend": {
      "url": "https://anarq.org"
    },
    "cooldown-minues": 15
  },
  "magic": {
    "smtp": {
      "host": "smtp.sendgrid.net",
      "port": 465,
      "secure": true,
      "username": "apikey",
      "password": "SG.dsf345asddfguh345.dsfouhg435ysSDFtrDFGsdf",
      "from": {
        "name": "John Doe",
        "address": "john@doe.com"
      }
    },
    "paypal": {
      "clientId": "AYXmWWv-OKV_RFjcksDoK4nJ0djwlhCmXp0h9staoD4U9dsY0oKfw8PBPH9TJ68s0SHjqGzWJNOT0inv",
      "allowDonations": "true"
    },
    "databases": {
      "mysql": {
        "generic": "Server=db;Database={database};Uid=root;Pwd=ThisIsNotAGoodPassword;SslMode=Preferred;Old Guids=true;"
      },
      "mssql": {
        "generic": "Server=localhost\\SQLEXPRESS;Database={database};Trusted_Connection=True;"
      },
      "default": "mysql"
    },
    "auth": {
      "secret": "asdfoih234gefhdudfu345o3i4uhfsduofdshou345togyguSDFGDSAHsadfoug435",
      "https-only": false,
      "valid-minutes": 120,
      "registration": {
        "allow": true,
        "confirm-email": null
      }
    },
    "io": {
      "root-folder": "~/files/"
    },
    "endpoint": {
      "root-folder": "~/files/"
    },
    "license": "TRIAL-VERSION"
  }
}
```

The things you'll need to change in the above configuration file is as follows.

* __magic/auth/secret__ - You'll need a new JWT auth secret
* __magic/paypal/clientId__ - You'll need to provide your own PayPal ClientID here
* __magic/smtp/password__ - If you're using SendGrid to send emails you can use your own API key password here
* __anarq/frontend/url__ - You'll need to provide the domain for where you intend to install AnarQ here

However, if you change the above parts, you can use the above appSettings.json file as is as you
configure Magic. This allows you to use the Docker images for Magic, making installation of the
backend extremely simple.

Also you'll need a [licence of Magic](https://servergardens.com/buy/) before 47 hours, or the
backend will stop working.

You can see a complete Angular HTTP service layer below, if you want to use AnarQ to learn Angular. This
file wraps ever single HTTP endpoint and the AnarQ backend, and you are free to use it as you see fit.

* [Angular HTTP service layer](https://github.com/polterguy/anarq.frontend/blob/master/src/app/services/anarq.service.ts)

## Profile

This section contains everything related to authentication, registration, and public retrieval of profiles
for registered users in the system.

### GET magic/modules/anarq/profile/authenticate

This endpoint allows you to authenticate a user with a username/password combination, provided as URL
encoded QUERY parameters. Below is an example.

```
https://your-api-domain.com/magic/modules/anarq/profile/authenticate?username=foo4&password=bar
```

The above will return a JWT token back to the caller, without an expiration date, or more specifically an
expiration date 5 years into the future. This allows you to store the token in your client's persistent storage,
such as for instance `localStorage` in your browser, and use the token to authorize future requests towards
your backend. Each consecutive request on behalf of the user needs to associate this token in the HTTP `Authorization`
header as a _"Bearer"_ token. Below is an example

```
Authorization: Bearer eyJhbGciOixyz.eyJ1bmlxyz.GAP-Aluxyz
```

The token returned will not expire before 5 years into the future, allowing you to create clients where you
don't store usernames and passwords, but rather simply store the token once as the user logs in, for never
again to ask the user for a password again, before 5 years down the road of course.

This endpoint does not require the user to be authenticated.

### POST magic/modules/anarq/profile/register

This endpoint registers a new user in the system. It takes the following payload.

```json
{
  "username": "username_on_site",
  "password": "password_on_site",
  "email": "john@doe.com",
  "full_name": "John Doe",
  "email-type": "web|client"
}
```

If the username or email address is already registered on the site, the endpoint will return failure,
explaining you more or less exactly what went wrong.

Notice, the exact flow of the registration process depends upon what client type you choose to use
in the optional `email-type` in the payload you choose to create as you invoke endpoint. This field
can have two different values.

* web
* client

The default value for `email-type` if omitted is `web`.

#### Web frontends

The value of `web` implies you have a web based HTML frontend somewhere, to where you can lead users,
somehow resolving to a URL that will invoke the `confirm-email` endpoint as
the user visits it, being able to retrieve the secret from a QUERY parameter named `secret`. Typically a
link generated this way will look like the following.

```
https://your-frontend-domain.com/confirm-email?email=john%40doe.com&secret=345fbacd678e3eed11f
```

Once the user visits the above link in your frontend, you'll need to extract both the `email` and the `secret`
QUERY parameters, and transmit these to the `confirm-email` endpoint, to have the user verify his or her email
address.

The system will attempt to send an email to the registered email address, using configuration settings found
from your configuration file, allowing the user to confirm his email address. If you wish to edit this
template email, you can find it in _"/anarq/data/emails/confirm-email.web.txt"_. In order to send a valid email,
you need to configure your appSettings.json file having a value of anarq/frontend/url pointing to your
frontend's root URL page, and you'll need to have a frontend URL being `/confirm-email`, accepting
the following query parameters.

* email
* secret

The secret needs to be supplied to the `profile/confirm-email` endpoint later before the user can post, comment or
like posts/comments in the system.

This endpoint does not require the user to be authenticated.

#### Client frontends

If you don't have a web based HTML capable frontend, due to maybe creating only an iOS or Android client -
Then sending the user a hyperlink he clicks to confirm his email is obviously not a choice. Hence, you can
therefor use the `client` type of email sent during registrations, which simply sends the user the generated
secret, allowing him to copy and paste it into your app/client, for then to submit it to your backend somehow,
by invoking the `profile/confirm-email` endpoint from within your client app.

### GET magic/modules/anarq/profile/username-available

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

This endpoint does not require the user to be authenticated. The idea is to invoke this endpoint
as the user types his or her username, to check if the username is available or not, before
clicking the _"Register"_ button.

### GET magic/modules/anarq/profile/email-available

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

This endpoint does not require the user to be authenticated.

### POST magic/modules/anarq/profile/confirm-email

This endpoints confirms a previously registered email address in the system, allowing the
registered user to prove he owns the email address specified as he or she registered at the site.
An example payload can be found below.

```json
{
  "email": "john@doe.com",
  "secret": "SOME_SECRET_HASH_VALUE_HERE"
}
```

The `secret` above needs to be the secret generated automatically by the system as the user registered
on the site, and is the SHA256 value of the combination of the user's email address, and the JWT auth
secret in your appSettings.json configuration file.

This endpoint does not require the user to be authenticated.

### GET magic/modules/anarq/profile/me

This endpoint returns information about the currently authenticated user, and can be used
to retrieve meta data about the currently logged in user, such as his username, email, full name, roles,
etc. This endpoint requires the user to be authenticated, and only returns information about the currently
authenticated user. Kind of similar to `whoami` on a Linux system.

### PUT magic/modules/anarq/profile/paypal-id

Stores the user's PayPal Client id, allowing user to get PayPal donations for his writing.
Example payload below.

```
{
  "payPalId": "PayPal Client ID goes here ..."
}
```

### PUT magic/modules/anarq/profile/email-notifications

Stores the user's email notifications settings. Implying whether or not AnarQ should
send the user an email when something of interest occurs that the user should be notified about.
Example payload below.

```
{
  "notifications": true
}
```

## Posts

This section contains everything related to retrieving OP posts from the backend, in addition to
the feeds, and some of the _"gaming parts"_ of the system.

Notice, posts cannot contain HTML, but assuming your client can handle it, Markdown is perfectly
safe. The backend will validate content submitted as the user tries to create or update a post, and
if it contains illegal HTML characters, it will reject the insert/update, to avoid compromising other
users by tricking them into downloading malicious JavaScript snippets, or HTML snippets that might
contain malware of some sort.

### GET magic/modules/anarq/posts/feed

Returns the most popular items according to the specified query parameters supplied. Popular
here meaning items having the most likes. The endpoint takes 5 QUERY parameters, all of which are
optional, and can be ommitted. Below is a list of parameters the endpoint can handle.

* limit - Maximum number of posts to return. If specified this must be in between the range of 0-100. The default value if omitted is 25.
* offset - Offset from where to start retrieving items. Combined with the above limit argument, this allows you to page items as you see fit.
* topic - Name of topic to return items from within. See sub section topic for an explanation of this.
* username - User that posted the OP.
* minutes - Number of minutes to filter by. Notice, can be multiplied with e.g. 86,400 to filter according to days, weeks, etc.

This is the main _"feed"_ endpoint, returning the most popular posts, according to what posts had the most upvotes. It allows you
to filter posts only submitted by a specific user, or posts submitted within a specific topic. The `minutes` filter allows you
to only return posts that were posted during the last n minutes, resulting in that old posts _"drops off"_ the feed over time,
regardless of how many upvotes they have. Which allows you to create feeds with most popular posts over the last 24 hours, 72 hours, etc
as you see fit.

The endpoint will return something resembling the following.

```json
[
  {
    "id": 3,
    "topic": "news",
    "created": "2021-03-09T08:50:17.000Z",
    "user": "john",
    "visibility": "protected",
    "excerpt": "Covid19 proven to be an international media hoax",
    "licks": 54
  },
  {
    "id": 1,
    "topic": "general",
    "created": "2021-03-09T08:21:46.000Z",
    "user": "peter",
    "visibility": "public",
    "excerpt": "Socially distancing you increases fatality rates for later mutations",
    "licks": 37
  },
]
```

Notice, the endpoint does _not_ return the actual contents of the posts, only an excerpt including its first 50 characters. If you want to retrieve
the entire content of a post, you'll have to use the GET `posts/post` invocation instead.

### GET magic/modules/anarq/posts/post

This endpoint returns a single post, including its entire content, and number of likes it currently has.
It takes one single QUERY parameter being the `id` of the post to return. Invoking it with the following
URL ...

```
https://your-api-domain.com/magic/modules/anarq/posts/post?id=3
```

... might return something resembling the following.

```json
{
  "id": 3,
  "topic": "news",
  "created": "2021-03-09T08:50:17.000Z",
  "user": "root",
  "visibility": "protected",
  "content": "This is ANOTHER news OP posting",
  "likes": 1
}
```

Notice, only authenticated users having confirmed their email address can retrieve posts that
has _"protected"_ as their `visibility` setting. Posts that have been moderated will only be returned
to users belonging to one of the following roles.

* root
* admin
* moderator

All other users can only see posts that are either _"public"_ or "_protected"_. Protected implies
only visible for registered users at the site, having confirmed their email address. In addition,
there is a status value for deleted posts being _"deleted"_.

### POST magic/modules/anarq/posts/post

This endpoint creates a new OP post, and requires the user to be authenticated, and having confirmed
his email address. It takes the following payload.

```json
{
  "visibility": "public",
  "topic": "news",
  "content": "Actual content of your post. Can contain Markdown but NOT HTML",
  "hyperlink": "https://foo.bar.com"
}
```

The `visibility` parts above must be one of the pre-defined visibility settings that exists in the system,
typically one of the following values.

* public
* protected

Public posts are visible to anyone, including users just passing by as visitors, not being authenticated. Protected posts
are only for users that have registered on the site, and having confirmed their email address. The endpoint will return
the ID of the item created, allowing you to for instance instantly navigate to the item, or somehow show it to the user
as it is created.

### PUT magic/modules/anarq/posts/post

This endpoint updates an existing post, but can only be invoked by the user that originally created the post, and
exists such that users can edit their existing posts, after having saved them. An example payload can be found below.

```json
{
  "id": 67777,
  "content": "This is the new updated content of the post, and will overwrite existing content",
  "visibility": "public",
  "topic": "news",
  "hyperlink": "https://foo.bar.com"
}
```

A user can change both the content of the post, and the visibility of the post using this endpoint.

### DELETE magic/modules/anarq/posts/post

Deletes a previously created OP post. Notice, endpoint can only be invoked by the user that originally created
the OP post. And the post is not actually deleted, but only flagged as deleted, making it publicly invisible on
the site for everyone except root accounts, admin accounts, and moderator accounts.

### GET magic/modules/anarq/posts/posts-count

This endpoint returns the number of OP posts in the system. If invoked by an authenticated user having confirmed
his or her email address, the endpoint will return count of both public and protected posts given the specified
filtering conditions. If invoked by a visitor not authenticated, it will only return count of public posts.
If invoked by a moderator, admin or root account, it will return count of _all_ posts, including moderated posts.

The endpoint takes the following optional filtering conditions.

* topic - Only show posts from within the specified topic
* user - Only show posts created by the specified user

### GET magic/modules/anarq/posts/posts

This endpoint works similarly to the above `posts/feed` endpoint, except it will not sort by popularity, but
rather when the post was created. This allows you to retrieve all posts in the system, and page through them as
you see fit, sorted by when the posts were created.

Notice, the endpoint will only return public posts unless invoked by an authenticated user having confirmed
his or her email address, at which point the endpoint will also return protected posts. If invoked by a
moderator, admin, or root account, the endpoint will return also moderated posts. Arguments for the endpoint
are as follows, all arguments are optional.

* topic
* username
* limit
* offset

## Comments

This section contains everything related to comments. Comments are stored as materialised paths, allowing you
to build tree structures of comments in your frontend, to show comments as children of other comments, according
to how they are created. This can be accomplished by using the _"parent"_ field, and/or the _"path"_ field
returned by the endpoints used to retrieve comments.

Notice, comments cannot contain HTML, but assuming your client can handle it, Markdown is perfectly
safe. The backend will validate content submitted as the user tries to create or update a comment, and
if it contains illegal HTML characters, it will reject the insert/update, to avoid compromising other
users by tricking them into downloading malicious JavaScript snippets, or HTML snippets that might
contain malware of some sort.

### POST magic/modules/anarq/comments/comment

This endpoint allows you to post a comment to either an OP posting, or another comment. Its payload is
as follows.

```json
{
  "parent": 67777,
  "content": "Actual content of comment. Can include Markdown but not HTML.",
  "visibility": "public"
}
```

The visibility of comments are similar to the visibility of OP posts, implying unless you've authenticated
at the site, and confirmed your email address, only public posts will be visible for you.
The `parent` above is the ID of the OP posting or another comment.

### PUT magic/modules/anarq/comments/comment

This endpoint allows a user to edit his existing comment, either changing its visibility, and/or changing
its content. It can only be invoked by the user creating the comment. An example payload can be found below.

```json
{
  "id": 67777,
  "content": "Some new comment here. May include Markdown but not HTML.",
  "visibility": "public"
}
```

The endpoint works similarly to the POST equivalent above, except of course instead of taking a `parent` it
requires an `id` to a previously created comment, and the comment _must_ have been created by the same
authenticated user trying to update it.

### DELETE magic/modules/anarq/comments/comment

This endpoint works similarly to the above PUt equivalent, but instead of changing its visibility, and/or content,
it marks the comment as deleted. The endpoint can only be invoked by the user originally having created the
comment. The endpoint requires a single QUERY parameter, being the `id` of the post the caller wants to delete.

### GET magic/modules/anarq/comments/comments

This endpoint returns comments belonging to a parent OP. It can take the following arguments.

* parent - Parent OP post
* limit - Maximum comments to return
* offset - Offset of where to start returning comments

It will return something resembling the following.

```json
[
  {
    "id":70,
    "created":"2021-03-25T08:09:28.000Z",
    "user":"thomas",
    "path":"/000000067/000000070",
    "parent":67,
    "content":"sefpih dfsgoih dfgoih dfg",
    "visibility":"public",
    "licks":0
  }
]
```

### GET magic/modules/anarq/comments/comments-count

This endpoint returns the number of comments matching the specified arguments. Legal
arguments are as follows.

* topic - Topic to filter within
* username - Username to filter within

## Licks

This section contains endpoints for liking posts and comments, allowing
users to like, and/or unlike existing posts, comments, etc.

### POST magic/modules/anarq/licks/lick

Creates a like for an OP posting or a comment. The like will automatically be asssociated with the currently
authenticated user. It requires the `id` to which post or comment you want to associate the like with.
Each comment and post can only be likes by each user at most once. Below is an example payload.

```json
{
  "id": 67777
}
```

### DELETE magic/modules/anarq/licks/lick

Deletes a previously created like for either an OP post or a comment. The endpoint can only be invoked by a
user having previously liked a comment or an OP post. The endpoint requires one single QUERY parameter
being `id`, which is the ID for the comment, and/or post the user previously liked.

### GET magic/modules/anarq/licks/likers

Returns all usernames for all users that liked a specific comment or an OP posting as an array of strings.

## Topics

This section contains everything related to managing and administrating topics in the system.

### POST magic/modules/anarq/topics/topic

This will create a new topic in your site, and takes a payload resembling the following.

```json
{
  "name": "topic_name",
  "description": "This is the descriptive text explaining what your topic is about"
}
```

Name being the primary key for your topic.

### PUT magic/modules/anarq/topics/topic

This will update the description of an existing topic. Notice, you cannot update the name after creating your topic,
only its description. It requires a payload resembling the following.

```json
{
  "name": "foo",
  "description": "This is the NEW descriptive text explaining what your topic is about"
}
```

### DELETE magic/modules/anarq/topics/topic

This will delete an existing topic in the system. Notice, the topic cannot have any posts, or the
deletion will fail. It takes one single parameter, being the name of the topic.

### GET magic/modules/anarq/topics/topics

Returns all topics that exists in the system, together with how many posts topic has, and when the
last activity within the topic was. Notice, this endpoint is cached, but there's another endpoint
that is not cached.

### GET magic/modules/anarq/topics/topics-no-cache

Returns all topics that exists in the system, together with how many posts topic has, and when the
last activity within the topic was. Notice, this endpoint is not cached.

## Admin

This section contains parts needed to administrate your backend, such as moderating posts or comments,
blocking users, etc.

### DELETE magic/modules/anarq/admin/comment

This endpoint allows you to hard delete a comment. Notice, the endpoint can only be invoked by a root or an
admin account, and _physically deletes_ the comment from your database. As an alternative, you might consider
simply invoking the `moderate` endpoint, which only marks the post or comment as moderated instead of physically
deleting it from your database. The endpoint takes one QUERY parameter named `id`, being the id of the comment
you want to delete.

**Warning** - Deleting a comment will also recursively _delete all descendant comments_ beneath the comment you're
currently deleting, while moderating a comment will keep all descendant comments.

### DELETE magic/modules/anarq/admin/post

This endpoint will perform a _hard delete_ of an OP post from your database, and requires one QUERY parameter named `id`.
The endpoint can only be invoked by an administrator.

### DELETE magic/modules/anarq/admin/moderate-comment

This endpoint will moderate a comment, making it invisible on the site, but keep the actual data in your database,
only performing a _"soft delete"_. The endpoint requires one QUERY parameter named `id`, being the ID to the
comment you wish to moderate.

### DELETE magic/modules/anarq/admin/moderate-post

This endpoint will moderate an OP post, making it invisible on the site, but keep the actual data in your database,
only performing a _"soft delete"_. The endpoint requires one QUERY parameter named `id`, being the ID to the
comment you wish to moderate.

### DELETE magic/modules/anarq/admin/un-moderate-comment

This endpoint will un-moderate a comment, making it visible on the site. The endpoint requires one QUERY
parameter named `id`, being the ID to the comment you wish to moderate.

### DELETE magic/modules/anarq/admin/un-moderate-post

This endpoint will un-moderate an OP post, making it visible on the site. The endpoint requires one QUERY
parameter named `id`, being the ID to the comment you wish to moderate.

### DELETE magic/modules/anarq/admin/block-user

This endpoint will completely block a user from being able to interact with the backend. It requires
one QUERY parameter being the username of the user you want to block. Endpoint can only be invoked
by an admin of the site.

### DELETE magic/modules/anarq/admin/un-block-user

This endpoint will remove a block on a user from the backend. It requires
one QUERY parameter being the username of the user you want to un-block. Endpoint can only be invoked
by an admin of the site.



## User

This section allows you to admininstrate your users, and retreieve meta information associated
with users.

### GET magic/modules/anarq/users/user

This endpoint returns profile information for the specified `username` QUERY parameter. The returned
response might resemble the following.

```json
{
  "comments": 5,
  "created": "2021-03-09T14:14:30.000Z",
  "full_name": "Thomas Hansen",
  "karma": 7,
  "licks": 14,
  "locked": false,
  "posts": 21,
  "roles": [
    "guest",
    "root"
  ]
}
```

### GET magic/modules/anarq/users/users

This endpoint lists all users matching the specified QUERY parameters. Parameters you can use are as follows.

* limit
* offset

### GET magic/modules/anarq/users/users-count

Returns the number of registered users in the system.

## Misc

These are miscelaneous endpoints, for things not specific to any of the above sections.

### GET magic/modules/anarq/misc/donations

Returns true if donations have been turned on for site in general.

### POST magic/modules/anarq/misc/log-donation

Logs a donation in the backend. Requires 3 QUERY parameters.

* user - User that received the donation
* donator - Email address of person donating
* amount - Amount that was donated

### GET magic/modules/anarq/misc/paypal-configuration

Returns site wide PayPal configuration. Notice, this is *sitewide* configuration, and
not for individual users. It will return the PayPal ClientID associated with the site.

### GET magic/modules/anarq/misc/tnc

Returns Terms and Conditions for the site. This is a Markdown document you can find within
the folder structure of AnarQ module in the backend.

## License

AnarQ is licensed under the terms of the MIT license, but you will need a license of Magic to run it.
