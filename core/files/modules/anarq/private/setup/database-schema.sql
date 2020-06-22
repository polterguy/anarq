/*
 * This MySQL script creates your anarq database
 */
create database `anarq`;
use `anarq`;





/*
 * Auth parts first.
 * Notice, AnarQ overrides the default authentication
 * and authorization database table(s) for Magic.
 */


/*
 * Creating users status table.
 */
create table `user_status` (
  `status` varchar(45) not null,
  `description` varchar(256) not null,
  primary key (`status`)
);

/*
 * Inserting default values into user status table.
 */
insert into user_status (`status`, `description`) values ('new', 'New user that just recently registered an account');
insert into user_status (`status`, `description`) values ('confirmed', 'User has confirmed his email');
insert into user_status (`status`, `description`) values ('verified', 'User has been explicitly and manually confirmed by a moderator');
insert into user_status (`status`, `description`) values ('blocked', 'User has been blocked from system');


/*
 * Creating users table.
 */
create table `users` (
  `username` varchar(256) not null, /* This is restricted such that it has to be a valid email address */
  `password` varchar(256) not null,
  `status` varchar(45) not null default 'new',
  `created` timestamp not null default now(),
  primary key (`username`),
  unique key `users_username_unique` (`username`),
  constraint `users_status_fky` foreign key (`status`) references `user_status` (`status`)
);


/*
 * Creating roles table.
 */
create table `roles` (
  `name` varchar(45) not null,
  `description` varchar(256) null,
  primary key (`name`),
  unique key `roles_name_unique` (`name`)
);

/*
 * Creating association between roles and users through users_roles table.
 */
create table `users_roles` (
  `user` varchar(256) not null,
  `role` varchar(45) not null,
  primary key (`user`, `role`),
  key `user_fky_idx` (`user`),
  key `role_fky_idx` (`role`),
  constraint `users_roles_user_fky` foreign key (`user`) references `users` (`username`) on delete cascade,
  constraint `users_roles_role_fky` foreign key (`role`) references `roles` (`name`) on delete cascade
);

/*
 * Inserting some few roles into our roles table.
 */
insert into roles (name, description) values ('root', 'This is the root account, that is like a super-admin type of account, having access to everything in the system.');
insert into roles (name, description) values ('admin', 'This is an admin account in your system, and it has complete access to do anything');
insert into roles (name, description) values ('moderator', 'This is a moderator account, that can moderate cases');
insert into roles (name, description) values ('user', 'This is a normal user in your system, and it does not have elevated rights in general');

/*
 * Creating users extra data types table.
 */
create table `users_extra_types` (
  `type` varchar(45) not null,
  `description` varchar(256) null,
  `mandatory` boolean not null,
  primary key (`type`)
);

/*
 * Inserting some extra types into our above table.
 * This table should typically contain things such as street address, zip code,
 * SSID, etc for your users - Which depends upon what KYC process you want to use.
 */
insert into users_extra_types (type, description, mandatory) values ('full_name', 'This is the full name of the user', 1);
insert into users_extra_types (type, description, mandatory) values ('email', 'This is the email of the user', 1);
insert into users_extra_types (type, description, mandatory) values ('phone', 'This is the phone number of the user', 1);
insert into users_extra_types (type, description, mandatory) values ('language', 'This is the preferred language of the user', 1);

/*
 * Creating extra table for users.
 */
create table `users_extra` (
  `user` varchar(256) not null,
  `type` varchar(45) not null,
  `value` varchar(1024) not null,
  primary key (`user`, `type`),
  key `users_extra_user_fky_idx` (`user`),
  key `users_extra_type_fky_idx` (`type`),
  constraint `users_extra_user_fky` foreign key (`user`) references `users` (`username`) on delete cascade,
  constraint `users_extra_type_fky` foreign key (`type`) references `users_extra_types` (`type`) on delete cascade
);





/*
 * Localization tables, for translating the system.
 */

/*
 * Contains all supported languages in the system.
 */
 create table `languages` (
  `locale` varchar(5) not null,
  `description` varchar(2048) null,
  primary key (`locale`)
);

/*
 * Inserting default supported languages.
 */
insert into languages (`locale`, `description`) values ('en', 'English');
insert into languages (`locale`, `description`) values ('no', 'Norsk');

/*
 * Contains all supported languages in the system.
 */
 create table `translations` (
  `locale` varchar(5) not null,
  `key` varchar(128) not null,
  `content` text null,
  primary key (`locale`, `key`),
  constraint `translations_locale_fky` foreign key (`locale`) references `languages` (`locale`)
);

/*
 * Inserting default English translation values.
 */
insert into translations (`locale`, `key`, `content`) values ('en', 'AuditYourVote', 'Audit your vote');
insert into translations (`locale`, `key`, `content`) values ('en', 'WhyAskMeThis', 'Why do you ask me this?');
insert into translations (`locale`, `key`, `content`) values ('en', 'ResetPasswordEmailSubject', 'Password reset link at AnarQ');
insert into translations (`locale`, `key`, `content`) values ('en', 'VoteReceiptEmailSubject', 'Your vote for case with ID of #');
insert into translations (`locale`, `key`, `content`) values ('en', 'CaseReceiptEmailSubject', 'Your case with ID of #');
insert into translations (`locale`, `key`, `content`) values ('en', 'VerifyEmailSubject', 'Welcome as a registered user at AnarQ');
insert into translations (`locale`, `key`, `content`) values ('en', 'AlreadyVotedForCase', 'You have already voted for this case');
insert into translations (`locale`, `key`, `content`) values ('en', 'CannotVoteOutsideRegions', 'You cannot vote for cases outside your region(s)');
insert into translations (`locale`, `key`, `content`) values ('en', 'CasePastDeadline', 'This case is past its deadline');
insert into translations (`locale`, `key`, `content`) values ('en', 'VoteHashFailure', 'Your vote\'s hash value does not validate towards the previous vote\'s hash value. This is a major security concern, and you should immediately contact somebody at AnarQ and inform them about this issue.');
insert into translations (`locale`, `key`, `content`) values ('en', 'YourVoteWasDeleted', 'Your vote has been deleted. This is a major security concern, and you should immediately contact one of our system administrators, and send him the link to this web page.');
insert into translations (`locale`, `key`, `content`) values ('en', 'YouHaveAlreadyVerifiedEmail', 'You have already verified your email address');
insert into translations (`locale`, `key`, `content`) values ('en', 'YourVerifyEmailTicketIsNotValid', 'Your ticket for verifying your email address is not valid');
insert into translations (`locale`, `key`, `content`) values ('en', 'AlreadySetRegions', 'You have already set your regions, and you have to specifically apply to change them');
insert into translations (`locale`, `key`, `content`) values ('en', 'WeHaveSentResetPasswordLink', 'We have sent you a reset password link');
insert into translations (`locale`, `key`, `content`) values ('en', 'NoSuchUserFound', 'No such user found');
insert into translations (`locale`, `key`, `content`) values ('en', 'YouCannotChangePassword', 'You do not have access to changing the user\'s password');
insert into translations (`locale`, `key`, `content`) values ('en', 'YouAreNotHuman', 'You are not a human being');
insert into translations (`locale`, `key`, `content`) values ('en', 'PhoneAlreadyRegistered', 'Phone is already registered');
insert into translations (`locale`, `key`, `content`) values ('en', 'CreatedCaseInRegionLessThan24HoursAgo', 'Du har laget en ny sak i denne regionen for mindre enn 5 minutter siden, vi tillater deg kun å lage en sak per 5 minutt i denne regionen');
insert into translations (`locale`, `key`, `content`) values ('en', 'CreatedCaseInRegionLessThan1HourAgo', 'Du har laget en ny sak i denne regionen for mindre enn 5 minutter siden, vi tillater kun en sak 5 minutter i denne regionen');
insert into translations (`locale`, `key`, `content`) values ('en', 'NoHtmlSubjectBody', 'You cannot use the characters [, ], < or > in neither your subject nor your body');
insert into translations (`locale`, `key`, `content`) values ('en', 'EmailAlreadyRegistered', 'Email address is already registered');
insert into translations (`locale`, `key`, `content`) values ('en', 'NotValidEmailAddress', 'Not a valid email address');
insert into translations (`locale`, `key`, `content`) values ('en', 'Abort', 'Abort');
insert into translations (`locale`, `key`, `content`) values ('en', 'PasswordIsNotLongEnough', 'Your password must be at least 10 characters long');
insert into translations (`locale`, `key`, `content`) values ('en', 'ForgotYourPassword', 'Did you forget your password?');
insert into translations (`locale`, `key`, `content`) values ('en', 'YourPasswordWasChanged', 'Your password was changed');
insert into translations (`locale`, `key`, `content`) values ('en', 'PasswordDoNotMatch', 'Passwords are not matching');
insert into translations (`locale`, `key`, `content`) values ('en', 'CreateNewPassword', 'Reset password');
insert into translations (`locale`, `key`, `content`) values ('en', 'ResetYourPassword', 'Reset your password');
insert into translations (`locale`, `key`, `content`) values ('en', 'EmailUsedDuringRegistration', 'Email you used during registration');
insert into translations (`locale`, `key`, `content`) values ('en', 'IForgotMyPassword', 'I forgot my password');
insert into translations (`locale`, `key`, `content`) values ('en', 'ForgotPassword', 'Forgot password?');
insert into translations (`locale`, `key`, `content`) values ('en', 'StartPoliticalRally', 'Start a political rally in');
insert into translations (`locale`, `key`, `content`) values ('en', 'TicketValidNowTellRegion', 'Your email address was successfully confirmed. Now you must tell us where you live.');
insert into translations (`locale`, `key`, `content`) values ('en', 'seconds', 'seconds');
insert into translations (`locale`, `key`, `content`) values ('en', 'minutes', 'minutes');
insert into translations (`locale`, `key`, `content`) values ('en', 'hours', 'hours');
insert into translations (`locale`, `key`, `content`) values ('en', 'days', 'days');
insert into translations (`locale`, `key`, `content`) values ('en', 'weeks', 'weeks');
insert into translations (`locale`, `key`, `content`) values ('en', 'months', 'months');
insert into translations (`locale`, `key`, `content`) values ('en', 'fromNow', 'from now');
insert into translations (`locale`, `key`, `content`) values ('en', 'ago', 'ago');

