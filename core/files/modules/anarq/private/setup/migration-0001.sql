
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

/*
 * Deletions.
 */
delete from translations where `key` = 'HomeSlogan';
delete from translations where `key` = 'fromNow';
delete from translations where `key` = 'StartPoliticalRally';


