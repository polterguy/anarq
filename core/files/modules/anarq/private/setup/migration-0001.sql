
/*
 * Migration script to evaluated during updating of app.
 *
 * New inserts.
 */
insert into translations (`locale`, `key`, `content`) values ('no', 'AuditYourVote', 'Verifiser stemmen din');
insert into translations (`locale`, `key`, `content`) values ('en', 'AuditYourVote', 'Audit your vote');


/*
 * Updates.
 */
update translations set `content` = 'Registrer' where `key` = 'Register' and `locale` = 'no';
update translations set `content` = 'Lukk' where `key` = 'Home' and `locale` = 'no';
update translations set `content` = 'Neste' where `key` = 'Get25NextItems' and `locale` = 'no';
update translations set `content` = 'Open for {0} days' where `key` = 'CaseOpenShort' and `locale` = 'en';
update translations set `content` = 'Open for {0} days' where `key` = 'CaseOpenMedium' and `locale` = 'en';
update translations set `content` = 'Open for {0} days' where `key` = 'CaseOpenLong' and `locale` = 'en';
update translations set `content` = 'Åpen i {0} dager' where `key` = 'CaseOpenShort' and `locale` = 'no';
update translations set `content` = 'Åpen i {0} dager' where `key` = 'CaseOpenMedium' and `locale` = 'no';
update translations set `content` = 'Åpen i {0} dager' where `key` = 'CaseOpenLong' and `locale` = 'no';



/*
 * Deletions.
 */
delete from translations where `key` = 'HomeSlogan';
delete from translations where `key` = 'fromNow';
delete from translations where `key` = 'StartPoliticalRally';
delete from translations where `key` = 'VotesUserHasGivenPerRegion';
delete from translations where `key` = 'YouHaveVerifiedEmail';