insert into translations (`locale`, `key`, `content`) values ('en', 'Logout', 'Logout');
insert into translations (`locale`, `key`, `content`) values ('en', 'Users', 'Users');
insert into translations (`locale`, `key`, `content`) values ('en', 'ClickHereIfYouLiveInX', 'Click here if you live in {0}');
insert into translations (`locale`, `key`, `content`) values ('en', 'CongratulationsHeader', 'Congratulations');
insert into translations (`locale`, `key`, `content`) values ('en', 'StepXOfY', 'Step {0} of {1}');
insert into translations (`locale`, `key`, `content`) values ('en', 'HomeSlogan', 'Where YOU are the government!');
insert into translations (`locale`, `key`, `content`) values ('en', 'Cases', 'Cases');
insert into translations (`locale`, `key`, `content`) values ('en', 'Open', 'Open');
insert into translations (`locale`, `key`, `content`) values ('en', 'Votes', 'Votes');
insert into translations (`locale`, `key`, `content`) values ('en', 'Popular', 'Popular');
insert into translations (`locale`, `key`, `content`) values ('en', 'Newest', 'Newest');
insert into translations (`locale`, `key`, `content`) values ('en', 'Closed', 'Closed');
insert into translations (`locale`, `key`, `content`) values ('en', 'MyCases', 'My cases');
insert into translations (`locale`, `key`, `content`) values ('en', 'CasesIHaveVotedFor', 'Cases I have voted for');
insert into translations (`locale`, `key`, `content`) values ('en', 'NothingToSeeHere', 'Nothing to see here');
insert into translations (`locale`, `key`, `content`) values ('en', 'Get25NextItems', 'Get 25 next items');
insert into translations (`locale`, `key`, `content`) values ('en', 'More', 'More');
insert into translations (`locale`, `key`, `content`) values ('en', 'ThereAreXUsersInSystem', 'There are {0} users in the system');
insert into translations (`locale`, `key`, `content`) values ('en', 'ThereAreXCasesInSystem', 'There are {0} cases in the system');
insert into translations (`locale`, `key`, `content`) values ('en', 'ThereAreXOpenCasesInSystem', 'There are {0} open cases in the system');
insert into translations (`locale`, `key`, `content`) values ('en', 'ThereAreXVotesInSystem', 'There are {0} votes in the system');
insert into translations (`locale`, `key`, `content`) values ('en', 'YouNeedToSetupRegions', 'You need to setup your regions');
insert into translations (`locale`, `key`, `content`) values ('en', 'Register', 'Register');
insert into translations (`locale`, `key`, `content`) values ('en', 'Login', 'Login');
insert into translations (`locale`, `key`, `content`) values ('en', 'CannotCreateCaseInRegion', 'You cannot create a case in {0} since you just recently created one.');
insert into translations (`locale`, `key`, `content`) values ('en', 'OnlyLoggedInUsersCanCreateCases', 'Only logged in users can create cases');
insert into translations (`locale`, `key`, `content`) values ('en', 'OKIGotIt', 'OK, I got it!');
insert into translations (`locale`, `key`, `content`) values ('en', 'SubjectMustBe', 'Subject must be 15-100 characters long, and end with a question mark ?');
insert into translations (`locale`, `key`, `content`) values ('en', 'Subject', 'Subject');
insert into translations (`locale`, `key`, `content`) values ('en', 'Acceptable', 'Acceptable');
insert into translations (`locale`, `key`, `content`) values ('en', 'FeelFreeToUseMarkdown', 'Feel free to use Markdown, but NO LINKS!');
insert into translations (`locale`, `key`, `content`) values ('en', 'BodyLength', 'Body {0} of {1}');
insert into translations (`locale`, `key`, `content`) values ('en', 'Deadline', 'Deadline');
insert into translations (`locale`, `key`, `content`) values ('en', 'CaseOpenShort', 'Case will only accept votes for {0} days');
insert into translations (`locale`, `key`, `content`) values ('en', 'ShortDeadline', 'Short deadline');
insert into translations (`locale`, `key`, `content`) values ('en', 'CaseOpenMedium', 'Case will only accept votes for {0} days');
insert into translations (`locale`, `key`, `content`) values ('en', 'MediumDeadline', 'Medium deadline');
insert into translations (`locale`, `key`, `content`) values ('en', 'CaseOpenLong', 'Case will only accept votes for {0} days');
insert into translations (`locale`, `key`, `content`) values ('en', 'LongDeadline', 'Long deadline');
insert into translations (`locale`, `key`, `content`) values ('en', 'SubmitCaseToPeopleRepublic', 'Submits your case to the people\'s republic of {0}');
insert into translations (`locale`, `key`, `content`) values ('en', 'Submit', 'Submit');
insert into translations (`locale`, `key`, `content`) values ('en', 'CaseSuccessfullyCreated', 'Case was successfully created');
insert into translations (`locale`, `key`, `content`) values ('en', 'CheckOutCase', 'Check out how the case is proceeding');
insert into translations (`locale`, `key`, `content`) values ('en', 'PreviousReceipt', 'Previous receipt');
insert into translations (`locale`, `key`, `content`) values ('en', 'Home', 'Home');
insert into translations (`locale`, `key`, `content`) values ('en', 'AllRightBaby', 'All RIGHT baby! I\'m ready to roll!');
insert into translations (`locale`, `key`, `content`) values ('en', 'SeeAllOpenCases', 'Go home');
insert into translations (`locale`, `key`, `content`) values ('en', 'VoteForCaseAbove', 'Vote for case above');
insert into translations (`locale`, `key`, `content`) values ('en', 'CheckOutHowTheCaseDid', 'Check out how the case did');
insert into translations (`locale`, `key`, `content`) values ('en', 'QuestionWasAskedIn', 'Question was asked in {0}');
insert into translations (`locale`, `key`, `content`) values ('en', 'CaseWasBroughtForthBy', 'Question was brought forth to the public by {0}');
insert into translations (`locale`, `key`, `content`) values ('en', 'VotesCanBeCastUntil', 'Votes can be cast until {0}');
insert into translations (`locale`, `key`, `content`) values ('en', 'XAyeAndUNay', '{0} yes and {1} no');
insert into translations (`locale`, `key`, `content`) values ('en', 'VoteAyeForCase', 'Vote yes for this case');
insert into translations (`locale`, `key`, `content`) values ('en', 'VoteNayForCase', 'Vote no for this case');
insert into translations (`locale`, `key`, `content`) values ('en', 'YouVotedAye', 'You voted aye for this case. Only you can see your vote.');
insert into translations (`locale`, `key`, `content`) values ('en', 'YouVotedNay', 'You voted nay for this case. Only you can see your vote.');
insert into translations (`locale`, `key`, `content`) values ('en', 'CryptoReceiptSent', 'A cryptographically signed email receipt of your vote was sent to your registered email address. Please keep this email safe somewhere in case of auditing of the system.');
insert into translations (`locale`, `key`, `content`) values ('en', 'CaseWasWon', 'Case was won');
insert into translations (`locale`, `key`, `content`) values ('en', 'CaseWasLost', 'Case was lost');
insert into translations (`locale`, `key`, `content`) values ('en', 'CaseWasNotDetermined', 'Case was not determined');
insert into translations (`locale`, `key`, `content`) values ('en', 'ThePeoplesCourt', 'The people\'s court of Law at the Independent Republic of {0}');
insert into translations (`locale`, `key`, `content`) values ('en', 'BringForthAQuestion', 'Bring forth a question to the People\'s Court of {0}');
insert into translations (`locale`, `key`, `content`) values ('en', 'SeemsAllCaughtUp', 'Seems that you are all caught up');
insert into translations (`locale`, `key`, `content`) values ('en', 'WhyNotCreate', 'Why don\'t you create a case yourself?');
insert into translations (`locale`, `key`, `content`) values ('en', 'GetXNextItems', 'Get {0} next items');
insert into translations (`locale`, `key`, `content`) values ('en', 'ProgressOfRegistration', 'Progress of registration process');
insert into translations (`locale`, `key`, `content`) values ('en', 'EmailMustBeAvailable', 'Email must be valid and available');
insert into translations (`locale`, `key`, `content`) values ('en', 'Email', 'Email');
insert into translations (`locale`, `key`, `content`) values ('en', 'UsernameCharacterPattern', '4-30 characters, a-z, 0-9 and \'-\' or \'_\'. Also, username cannot be your email address');
insert into translations (`locale`, `key`, `content`) values ('en', 'Username', 'Username');
insert into translations (`locale`, `key`, `content`) values ('en', 'NamePattern', 'Name must be your full legal name, as written in public records');
insert into translations (`locale`, `key`, `content`) values ('en', 'FullLegalName', 'Full legal name');
insert into translations (`locale`, `key`, `content`) values ('en', 'Why', 'Why ...?');
insert into translations (`locale`, `key`, `content`) values ('en', 'PhonePattern', 'Phone number must be a registered cell phone in your name, and contain only numbers (no spaces)');
insert into translations (`locale`, `key`, `content`) values ('en', 'CellPhone', 'Cell phone');
insert into translations (`locale`, `key`, `content`) values ('en', 'PasswordMustBeAtLeast', 'Password must be at least 10 characters long');
insert into translations (`locale`, `key`, `content`) values ('en', 'Password', 'Password');
insert into translations (`locale`, `key`, `content`) values ('en', 'PleaseRepeatPassword', 'Please repeat your password');
insert into translations (`locale`, `key`, `content`) values ('en', 'RepeatPassword', 'Repeat password');
insert into translations (`locale`, `key`, `content`) values ('en', 'IAgreeToTermsAndConditions', 'I agree to the above terms and conditions');
insert into translations (`locale`, `key`, `content`) values ('en', 'ConfirmThatYouAreHuman', 'Confirm that you are a human being');
insert into translations (`locale`, `key`, `content`) values ('en', 'SubmitFormAndRegister', 'Submit the form and accept the terms and conditions of the site');
insert into translations (`locale`, `key`, `content`) values ('en', 'IAmHuman', 'I am a Human Being!');
insert into translations (`locale`, `key`, `content`) values ('en', 'ConfirmEmailAddress', 'Please check your email and confirm your email address.');
insert into translations (`locale`, `key`, `content`) values ('en', 'YouAreAlreadyRegistered', 'You are already registered at this site');
insert into translations (`locale`, `key`, `content`) values ('en', 'UsernameAlreadyRegistered', 'Username \'{0}\' is already registered');
insert into translations (`locale`, `key`, `content`) values ('en', 'YouCanOnlyRegisterOnce', 'You can only register once at the site');
insert into translations (`locale`, `key`, `content`) values ('en', 'PleaseCheckEmailInbox', 'Please check your email inbox');
insert into translations (`locale`, `key`, `content`) values ('en', 'WhereDoYouLive', 'Where do you live?');
insert into translations (`locale`, `key`, `content`) values ('en', 'Filter', 'Filter');
insert into translations (`locale`, `key`, `content`) values ('en', 'YouHaveAlreadySetupRegions', 'You have already configured your regions, and you need to specifically apply to change them.');
insert into translations (`locale`, `key`, `content`) values ('en', 'OnlyLoggedInUsersCanChangeRegions', 'Only logged in users can configure their regions.');
insert into translations (`locale`, `key`, `content`) values ('en', 'Congratulations', 'Congratulations, you are now allowed to vote and propose cases at AnarQ');
insert into translations (`locale`, `key`, `content`) values ('en', 'UserHasVerifiedEmail', 'User has verified his email address');
insert into translations (`locale`, `key`, `content`) values ('en', 'ThisIsHowManyCases', 'This is how many cases the user has created');
insert into translations (`locale`, `key`, `content`) values ('en', 'CasesCreated', 'Cases created');
insert into translations (`locale`, `key`, `content`) values ('en', 'ThisIsHowManyVotesReceived', 'This is how many votes user has received on his or her own cases');
insert into translations (`locale`, `key`, `content`) values ('en', 'VotesGiven', 'Votes given');
insert into translations (`locale`, `key`, `content`) values ('en', 'VotesUserHasGiven', 'Votes');
insert into translations (`locale`, `key`, `content`) values ('en', 'ThisIsHowManyVotesGiven', 'This is how many votes the user has given');
insert into translations (`locale`, `key`, `content`) values ('en', 'Phone', 'Phone');
insert into translations (`locale`, `key`, `content`) values ('en', 'VotesUserHasGivenPerRegion', 'Votes user has given per region');
insert into translations (`locale`, `key`, `content`) values ('en', 'WinningsAndLoosings', 'Winnings and loosings');
insert into translations (`locale`, `key`, `content`) values ('en', 'YouHaveAlreadyVoted', 'You have already voted for this case');
insert into translations (`locale`, `key`, `content`) values ('en', 'YouHaveNotVoted', 'You have not given your vote in this case');
insert into translations (`locale`, `key`, `content`) values ('en', 'WonCases', 'Won cases');
insert into translations (`locale`, `key`, `content`) values ('en', 'LostCases', 'Lost cases');
insert into translations (`locale`, `key`, `content`) values ('en', 'TiedCases', 'Tied cases');
insert into translations (`locale`, `key`, `content`) values ('en', 'VotesSmall', 'votes');
insert into translations (`locale`, `key`, `content`) values ('en', 'YouHaveVerifiedEmail', 'You have successfully verified your email');

