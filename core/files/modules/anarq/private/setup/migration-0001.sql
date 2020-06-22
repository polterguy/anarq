
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
update translations set `content` = 'Registrer' where `key` = 'Register' and `locale` = 'no'
update translations set `content` = 'Lukk' where `key` = 'Home' and `locale` = 'no'

/*
 * Deletions.
 */
delete from translations where `key` = 'HomeSlogan';


