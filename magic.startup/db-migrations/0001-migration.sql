
/*
 * Migrates the AnarQ database to the latest version.
 */
alter table likes add created datetime not null default current_timestamp;