/*
 * Longer English translation texts.
 */
insert into translations (`locale`, `key`, `content`) values ('en', 'WhyWeNeedRegions', 
    'We need to know where you live, because we only allow users to create cases and vote '
    'for cases within the region(s) they live. This prohibits others from influencing '
    'the politics in your area, unless they also live in the same area as you live in. '
    'AnarQ believes strongly in de-centralized democratic processes, and allowing '
    'those who are influenced from politics, to be the only ones allowing to decide what should be done. '
    'Hence, we will need to know where you live, such that we can allow you to create cases, and '
    'vote for cases, in the area where you live.');
insert into translations (`locale`, `key`, `content`) values ('en', 'ReceiptChecksOut', 
      'Receipt checks out, and does not seem to have been tampered with. In case an external\n'
      'auditing of the system is required, you might need to forward your original receipt (email)\n'
      'to an independent 3rd party organisation.');
insert into translations (`locale`, `key`, `content`) values ('en', 'NameInformation',
      '<p>\n'
      '  In order to make sure you are a real person, and preventing people from registering\n'
      '  fake profiles, and such influence votes - We ask you to kindly give us your full name,\n'
      '  as it can be found in official archives. Notice, if you cannot prove you are a legal\n'
      '  person and citizen of country of registration, we might suspend your account,\n'
      '  and yes, <strong>we will check this up.</strong>\n'
      '</p>\n'
      '<p>\n'
      '  We will never disclose your personal information to any 3rd parties.\n'
      '</p>');
insert into translations (`locale`, `key`, `content`) values ('en', 'PhoneInfo', 
      '<p>\n'
      '  In order to make sure you are a real person, and preventing people from registering\n'
      '  fake profiles, and such influence votes - We ask you to kindly give us your phone number,\n'
      '  as it can be found in the yellow pages. Notice, if you cannot prove you are a legal\n'
      '  person and citizen of country of registration, <strong>we might suspend your account</strong>,\n'
      '  and yes, we will check this up!\n'
      '</p>\n'
      '<p>\n'
      '  Notice, we might also send you a text message, to have you verify your phone number\n'
      '  as you use the site.\n'
      '</p>\n'
      '<p>\n'
      '  We will never disclose your personal information to any 3rd parties.\n'
      '</p>');
insert into translations (`locale`, `key`, `content`) values ('en', 'ReadThisFirstQuestion',
	    '<h3>READ THIS FIRST!</h3>\n'
      '<p>Since this is the first case you create, you should <strong>carefully read the following information</strong>, to avoid problems and confusion.\n'
      ' Don\'t worry though, we won\'t show it to you again, after you have createdyour first case.</p>\n'
      '<ol>\n'
      '<li>We don\'t allow posts that can be perrceived as bullying of vulnerable individuals.</li>\n'
      '<li>Every question\'s subject must end with a question mark \'?\', then in its description you can write more informally to rally support for your side. However, the subject must be a valid question.</li>\n'
      '<li>Every case must have a clearly defined yes/no answer, making it easy for others to understand what their votes imply. All cases that are difficult to <strong>clearly answer yes or no to</strong>, as phrased in the subject, will be rejected by a moderator.</li>\n'
      '<li>You cannot delete your case, and neither will we censor cases, unless they are violating our terms and conditions. <strong>So phrase your case carefully</strong>. Your case might very well never be possible to remove due to these reasons. So make sure you are willing to publicly ask whatever question you are about to ask before you click the submit button.</li>\n'
      '<li><strong>NO OUTING OF CHILDREN</strong>, in any ways, what so ever - And <strong>especially not</strong> anything that might be seen as <strong>sexually explicit content</strong> about children. If you mention full names, and/or personal data, of any human being below the age of 18, <strong>WE WILL CENSOR YOUR CASE</strong>!</li>\n'
      '<li>Your subject line must be between 15 and 100 characters long, and it must end with a question mark.</li>\n'
      '<li>Your content must be between 50 and 2500 characters long.</li>\n'
      '<li>A piece of advice is to read a couple of the existing cases in the system, before creating your own cases, to get a general feeling of how cases can be created.</li>\n'
      '<li>You should also phrase your question such that whatever results you wish to see, is everybody answering <strong><em>"YES"</em></strong>! The reasons is that we calculate a <em>"won"</em> case this way, and lost cases as cases where more than 50% of the voters gave you a <em>"no"</em>. And your user is granted <em>"political points"</em> by calculating the ratio of won cases this way.</li>\n'
      '</ol>\n'
      '<p>\n'
      'Any case not according to the above terms and conditions, will be rejected and censored. AnarQ is first and foremost\n'
      'an attempt at creating a <strong>serious Direct Democratic Political System</strong>, to influence our government to\n'
      'do the right thing, according to the people\'s will - And not your <em>"traditional social media platform"</em>.\n'
      '</p>\n'
      '<span class="slogan">Happy AnarQ</span>');
insert into translations (`locale`, `key`, `content`) values ('en', 'ReadThisFirstCase',
      '<h3>Read this</h3>\n'
      '<p>\n'
      '  Since this is the first case you create, we\'d like to give you some piece of advice\n'
      '  in regards to how to market your case. First of all, if you <strong>print the case</strong>,\n'
      '  the case\n'
      '  will probably fit quite well on a piece of A4 paper, and contain a QR code allowing\n'
      '  others to read your case online, and vote for whatever their opinion reflects in regards\n'
      '  to the case. This allows you to completely bypass any conventional censoring regimes,\n'
      '  such as those found at Facebook and Twitter, by simply marketing your case on conventional\n'
      '  paper, and hang these sheets of paper up in the local area where you live. But please, try to follow\n'
      '  the law in whatever region you happen to live in, and don\'t spam your entire local\n'
      '  neighbourhood with sheets of paper, violating other individuals right to use their own\n'
      '  real estate as they see fit themselves.\n'
      '</p>\n'
      '<p>\n'
      '  In fact, the cases you create in AnarQ are almost like <em>"pre defined"</em> marketing\n'
      '  brochures in such a regard, allowing you to literally use them as brochures, and hand\n'
      '  them out during political rallies, demonstrations, and such, and also hang them up at\n'
      '  places in your local neighbourhood. We refer to this idea as <em>"guerilla politics"</em>,\n'
      '  since it allows you to completely bypass any traditional censorship regime in traditional\n'
      '  online social media, and/or conventional press. The idea is kind of like that you are your\n'
      '  own press!\n'
      '</p>\n'
      '<p>\n'
      '  Check out how your case looks like in print by clicking <strong>CTRL+P</strong> to see a preview\n'
      '  of how it will end up looking.\n'
      '</p>\n'
      '<p>\n'
      '  <strong>Notice!</strong> In addition to this, you might also want to share your case online,\n'
      '  on for instance Twitter and Facebook. But printing out your case, makes you less dependent\n'
      '  upon these websites and their censorship regime, which history have taught us is pretty bad\n'
      '  unfortunately.\n'
      '</p>\n'
      '<span\n'
      '  class="slogan">\n'
      '  Free Speech! One Each!\n'
      '</span>');
insert into translations (`locale`, `key`, `content`) values ('en', 'TermsAndConditions',
      '<label class="bottom-slogan">Terms and Conditions</label>\n'
      '<p>\n'
      '  By clicking the Submit button below, you guarantee us that the above information is\n'
      '  correct, that you have given us your full legal name, and a cell phone number registered\n'
      '  to you in the country where you are registering.\n'
      '</p>\n'
      '<p>\n'
      '  You also explicitly give us permission to <strong>send you email receipts, of every single vote you\n'
      '  cast in the system</strong>, in addition to sending you email receipts for every case you create.\n'
      '</p>\n'
      '<p>\n'
      '  Votes you cast, and your private information, will never be shared by us with anyone.\n'
      '  The only exception is in case of that the United Nations appoints an independent 3rd\n'
      '  party to audit votes in your region for a particular case - At which point we will\n'
      '  under strict supervision allow auditing of your vote, on individual cases,\n'
      '  by an independent 3rd party organisation, appointed by the United Nations (only!)\n'
      '  At which point the independent 3rd party organisation might contact you to verify your\n'
      '  vote for security reasons, in order to influence legislation in and around the area\n'
      '  where you live, according to your vote\'s value.\n'
      '</p>');



/*
 * Inserting default Norwegian translation values.
 */
insert into translations (`locale`, `key`, `content`) values ('no', 'AuditYourVote', 'Sjekk stemmen din');
insert into translations (`locale`, `key`, `content`) values ('no', 'WhyAskMeThis', 'Hvorfor spør dere?');
insert into translations (`locale`, `key`, `content`) values ('no', 'VoteReceiptEmailSubject', 'Kvitttering for at du har stemt på sak med saksnummer #');
insert into translations (`locale`, `key`, `content`) values ('no', 'CaseReceiptEmailSubject', 'Kvittering for saken din med saksnummer #');
insert into translations (`locale`, `key`, `content`) values ('no', 'VerifyEmailSubject', 'Velkommen som ny bruker hos AnarQ');
insert into translations (`locale`, `key`, `content`) values ('no', 'AlreadyVotedForCase', 'Du har allerede stemt på denne saken');
insert into translations (`locale`, `key`, `content`) values ('no', 'CannotVoteOutsideRegions', 'Du kan ikke stemme på saker som ligger utenfor dine region(er)');
insert into translations (`locale`, `key`, `content`) values ('no', 'CasePastDeadline', 'Denne saken kan du ikke lengre stemme på');
insert into translations (`locale`, `key`, `content`) values ('no', 'VoteHashFailure', 'Hash verdien for stemmen din overenstemmer ikke med forrige stemmes hash verdi. Vår så snill å kontakt en administrator hos AnarQ, og oppgi denne lenken, siden dette er et tegn på manipulasjon av stemmer.');
insert into translations (`locale`, `key`, `content`) values ('no', 'YourVoteWasDeleted', 'Stemmen din har blitt slettet, dette er tegn på noe alvorlig galt og at noen har manipulert avstemmingen. Vennligst kontakt en administrator hos AnarQ og gi han eller henne denne lenken.');
insert into translations (`locale`, `key`, `content`) values ('no', 'YouHaveAlreadyVerifiedEmail', 'Du har allerede bekrefted epost-addressen din');
insert into translations (`locale`, `key`, `content`) values ('no', 'YourVerifyEmailTicketIsNotValid', 'Billetten din for å verifiser epost-addressen din er ikke gyldig');
insert into translations (`locale`, `key`, `content`) values ('no', 'AlreadySetRegions', 'Du har allerede konfigurert regionene dine, og du må spesifikt søke om å forandre de');
insert into translations (`locale`, `key`, `content`) values ('no', 'WeHaveSentResetPasswordLink', 'Vi har sendt deg en lenke for å nullstille passordet ditt');
insert into translations (`locale`, `key`, `content`) values ('no', 'NoSuchUserFound', 'Vi kunne ikke finne denne brukeren');
insert into translations (`locale`, `key`, `content`) values ('no', 'YouCannotChangePassword', 'Du har ikke tilgang til å forandre denne brukerens passord');
insert into translations (`locale`, `key`, `content`) values ('no', 'YouAreNotHuman', 'Du klarte ikke å bevise at du et menneske, kun mennesker kan registrere seg her');
insert into translations (`locale`, `key`, `content`) values ('no', 'PhoneAlreadyRegistered', 'Telefonnummeret er allerede registrert');
insert into translations (`locale`, `key`, `content`) values ('no', 'CreatedCaseInRegionLessThan24HoursAgo', 'Du har laget en ny sak i denne regionen for mindre enn 24 timer siden, vi tillater kun en sak per døgn i denne regionen');
insert into translations (`locale`, `key`, `content`) values ('no', 'CreatedCaseInRegionLessThan1HourAgo', 'Du har laget en ny sak i denne regionen for mindre enn en time siden, vi tillater kun en sak per time i denne regionen');
insert into translations (`locale`, `key`, `content`) values ('no', 'NoHtmlSubjectBody', 'Du kan ikke bruke [, ], < eller > i hverken overskriften eller innholdet');
insert into translations (`locale`, `key`, `content`) values ('no', 'EmailAlreadyRegistered', 'Epost-addresse er allerede registrert');
insert into translations (`locale`, `key`, `content`) values ('no', 'NotValidEmailAddress', 'Ikke en gyldig epost-addresse');
insert into translations (`locale`, `key`, `content`) values ('no', 'Abort', 'Avbryt');
insert into translations (`locale`, `key`, `content`) values ('no', 'PasswordIsNotLongEnough', 'Passordet ditt må være minst 10 bokstaver langt');
insert into translations (`locale`, `key`, `content`) values ('no', 'ForgotYourPassword', 'Har du glemt passordet ditt?');
insert into translations (`locale`, `key`, `content`) values ('no', 'YourPasswordWasChanged', 'Passordet ditt er forandret');
insert into translations (`locale`, `key`, `content`) values ('no', 'PasswordDoNotMatch', 'Passordene er ikke like');
insert into translations (`locale`, `key`, `content`) values ('no', 'CreateNewPassword', 'Nullstill passord');
insert into translations (`locale`, `key`, `content`) values ('no', 'ResetYourPassword', 'Nullstill passordet ditt');
insert into translations (`locale`, `key`, `content`) values ('no', 'EmailUsedDuringRegistration', 'Epost som du brukte da du registrerte deg');
insert into translations (`locale`, `key`, `content`) values ('no', 'IForgotMyPassword', 'Jeg har glemt passsordet mitt');
insert into translations (`locale`, `key`, `content`) values ('no', 'ForgotPassword', 'Glemt passordet?');
insert into translations (`locale`, `key`, `content`) values ('no', 'StartPoliticalRally', 'Start en politisk sak i');
insert into translations (`locale`, `key`, `content`) values ('no', 'TicketValidNowTellRegion', 'Epost-adressen din er bekreftet, nå må du fortelle oss hvor du bor.');
insert into translations (`locale`, `key`, `content`) values ('no', 'seconds', 'sekunder');
insert into translations (`locale`, `key`, `content`) values ('no', 'minutes', 'minutter');
insert into translations (`locale`, `key`, `content`) values ('no', 'hours', 'timer');
insert into translations (`locale`, `key`, `content`) values ('no', 'days', 'dager');
insert into translations (`locale`, `key`, `content`) values ('no', 'weeks', 'uker');
insert into translations (`locale`, `key`, `content`) values ('no', 'months', 'måneder');
insert into translations (`locale`, `key`, `content`) values ('no', 'fromNow', 'fra nå av');
insert into translations (`locale`, `key`, `content`) values ('no', 'ago', 'siden');

insert into translations (`locale`, `key`, `content`) values ('no', 'Logout', 'Logg ut');
insert into translations (`locale`, `key`, `content`) values ('no', 'HomeSlogan', 'Hvor DU er Regjeringen!');
insert into translations (`locale`, `key`, `content`) values ('no', 'ClickHereIfYouLiveInX', 'Trykk here hvis du bor i {0}');
insert into translations (`locale`, `key`, `content`) values ('no', 'StepXOfY', 'Steg {0} av {1}');
insert into translations (`locale`, `key`, `content`) values ('no', 'CongratulationsHeader', 'Gratulere');
insert into translations (`locale`, `key`, `content`) values ('no', 'Users', 'Brukere');
insert into translations (`locale`, `key`, `content`) values ('no', 'Cases', 'Saker');
insert into translations (`locale`, `key`, `content`) values ('no', 'Open', 'Åpne');
insert into translations (`locale`, `key`, `content`) values ('no', 'Votes', 'Stemmer');
insert into translations (`locale`, `key`, `content`) values ('no', 'Popular', 'Populære saker');
insert into translations (`locale`, `key`, `content`) values ('no', 'Newest', 'Ferskeste saker');
insert into translations (`locale`, `key`, `content`) values ('no', 'Closed', 'Avsluttede saker');
insert into translations (`locale`, `key`, `content`) values ('no', 'MyCases', 'Mine saker');
insert into translations (`locale`, `key`, `content`) values ('no', 'CasesIHaveVotedFor', 'Saker jeg har stemt for');
insert into translations (`locale`, `key`, `content`) values ('no', 'NothingToSeeHere', 'Ingenting å se her');
insert into translations (`locale`, `key`, `content`) values ('no', 'Get25NextItems', 'Neste 25 treff');
insert into translations (`locale`, `key`, `content`) values ('no', 'More', 'Mer');
insert into translations (`locale`, `key`, `content`) values ('no', 'ThereAreXUsersInSystem', 'Det er {0} brukere i systemet');
insert into translations (`locale`, `key`, `content`) values ('no', 'ThereAreXCasesInSystem', 'Det er {0} saker i systemet');
insert into translations (`locale`, `key`, `content`) values ('no', 'ThereAreXOpenCasesInSystem', 'Det er {0} åpne saker i systemet');
insert into translations (`locale`, `key`, `content`) values ('no', 'ThereAreXVotesInSystem', 'Det er {0} stemmer i systemet');
insert into translations (`locale`, `key`, `content`) values ('no', 'YouNeedToSetupRegions', 'Du må sette opp regionene dine');
insert into translations (`locale`, `key`, `content`) values ('no', 'Register', 'Registrer');
insert into translations (`locale`, `key`, `content`) values ('no', 'Login', 'Logg inn');
insert into translations (`locale`, `key`, `content`) values ('no', 'CannotCreateCaseInRegion', 'Du kan ikke lage en ny sak i {0} siden du akkurat laget en sak der.');
insert into translations (`locale`, `key`, `content`) values ('no', 'OnlyLoggedInUsersCanCreateCases', 'Kun brukere som er logget inn kan lage saker');
insert into translations (`locale`, `key`, `content`) values ('no', 'OKIGotIt', 'OK, jeg skjønner');
insert into translations (`locale`, `key`, `content`) values ('no', 'SubjectMustBe', 'Overskriften må være 15-100 bokstaver lang, og må avsluttes med et spørsmålstegn ?');
insert into translations (`locale`, `key`, `content`) values ('no', 'Subject', 'Overskrift');
insert into translations (`locale`, `key`, `content`) values ('no', 'Acceptable', 'Akseptert');
insert into translations (`locale`, `key`, `content`) values ('no', 'FeelFreeToUseMarkdown', 'Bruk gjerne Markdown, men INGEN LENKER!');
insert into translations (`locale`, `key`, `content`) values ('no', 'BodyLength', 'Innhold {0} av {1}');
insert into translations (`locale`, `key`, `content`) values ('no', 'Deadline', 'Avsluttes');
insert into translations (`locale`, `key`, `content`) values ('no', 'CaseOpenShort', 'Saken vil kun akseptere stemmer i {0} dager');
insert into translations (`locale`, `key`, `content`) values ('no', 'ShortDeadline', 'Kort frist');
insert into translations (`locale`, `key`, `content`) values ('no', 'CaseOpenMedium', 'Saken vil kun akseptere stemmer i {0} dager');
insert into translations (`locale`, `key`, `content`) values ('no', 'MediumDeadline', 'Medium frist');
insert into translations (`locale`, `key`, `content`) values ('no', 'CaseOpenLong', 'Saken vil kun akseptere stemmer i {0} dager');
insert into translations (`locale`, `key`, `content`) values ('no', 'LongDeadline', 'Lang frist');
insert into translations (`locale`, `key`, `content`) values ('no', 'SubmitCaseToPeopleRepublic', 'Send inn saken din til den uavhengige Republikken {0}');
insert into translations (`locale`, `key`, `content`) values ('no', 'Submit', 'Send inn');
insert into translations (`locale`, `key`, `content`) values ('no', 'CaseSuccessfullyCreated', 'Saken din ble lagret');
insert into translations (`locale`, `key`, `content`) values ('no', 'CheckOutCase', 'Se hvordan saken går');
insert into translations (`locale`, `key`, `content`) values ('no', 'PreviousReceipt', 'Forrige kvittering');
insert into translations (`locale`, `key`, `content`) values ('no', 'Home', 'Lukk');
insert into translations (`locale`, `key`, `content`) values ('no', 'AllRightBaby', 'Okey dokey, jeg er klar til å rulle!');
insert into translations (`locale`, `key`, `content`) values ('no', 'VoteForCaseAbove', 'Stem for ovenstående sak ved å scanne QR koden over');
insert into translations (`locale`, `key`, `content`) values ('no', 'CheckOutHowTheCaseDid', 'Se hvordan saken gjorde seg');
insert into translations (`locale`, `key`, `content`) values ('no', 'QuestionWasAskedIn', 'Spørsmålet ble spurt i {0}');
insert into translations (`locale`, `key`, `content`) values ('no', 'CaseWasBroughtForthBy', 'Spørsmålet ble spurt av {0}');
insert into translations (`locale`, `key`, `content`) values ('no', 'VotesCanBeCastUntil', 'Stemmer kan bli avgitt til {0}');
insert into translations (`locale`, `key`, `content`) values ('no', 'XAyeAndUNay', '{0} ja og {1} nei');
insert into translations (`locale`, `key`, `content`) values ('no', 'VoteAyeForCase', 'Stem ja for denne saken');
insert into translations (`locale`, `key`, `content`) values ('no', 'VoteNayForCase', 'Stem nei for denne saken');
insert into translations (`locale`, `key`, `content`) values ('no', 'YouVotedAye', 'Du stemte ja for denne saken. Kun du kan se hvilke stemme du avga.');
insert into translations (`locale`, `key`, `content`) values ('no', 'YouVotedNay', 'Du stemte nei for denne saken. Kun du kan se hvilke stemme du avga.');
insert into translations (`locale`, `key`, `content`) values ('no', 'CryptoReceiptSent', 'En kryptografisk signert epost ble sendt til deg som kvittering for din stemme. Ta vare på denne eposten.');
insert into translations (`locale`, `key`, `content`) values ('no', 'CaseWasWon', 'Saken ble vunnet');
insert into translations (`locale`, `key`, `content`) values ('no', 'CaseWasLost', 'Saken ble tapt');
insert into translations (`locale`, `key`, `content`) values ('no', 'CaseWasNotDetermined', 'Saken ble uavgjort');
insert into translations (`locale`, `key`, `content`) values ('no', 'ThePeoplesCourt', 'Folkets Uavhengige Republikk i {0}');
insert into translations (`locale`, `key`, `content`) values ('no', 'BringForthAQuestion', 'Frem et spørsmål til Folkedomstolen i {0}');
insert into translations (`locale`, `key`, `content`) values ('no', 'SeemsAllCaughtUp', 'Det ser ut som om det ikke er noe mer her å gjøre for deg');
insert into translations (`locale`, `key`, `content`) values ('no', 'WhyNotCreate', 'Hvorfor lager du ikke en ny sak selv?');
insert into translations (`locale`, `key`, `content`) values ('no', 'GetXNextItems', 'Hent {0} neste saker');
insert into translations (`locale`, `key`, `content`) values ('no', 'ProgressOfRegistration', 'Hvor langt du har kommet i registreringsprosessen');
insert into translations (`locale`, `key`, `content`) values ('no', 'EmailMustBeAvailable', 'Epost må være gyldig og tilgjengelig');
insert into translations (`locale`, `key`, `content`) values ('no', 'Email', 'Epost');
insert into translations (`locale`, `key`, `content`) values ('no', 'UsernameCharacterPattern', '4-30 bokstaver langt, a-z, 0-9 og \'-\' eller \'_\'. Brukernavnet kan heller ikke være eposten din');
insert into translations (`locale`, `key`, `content`) values ('no', 'Username', 'Brukernavn');
insert into translations (`locale`, `key`, `content`) values ('no', 'NamePattern', 'Navnet ditt må være ditt folkeregistrerte navn');
insert into translations (`locale`, `key`, `content`) values ('no', 'FullLegalName', 'Fullt navn');
insert into translations (`locale`, `key`, `content`) values ('no', 'Why', 'Hvorfor ...?');
insert into translations (`locale`, `key`, `content`) values ('no', 'PhonePattern', 'Telefonnummert må være mobiltelefonnummeret ditt, og registrert i ditt navn. Ingen mellomrom, kun tall.');
insert into translations (`locale`, `key`, `content`) values ('no', 'CellPhone', 'Mobiltelefon');
insert into translations (`locale`, `key`, `content`) values ('no', 'PasswordMustBeAtLeast', 'Passordet må være minst 10 bokstaver langt');
insert into translations (`locale`, `key`, `content`) values ('no', 'Password', 'Passord');
insert into translations (`locale`, `key`, `content`) values ('no', 'PleaseRepeatPassword', 'Repeter passordet ditt');
insert into translations (`locale`, `key`, `content`) values ('no', 'RepeatPassword', 'Repeter passord');
insert into translations (`locale`, `key`, `content`) values ('no', 'IAgreeToTermsAndConditions', 'Jeg aksepterer vilkårene for AnarQ');
insert into translations (`locale`, `key`, `content`) values ('no', 'ConfirmThatYouAreHuman', 'Bekreft at du er et menneske');
insert into translations (`locale`, `key`, `content`) values ('no', 'SubmitFormAndRegister', 'Send informasjonen og aksepter vilkårene for bruk av AnarQ');
insert into translations (`locale`, `key`, `content`) values ('no', 'IAmHuman', 'Jeg er et menneske!');
insert into translations (`locale`, `key`, `content`) values ('no', 'ConfirmEmailAddress', 'Sjekk eposten din og bekreft epostadressen din.');
insert into translations (`locale`, `key`, `content`) values ('no', 'YouAreAlreadyRegistered', 'Du er allerede registrert på AnarQ');
insert into translations (`locale`, `key`, `content`) values ('no', 'UsernameAlreadyRegistered', 'Brukernavnet \'{0}\' er allerede opptatt');
insert into translations (`locale`, `key`, `content`) values ('no', 'YouCanOnlyRegisterOnce', 'Du kan kun registrere deg en gang på AnarQ');
insert into translations (`locale`, `key`, `content`) values ('no', 'PleaseCheckEmailInbox', 'Sjekk eposten din');
insert into translations (`locale`, `key`, `content`) values ('no', 'WhereDoYouLive', 'Hvor bor du?');
insert into translations (`locale`, `key`, `content`) values ('no', 'Filter', 'Filtrer');
insert into translations (`locale`, `key`, `content`) values ('no', 'YouHaveAlreadySetupRegions', 'Du har allerede konfigurert regionene dine, og du må spesifikt søke om å forandre de.');
insert into translations (`locale`, `key`, `content`) values ('no', 'OnlyLoggedInUsersCanChangeRegions', 'Kun innloggede brukere kan konfigurere regionene sine.');
insert into translations (`locale`, `key`, `content`) values ('no', 'Congratulations', 'Gratulere, du kan nå avgi stemmer og fremme saker i AnarQ');
insert into translations (`locale`, `key`, `content`) values ('no', 'UserHasVerifiedEmail', 'Brukeren har verifisert epostadressen sin');
insert into translations (`locale`, `key`, `content`) values ('no', 'ThisIsHowManyCases', 'Dette er hvor mange saker brukeren har fremmet');
insert into translations (`locale`, `key`, `content`) values ('no', 'CasesCreated', 'Saker laget');
insert into translations (`locale`, `key`, `content`) values ('no', 'ThisIsHowManyVotesReceived', 'Dette er hvor mange stemmer som brukeren har fått på sakene sine');
insert into translations (`locale`, `key`, `content`) values ('no', 'VotesGiven', 'Stemmer gitt');
insert into translations (`locale`, `key`, `content`) values ('no', 'VotesUserHasGiven', 'Stemmer avgitt');
insert into translations (`locale`, `key`, `content`) values ('no', 'ThisIsHowManyVotesGiven', 'Dette er hvor mange stemmer som brukeren har avgitt');
insert into translations (`locale`, `key`, `content`) values ('no', 'Phone', 'Telefon');
insert into translations (`locale`, `key`, `content`) values ('no', 'VotesUserHasGivenPerRegion', 'Stemmer som bruker har avgitt per region');
insert into translations (`locale`, `key`, `content`) values ('no', 'WinningsAndLoosings', 'Saker som er tapt og vunnet av bruker');
insert into translations (`locale`, `key`, `content`) values ('no', 'YouHaveAlreadyVoted', 'Du har allerede stemt for denne saken');
insert into translations (`locale`, `key`, `content`) values ('no', 'YouHaveNotVoted', 'Du har ikke stemt for denne saken');
insert into translations (`locale`, `key`, `content`) values ('no', 'WonCases', 'Saker vunnet');
insert into translations (`locale`, `key`, `content`) values ('no', 'LostCases', 'Saker tapt');
insert into translations (`locale`, `key`, `content`) values ('no', 'TiedCases', 'Uavgjorte saker');
insert into translations (`locale`, `key`, `content`) values ('no', 'VotesSmall', 'stemmer');
insert into translations (`locale`, `key`, `content`) values ('no', 'YouHaveVerifiedEmail', 'Du har verifisert eposten din');

/*
 * Longer Norwegian translation texts.
 */
insert into translations (`locale`, `key`, `content`) values ('no', 'WhyWeNeedRegions', 
    'Vi må vite hvor du bor, siden vi kun tillater brukere å opprette saker, og '
    'å stemme for saker, i det området de bor i. '
    'Vi i AnarQ tror sterkt på at kun de som blir påvirket av en sak, har uttalerett om '
    'saken. Derfor må vi vite hvor du bor, slik at du kan få lage saker, og stemme på saker, '
    'i det området hvor du faktisk bor.');
insert into translations (`locale`, `key`, `content`) values ('no', 'ReceiptChecksOut', 
      'Kvitteringen er gyldig, og det ser ikke ut som om noen har tuklet med den. \n'
      'I tilfelle en ekstern revisjon av systemet er påkrevd, kan det hende at du må sende \n'
      'kvitteringen din til en uavhengig 3. parts organisasjon.');
insert into translations (`locale`, `key`, `content`) values ('no', 'NameInformation',
      '<p>\n'
      '  For å forsikre oss om at du er en ekte person, og for å forhindre folk fra å registrere \n'
      '  falske profiler, for slik å influere offisielle saker - Så spør vi deg om å gi oss \n'
      '  ditt fulle navn, slik som vi kan finne det i offisielle arkiver. Merk, hvis du ikke kan bevise at\n'
      '  du er en ekte person, så kan det hende at vi suspenderer kontoen din, og nuller stemmene\n'
      '  dine. Og ja, <strong>vi vil sjekke dette.</strong>\n'
      '</p>\n'
      '<p>\n'
      '  Vi vil aldri levere ut dine data til 3. parts organisasjoner eller individer, med mindre Forente Nasjoner rekvirerer en revisjon av en sak.\n'
      '</p>');
insert into translations (`locale`, `key`, `content`) values ('no', 'PhoneInfo', 
      '<p>\n'
      '  For å forsikre oss om at du er en ekte person, og for å forhindre folk fra å registrere \n'
      '  falske profiler, for slik å influere offisielle saker - Så spør vi deg om å gi oss \n'
      '  ditt mobiltelefonnummer, slik som vi kan finne det i offisielle arkiver. Merk, hvis du ikke kan bevise at\n'
      '  du er en ekte person, så kan det hende at vi suspenderer kontoen din, og nuller stemmene\n'
      '  dine. Og ja, <strong>vi vil sjekke dette</strong>. Det kan også hende at vi sender deg en tekstmelding for å\n'
      '  bekrefte at mobiltelefonen faktisk er din.'
      '</p>\n'
      '<p>\n'
      '  Vi vil aldri levere ut dine data til 3. parts organisasjoner eller individer, med mindre Forente Nasjoner rekvirerer en revisjon av en sak.\n'
      '</p>');
insert into translations (`locale`, `key`, `content`) values ('no', 'ReadThisFirstQuestion',
      '<h3>Les dette først</h3>\n'
      '<p>Siden dette er din første sak, så burde du lese dette nøye for å forhindre problemer og forvirring.</p>\n'
      '<ol>\n'
      '<li>Vi vil sensurere saker som vi mener er mobbing av individer.</li>\n'
      '<li>Overskriften din må avsluttes med et spørsmålstegn. I selve saken kan du skrive mer utfyllende, for å overtale andre om ditt syn, men overskriften må avsluttes med \'?\'</li>\n'
      '<li>Alle saker må ha et klart definert ja/nei svar, og det må være enkelt å forstå for andre hvilke svar de avgir hvis de stemmer.</li>\n'
      '<li>Du kan <strong>ikke slette eller redigere saker</strong> etter at du har opprettet de.</li>\n'
      '<li><strong>Personalia om og utlevering av barn er ikke akseptert</strong>. Saker som bryter disse retningslinjene vil bli sensurert. Hvis du nevner barn i saken din, skal barnets navn og personalia anonymiseres. Bruk gjerne \'Kari\' og \'Ola\' som generiske navn hvis du omtaler barn.</li>\n'
      '<li>Overskriften din må være mellom 15 og 100 bokstaver lang.</li>\n'
      '<li>Innholdet må være mellom 50 og 2500 bokstaver langt.</li>\n'
      '<li>Et godt råd er å lese andres saker før du lager egne saker, siden dette vil gi deg en følelse av hvilke saker som er akseptert her, og hvordan du kan generere oppmerksomhet rundt saken din.</li>\n'
      '<li>Du burde også konstruere saken din slik at det resultatet du selv ønsker å oppnå, er at flest mulig svarer <em>"JA"</em>, siden dette resulterer i at du som bruker scorer poeng, noe som resulterer i at du får tilgang til flere funksjoner over tid, og at andre kan se på profilen din at du er en seriøs person.</li>\n'
      '<li>Print gjerne saken din opp i flere eksemplarer etter at du har konstruert den, men ikke forsøple andre folks eiendom ulovlig ved å henge den opp på plasser du ikke har lov til å henge opp saken din.</li>'
      '</ol>\n'
      '<span class="slogan">Happy AnarQ</span>');
insert into translations (`locale`, `key`, `content`) values ('no', 'ReadThisFirstCase',
      '<h3>Les dette</h3>\n'
      '<p>\n'
      '  Siden dette er din første sak, så skal vi gi deg noen gode råd.\n'
      '  For det første, hvis du <strong>printer ut saken din</strong>, så vil den passe fint\n'
      '  på et A4 ark, og inneholde en QR kode, som andre kan scanne, for å komme direkte til din sak.\n'
      '  Dette gjør det mulig for deg å 100% ignorere sensureringsregimer, slik som de du finner på Facebook\n'
      '  og Twitter, ved å henge opp saken din i ditt lokalmiljø, for slik å skaffe lesere til saken din.\n'
      '</p>\n'
      '<p>\n'
      '  Trykk på <strong>CTRL+P</strong> for å se hvordan saken din vil se ut på print.\n'
      '</p>\n'
      '<p>\n'
      '  <strong>Merk!</strong> Du kan selvfølgelig også dele saken din i andre sosiale medier,\n'
      '  men våre erfaringer i forhold til dette, er at både Facebook og Twitter har forutinntatte politiske meninger,\n'
      '  og ofte sensurerer saker som har politisk karakter. Det gjør ikke vi.\n'
      '  Vi har faktisk ikke politiske meninger, utover de du gir oss :)'
      '</p>\n'
      '<span class="slogan">Free Speech! One Each!</span>');
insert into translations (`locale`, `key`, `content`) values ('no', 'TermsAndConditions',
      '<label class="bottom-slogan">Vilkår for bruk av AnarQ</label>\n'
      '<p>\n'
      '  Når du trykker \'Send inn\' knappen under, så lover du oss at informasjonen ovenfor er\n'
      '  korrekt, og at du har gitt oss ditt reelle navn og mobiltelefonnummer.\n'
      '</p>\n'
      '<p>\n'
      '  Du gir oss også <strong>tillatelse til å sende deg epost kvitteringer</strong> for hver stemme du avgir,\n'
      '  og hver sak du oppretter.\n'
      '</p>\n'
      '<p>\n'
      '  Saker du oppretter, vil bli assosiert med ditt brukernavn, men hvilke stemmer du avgir,\n'
      '  er hemmelig, og AnarQ vil ikke avsløre din personlige informasjon, eller hvordan du\n'
      '  stemmer, til noen andre - Med unntak av en nøytral 3. part utnevnt af Forente Nasjoner,\n'
      '  som ønsker å foreta en revisjon av en sak, for å verifisere at den er ekte, reell, og reflekterer\n'
      '  folkets demokratiske mening om saken.\n'
      '</p>');


/*
 * Region information tables, both generically, and for users specifically,
 * to associate a user with one or more regions, to restrict which cases
 * he or she can vote on.
 */

/*
 * Creating region table, that contains all your different regions.
 * This table should contain things such as municipalities, counties,
 * countries, etc.
 */
 create table `regions` (
  `region` varchar(45) not null,
  `description` varchar(256) null,
  primary key (`region`)
);

insert into regions (region, description) values ('norge', 'Land');
insert into regions (region, description) values ('alstahaug', 'Kommune');
insert into regions (region, description) values ('alta', 'Kommune');
insert into regions (region, description) values ('alvdal', 'Kommune');
insert into regions (region, description) values ('alver', 'Kommune');
insert into regions (region, description) values ('andøy', 'Kommune');
insert into regions (region, description) values ('aremark', 'Kommune');
insert into regions (region, description) values ('arendal', 'Kommune');
insert into regions (region, description) values ('asker', 'Kommune');
insert into regions (region, description) values ('askvoll', 'Kommune');
insert into regions (region, description) values ('askøy', 'Kommune');
insert into regions (region, description) values ('aukra', 'Kommune');
insert into regions (region, description) values ('aure', 'Kommune');
insert into regions (region, description) values ('aurland', 'Kommune');
insert into regions (region, description) values ('aurskog-Høland', 'Kommune');
insert into regions (region, description) values ('austevoll', 'Kommune');
insert into regions (region, description) values ('austrheim', 'Kommune');
insert into regions (region, description) values ('averøy', 'Kommune');
insert into regions (region, description) values ('balsfjord', 'Kommune');
insert into regions (region, description) values ('bamble', 'Kommune');
insert into regions (region, description) values ('bardu', 'Kommune');
insert into regions (region, description) values ('beiarn', 'Kommune');
insert into regions (region, description) values ('berg', 'Kommune');
insert into regions (region, description) values ('bergen', 'Kommune');
insert into regions (region, description) values ('berlevåg', 'Kommune');
insert into regions (region, description) values ('bindal', 'Kommune');
insert into regions (region, description) values ('birkenes', 'Kommune');
insert into regions (region, description) values ('bjerkreim', 'Kommune');
insert into regions (region, description) values ('bjornafjorden', 'Kommune');
insert into regions (region, description) values ('bjugn', 'Kommune');
insert into regions (region, description) values ('bodø', 'Kommune');
insert into regions (region, description) values ('bokn', 'Kommune');
insert into regions (region, description) values ('bremanger', 'Kommune');
insert into regions (region, description) values ('brønnøy', 'Kommune');
insert into regions (region, description) values ('bygland', 'Kommune');
insert into regions (region, description) values ('bykle', 'Kommune');
insert into regions (region, description) values ('bærum', 'Kommune');
insert into regions (region, description) values ('bø', 'Kommune');
insert into regions (region, description) values ('bømlo', 'Kommune');
insert into regions (region, description) values ('båtsfjord', 'Kommune');
insert into regions (region, description) values ('dovre', 'Kommune');
insert into regions (region, description) values ('drammen', 'Kommune');
insert into regions (region, description) values ('drangedal', 'Kommune');
insert into regions (region, description) values ('dyrøy', 'Kommune');
insert into regions (region, description) values ('dønna', 'Kommune');
insert into regions (region, description) values ('eidfjord', 'Kommune');
insert into regions (region, description) values ('eidskog', 'Kommune');
insert into regions (region, description) values ('eidsvoll', 'Kommune');
insert into regions (region, description) values ('eigersund', 'Kommune');
insert into regions (region, description) values ('elverum', 'Kommune');
insert into regions (region, description) values ('enebakk', 'Kommune');
insert into regions (region, description) values ('engerdal', 'Kommune');
insert into regions (region, description) values ('etne', 'Kommune');
insert into regions (region, description) values ('etnedal', 'Kommune');
insert into regions (region, description) values ('evenes', 'Kommune');
insert into regions (region, description) values ('evje-og-hornnes', 'Kommune');
insert into regions (region, description) values ('farsund', 'Kommune');
insert into regions (region, description) values ('fauske', 'Kommune');
insert into regions (region, description) values ('fedje', 'Kommune');
insert into regions (region, description) values ('finnøy', 'Kommune');
insert into regions (region, description) values ('fitjar', 'Kommune');
insert into regions (region, description) values ('fjaler', 'Kommune');
insert into regions (region, description) values ('fjord', 'Kommune');
insert into regions (region, description) values ('flakstad', 'Kommune');
insert into regions (region, description) values ('flatanger', 'Kommune');
insert into regions (region, description) values ('flekkefjord', 'Kommune');
insert into regions (region, description) values ('flesberg', 'Kommune');
insert into regions (region, description) values ('flora', 'Kommune');
insert into regions (region, description) values ('flå', 'Kommune');
insert into regions (region, description) values ('folldal', 'Kommune');
insert into regions (region, description) values ('fredrikstad', 'Kommune');
insert into regions (region, description) values ('frogn', 'Kommune');
insert into regions (region, description) values ('froland', 'Kommune');
insert into regions (region, description) values ('frosta', 'Kommune');
insert into regions (region, description) values ('frøya', 'Kommune');
insert into regions (region, description) values ('fyresdal', 'Kommune');
insert into regions (region, description) values ('færder', 'Kommune');
insert into regions (region, description) values ('gamvik', 'Kommune');
insert into regions (region, description) values ('gausdal', 'Kommune');
insert into regions (region, description) values ('gildeskål', 'Kommune');
insert into regions (region, description) values ('giske', 'Kommune');
insert into regions (region, description) values ('gjemnes', 'Kommune');
insert into regions (region, description) values ('gjerdrum', 'Kommune');
insert into regions (region, description) values ('gjerstad', 'Kommune');
insert into regions (region, description) values ('gjesdal', 'Kommune');
insert into regions (region, description) values ('gjøvik', 'Kommune');
insert into regions (region, description) values ('gloppen', 'Kommune');
insert into regions (region, description) values ('gol', 'Kommune');
insert into regions (region, description) values ('gran', 'Kommune');
insert into regions (region, description) values ('grane', 'Kommune');
insert into regions (region, description) values ('gratangen', 'Kommune');
insert into regions (region, description) values ('grimstad', 'Kommune');
insert into regions (region, description) values ('grong', 'Kommune');
insert into regions (region, description) values ('grue', 'Kommune');
insert into regions (region, description) values ('gulen', 'Kommune');
insert into regions (region, description) values ('hadsel', 'Kommune');
insert into regions (region, description) values ('halden', 'Kommune');
insert into regions (region, description) values ('hamar', 'Kommune');
insert into regions (region, description) values ('hamarøy', 'Kommune');
insert into regions (region, description) values ('hammerfest', 'Kommune');
insert into regions (region, description) values ('hareid', 'Kommune');
insert into regions (region, description) values ('harstad', 'Kommune');
insert into regions (region, description) values ('hasvik', 'Kommune');
insert into regions (region, description) values ('hattfjelldal', 'Kommune');
insert into regions (region, description) values ('haugesund', 'Kommune');
insert into regions (region, description) values ('heim', 'Kommune');
insert into regions (region, description) values ('hemnes', 'Kommune');
insert into regions (region, description) values ('hemsedal', 'Kommune');
insert into regions (region, description) values ('herøy-i-møre', 'Kommune');
insert into regions (region, description) values ('herøy i Nordland', 'Kommune');
insert into regions (region, description) values ('hitra', 'Kommune');
insert into regions (region, description) values ('hjartdal', 'Kommune');
insert into regions (region, description) values ('hjelmeland', 'Kommune');
insert into regions (region, description) values ('hol', 'Kommune');
insert into regions (region, description) values ('hole', 'Kommune');
insert into regions (region, description) values ('holmestrand', 'Kommune');
insert into regions (region, description) values ('holtålen', 'Kommune');
insert into regions (region, description) values ('horten', 'Kommune');
insert into regions (region, description) values ('hurdal', 'Kommune');
insert into regions (region, description) values ('hustadvika', 'Kommune');
insert into regions (region, description) values ('hvaler', 'Kommune');
insert into regions (region, description) values ('hyllestad', 'Kommune');
insert into regions (region, description) values ('hægebostad', 'Kommune');
insert into regions (region, description) values ('høyanger', 'Kommune');
insert into regions (region, description) values ('høylandet', 'Kommune');
insert into regions (region, description) values ('hå', 'Kommune');
insert into regions (region, description) values ('ibestad', 'Kommune');
insert into regions (region, description) values ('inderøy', 'Kommune');
insert into regions (region, description) values ('indre-fosen', 'Kommune');
insert into regions (region, description) values ('indre-østfold', 'Kommune');
insert into regions (region, description) values ('iveland', 'Kommune');
insert into regions (region, description) values ('jevnaker', 'Kommune');
insert into regions (region, description) values ('jondal', 'Kommune');
insert into regions (region, description) values ('karasjok', 'Kommune');
insert into regions (region, description) values ('karlsøy', 'Kommune');
insert into regions (region, description) values ('karmøy', 'Kommune');
insert into regions (region, description) values ('kautokeino', 'Kommune');
insert into regions (region, description) values ('klepp', 'Kommune');
insert into regions (region, description) values ('kongsberg', 'Kommune');
insert into regions (region, description) values ('kongsvinger', 'Kommune');
insert into regions (region, description) values ('kragerø', 'Kommune');
insert into regions (region, description) values ('kristiansand', 'Kommune');
insert into regions (region, description) values ('kristiansund', 'Kommune');
insert into regions (region, description) values ('krødsherad', 'Kommune');
insert into regions (region, description) values ('kvam-herad', 'Kommune');
insert into regions (region, description) values ('kvinesdal', 'Kommune');
insert into regions (region, description) values ('kvinnherad', 'Kommune');
insert into regions (region, description) values ('kviteseid', 'Kommune');
insert into regions (region, description) values ('kvitsøy', 'Kommune');
insert into regions (region, description) values ('kvæfjord', 'Kommune');
insert into regions (region, description) values ('kvænangen', 'Kommune');
insert into regions (region, description) values ('kåfjord', 'Kommune');
insert into regions (region, description) values ('larvik', 'Kommune');
insert into regions (region, description) values ('lavangen', 'Kommune');
insert into regions (region, description) values ('lebesby', 'Kommune');
insert into regions (region, description) values ('leirfjord', 'Kommune');
insert into regions (region, description) values ('leka', 'Kommune');
insert into regions (region, description) values ('lenvik', 'Kommune');
insert into regions (region, description) values ('lesja', 'Kommune');
insert into regions (region, description) values ('levanger', 'Kommune');
insert into regions (region, description) values ('lier', 'Kommune');
insert into regions (region, description) values ('lierne', 'Kommune');
insert into regions (region, description) values ('lillehammer', 'Kommune');
insert into regions (region, description) values ('lillesand', 'Kommune');
insert into regions (region, description) values ('lillestrøm', 'Kommune');
insert into regions (region, description) values ('lindesnes', 'Kommune');
insert into regions (region, description) values ('lom', 'Kommune');
insert into regions (region, description) values ('longyearbyen', 'Kommune');
insert into regions (region, description) values ('loppa', 'Kommune');
insert into regions (region, description) values ('lund', 'Kommune');
insert into regions (region, description) values ('lunner', 'Kommune');
insert into regions (region, description) values ('lurøy', 'Kommune');
insert into regions (region, description) values ('luster', 'Kommune');
insert into regions (region, description) values ('lyngdal', 'Kommune');
insert into regions (region, description) values ('lyngen', 'Kommune');
insert into regions (region, description) values ('lærdal', 'Kommune');
insert into regions (region, description) values ('lødingen', 'Kommune');
insert into regions (region, description) values ('lørenskog', 'Kommune');
insert into regions (region, description) values ('løten', 'Kommune');
insert into regions (region, description) values ('malvik', 'Kommune');
insert into regions (region, description) values ('marker', 'Kommune');
insert into regions (region, description) values ('masfjorden', 'Kommune');
insert into regions (region, description) values ('melhus', 'Kommune');
insert into regions (region, description) values ('meløy', 'Kommune');
insert into regions (region, description) values ('meråker', 'Kommune');
insert into regions (region, description) values ('midt-telemark', 'Kommune');
insert into regions (region, description) values ('midtre-gauldal', 'Kommune');
insert into regions (region, description) values ('modalen', 'Kommune');
insert into regions (region, description) values ('modum', 'Kommune');
insert into regions (region, description) values ('molde', 'Kommune');
insert into regions (region, description) values ('moskenes', 'Kommune');
insert into regions (region, description) values ('moss', 'Kommune');
insert into regions (region, description) values ('målselv', 'Kommune');
insert into regions (region, description) values ('måsøy', 'Kommune');
insert into regions (region, description) values ('namsos', 'Kommune');
insert into regions (region, description) values ('namsskogan', 'Kommune');
insert into regions (region, description) values ('nannestad', 'Kommune');
insert into regions (region, description) values ('narvik', 'Kommune');
insert into regions (region, description) values ('nes', 'Kommune');
insert into regions (region, description) values ('nesbyen', 'Kommune');
insert into regions (region, description) values ('nesna', 'Kommune');
insert into regions (region, description) values ('nesodden', 'Kommune');
insert into regions (region, description) values ('nesseby', 'Kommune');
insert into regions (region, description) values ('nissedal', 'Kommune');
insert into regions (region, description) values ('nittedal', 'Kommune');
insert into regions (region, description) values ('nome', 'Kommune');
insert into regions (region, description) values ('nord-aurdal', 'Kommune');
insert into regions (region, description) values ('nord-fron', 'Kommune');
insert into regions (region, description) values ('nord-odal', 'Kommune');
insert into regions (region, description) values ('nordkapp', 'Kommune');
insert into regions (region, description) values ('nordre-follo', 'Kommune');
insert into regions (region, description) values ('nordre-land', 'Kommune');
insert into regions (region, description) values ('nordreisa', 'Kommune');
insert into regions (region, description) values ('nore-og-uvdal', 'Kommune');
insert into regions (region, description) values ('notodden', 'Kommune');
insert into regions (region, description) values ('nærøysund', 'Kommune');
insert into regions (region, description) values ('odda', 'Kommune');
insert into regions (region, description) values ('oppdal', 'Kommune');
insert into regions (region, description) values ('orkland', 'Kommune');
insert into regions (region, description) values ('os', 'Kommune');
insert into regions (region, description) values ('osen', 'Kommune');
insert into regions (region, description) values ('oslo', 'Kommune');
insert into regions (region, description) values ('osterøy', 'Kommune');
insert into regions (region, description) values ('overhalla', 'Kommune');
insert into regions (region, description) values ('porsanger', 'Kommune');
insert into regions (region, description) values ('porsgrunn', 'Kommune');
insert into regions (region, description) values ('rakkestad', 'Kommune');
insert into regions (region, description) values ('rana', 'Kommune');
insert into regions (region, description) values ('randaberg', 'Kommune');
insert into regions (region, description) values ('rauma', 'Kommune');
insert into regions (region, description) values ('rendalen', 'Kommune');
insert into regions (region, description) values ('rennebu', 'Kommune');
insert into regions (region, description) values ('rennesøy', 'Kommune');
insert into regions (region, description) values ('rindal', 'Kommune');
insert into regions (region, description) values ('ringebu', 'Kommune');
insert into regions (region, description) values ('ringerike', 'Kommune');
insert into regions (region, description) values ('ringsaker', 'Kommune');
insert into regions (region, description) values ('risør', 'Kommune');
insert into regions (region, description) values ('rollag', 'Kommune');
insert into regions (region, description) values ('rælingen', 'Kommune');
insert into regions (region, description) values ('rødøy', 'Kommune');
insert into regions (region, description) values ('røros', 'Kommune');
insert into regions (region, description) values ('røst', 'Kommune');
insert into regions (region, description) values ('røyrvik', 'Kommune');
insert into regions (region, description) values ('råde', 'Kommune');
insert into regions (region, description) values ('salangen', 'Kommune');
insert into regions (region, description) values ('saltdal', 'Kommune');
insert into regions (region, description) values ('samnanger', 'Kommune');
insert into regions (region, description) values ('sande', 'Kommune');
insert into regions (region, description) values ('sandefjord', 'Kommune');
insert into regions (region, description) values ('sandnes', 'Kommune');
insert into regions (region, description) values ('sarpsborg', 'Kommune');
insert into regions (region, description) values ('sauda', 'Kommune');
insert into regions (region, description) values ('sel', 'Kommune');
insert into regions (region, description) values ('selbu', 'Kommune');
insert into regions (region, description) values ('seljord', 'Kommune');
insert into regions (region, description) values ('sigdal', 'Kommune');
insert into regions (region, description) values ('siljan', 'Kommune');
insert into regions (region, description) values ('sirdal', 'Kommune');
insert into regions (region, description) values ('skaun', 'Kommune');
insert into regions (region, description) values ('skien', 'Kommune');
insert into regions (region, description) values ('skiptvet', 'Kommune');
insert into regions (region, description) values ('skjervøy', 'Kommune');
insert into regions (region, description) values ('skjåk', 'Kommune');
insert into regions (region, description) values ('smøla', 'Kommune');
insert into regions (region, description) values ('snillfjord', 'Kommune');
insert into regions (region, description) values ('snåsa', 'Kommune');
insert into regions (region, description) values ('sogndal', 'Kommune');
insert into regions (region, description) values ('sokndal', 'Kommune');
insert into regions (region, description) values ('sola', 'Kommune');
insert into regions (region, description) values ('solund', 'Kommune');
insert into regions (region, description) values ('sortland', 'Kommune');
insert into regions (region, description) values ('stad', 'Kommune');
insert into regions (region, description) values ('stange', 'Kommune');
insert into regions (region, description) values ('stavanger', 'Kommune');
insert into regions (region, description) values ('steigen', 'Kommune');
insert into regions (region, description) values ('steinkjer', 'Kommune');
insert into regions (region, description) values ('stjørdal', 'Kommune');
insert into regions (region, description) values ('stord', 'Kommune');
insert into regions (region, description) values ('stor-elvdal', 'Kommune');
insert into regions (region, description) values ('storfjord', 'Kommune');
insert into regions (region, description) values ('strand', 'Kommune');
insert into regions (region, description) values ('stranda', 'Kommune');
insert into regions (region, description) values ('stryn', 'Kommune');
insert into regions (region, description) values ('sula', 'Kommune');
insert into regions (region, description) values ('suldal', 'Kommune');
insert into regions (region, description) values ('sunnfjord', 'Kommune');
insert into regions (region, description) values ('sunndal', 'Kommune');
insert into regions (region, description) values ('surnadal', 'Kommune');
insert into regions (region, description) values ('sveio', 'Kommune');
insert into regions (region, description) values ('sykkylven', 'Kommune');
insert into regions (region, description) values ('sømna', 'Kommune');
insert into regions (region, description) values ('søndre-land', 'Kommune');
insert into regions (region, description) values ('sør-aurdal', 'Kommune');
insert into regions (region, description) values ('sørfold', 'Kommune');
insert into regions (region, description) values ('sør-fron', 'Kommune');
insert into regions (region, description) values ('sør-odal', 'Kommune');
insert into regions (region, description) values ('sørreisa', 'Kommune');
insert into regions (region, description) values ('sør-varanger', 'Kommune');
insert into regions (region, description) values ('tana', 'Kommune');
insert into regions (region, description) values ('time', 'Kommune');
insert into regions (region, description) values ('tingvoll', 'Kommune');
insert into regions (region, description) values ('tinn', 'Kommune');
insert into regions (region, description) values ('tjeldsund', 'Kommune');
insert into regions (region, description) values ('tokke', 'Kommune');
insert into regions (region, description) values ('tolga', 'Kommune');
insert into regions (region, description) values ('torsken', 'Kommune');
insert into regions (region, description) values ('tranøy', 'Kommune');
insert into regions (region, description) values ('tromsø', 'Kommune');
insert into regions (region, description) values ('trondheim', 'Kommune');
insert into regions (region, description) values ('trysil', 'Kommune');
insert into regions (region, description) values ('træna', 'Kommune');
insert into regions (region, description) values ('tvedestrand', 'Kommune');
insert into regions (region, description) values ('tydal', 'Kommune');
insert into regions (region, description) values ('tynset', 'Kommune');
insert into regions (region, description) values ('tysnes', 'Kommune');
insert into regions (region, description) values ('tysvær', 'Kommune');
insert into regions (region, description) values ('tønsberg', 'Kommune');
insert into regions (region, description) values ('ullensaker', 'Kommune');
insert into regions (region, description) values ('ullensvang', 'Kommune');
insert into regions (region, description) values ('ulstein', 'Kommune');
insert into regions (region, description) values ('ulvik', 'Kommune');
insert into regions (region, description) values ('utsira', 'Kommune');
insert into regions (region, description) values ('vadsø', 'Kommune');
insert into regions (region, description) values ('vaksdal', 'Kommune');
insert into regions (region, description) values ('valle', 'Kommune');
insert into regions (region, description) values ('vang', 'Kommune');
insert into regions (region, description) values ('vanylven', 'Kommune');
insert into regions (region, description) values ('vardø', 'Kommune');
insert into regions (region, description) values ('vefsn', 'Kommune');
insert into regions (region, description) values ('vega', 'Kommune');
insert into regions (region, description) values ('vegårshei', 'Kommune');
insert into regions (region, description) values ('vennesla', 'Kommune');
insert into regions (region, description) values ('verdal', 'Kommune');
insert into regions (region, description) values ('vestby', 'Kommune');
insert into regions (region, description) values ('vestnes', 'Kommune');
insert into regions (region, description) values ('vestre-slidre', 'Kommune');
insert into regions (region, description) values ('vestre-toten', 'Kommune');
insert into regions (region, description) values ('vestvågøy', 'Kommune');
insert into regions (region, description) values ('vevelstad', 'Kommune');
insert into regions (region, description) values ('vik', 'Kommune');
insert into regions (region, description) values ('vindafjord', 'Kommune');
insert into regions (region, description) values ('vinje', 'Kommune');
insert into regions (region, description) values ('volda', 'Kommune');
insert into regions (region, description) values ('voss', 'Kommune');
insert into regions (region, description) values ('værøy', 'Kommune');
insert into regions (region, description) values ('vågan', 'Kommune');
insert into regions (region, description) values ('vågsøy', 'Kommune');
insert into regions (region, description) values ('vågå', 'Kommune');
insert into regions (region, description) values ('våler-i-hedmark', 'Kommune');
insert into regions (region, description) values ('våler-i-østfold', 'Kommune');
insert into regions (region, description) values ('øksnes', 'Kommune');
insert into regions (region, description) values ('ørland', 'Kommune');
insert into regions (region, description) values ('ørsta', 'Kommune');
insert into regions (region, description) values ('østre-toten', 'Kommune');
insert into regions (region, description) values ('øvre-eiker', 'Kommune');
insert into regions (region, description) values ('øyer', 'Kommune');
insert into regions (region, description) values ('øygarden', 'Kommune');
insert into regions (region, description) values ('øystre Slidre', 'Kommune');
insert into regions (region, description) values ('åfjord', 'Kommune');
insert into regions (region, description) values ('ål', 'Kommune');
insert into regions (region, description) values ('ålesund', 'Kommune');
insert into regions (region, description) values ('åmli', 'Kommune');
insert into regions (region, description) values ('åmot', 'Kommune');
insert into regions (region, description) values ('årdal', 'Kommune');
insert into regions (region, description) values ('ås', 'Kommune');
insert into regions (region, description) values ('åseral', 'Kommune');
insert into regions (region, description) values ('åsnes', 'Kommune');

/*
 * Creating association between regions and users table.
 * Notice, a user might belong to multiple regions, due to
 * him or her having an association with both for instance
 * a country, county and a municipality, etc.
 *
 * But a region might also be a group, such as "board of director",
 * "employees", "clients", etc.
 *
 * Each case is associated with one or more "regions", and only
 * users belonging to that region are allowed to cast votes on
 * the case at hand.
 */
create table `users_regions` (
  `user` varchar(256) not null,
  `region` varchar(45) not null,
  primary key (`user`, `region`),
  key `user_fky_idx` (`user`),
  key `region_fky_idx` (`region`),
  constraint `users_regions_user_fky` foreign key (`user`) references `users` (`username`) on delete cascade,
  constraint `users_regions_region_fky` foreign key (`region`) references `regions` (`region`) on delete cascade
);





/*
 * Cases related tables, that contains everything people want to vote
 * in regards to.
 */


/*
 * All types of cases that the system supports.
 */
create table `case_types` (
  `type` varchar(30) not null,
  `description` varchar(256) not null,
  primary key (`type`)
);

/*
 * Inserting some case types into our above table.
 */
insert into case_types (type, description) values ('new', 'This is a newly added case that has not yet been approved');
insert into case_types (type, description) values ('open', 'This is an open vote, that users can vote about');
insert into case_types (type, description) values ('rejected', 'This is a rejected case, that an administrator in the system for some reasons rejected');

/*
 * This is all cases in the system.
 * This includes cases that have been suggested, voted about,
 * closed, legislated, etc.
 */
create table `cases` (
  `id` bigint not null auto_increment,
  `type` varchar(30) not null default 'new',
  `region` varchar(45) not null, /* Only users within specified region are allowed to cast votes on case */
  `user` varchar(256) not null,
  `moderator` varchar(256) null, /* User that moderated the case */
  `created` timestamp not null default now(),
  `deadline` datetime null,
  `subject` varchar(1024) null,
  `body` text not null, /* Notice, maximum 65.535 characters! */
  primary key (`id`),
  key `type_fky_idx` (`type`),
  constraint `cases_type_fky` foreign key (`type`) references `case_types` (`type`),
  constraint `cases_region_fky` foreign key (`region`) references `regions` (`region`),
  constraint `cases_user_fky` foreign key (`user`) references `users` (`username`),
  constraint `cases_moderator_fky` foreign key (`moderator`) references `users` (`username`)
);

/*
 * Making sure our default deadline is 3 days for new cases.
 */
delimiter ;;
create trigger `cases_deadline_trigger` before insert on `cases` for each row
begin
  if new.`deadline` is null then
    set new.`deadline` = date_add(now(), interval 3 day);
  end if;
end;;
delimiter ;

/*
 * Votes table.
 * These are all votes that have been casted for a specific case.
 */
create table `votes` (
  `id` bigint not null auto_increment,
  `case` bigint not null,
  `user` varchar(256) not null,
  `created` datetime not null,
  `opinion` boolean not null,
  `hash` varchar(256) not null,
  primary key (`id`),
  key `case_fky_idx` (`case`),
  unique key `votes_hash_unique` (`hash`),
  constraint `votes_case_fky` foreign key (`case`) references `cases` (`id`), /* No delete to safe guard votes given! */
  constraint `votes_user_fky` foreign key (`user`) references `users` (`username`) on delete cascade
);
