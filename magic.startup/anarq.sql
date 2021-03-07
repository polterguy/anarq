
/*
 * AnarQ main SQL scheme.
 */
create database anarq;
use anarq;


/*
 * Topic posts belongs to.
 */
create table topics (
  name varchar(25) not null,
  description varchar(2048) null,
  primary key (name)
);


/*
 * Inserting default topic into database.
 */
insert into topics (name, description) values ('general', 'General discussions for things not related to anything in particular');
insert into topics (name, description) values ('news', 'Commentary on news and articles');


/*
 * Visibility for posts.
 */
create table visibility (
  name varchar(25) not null,
  description varchar(2048) null,
  primary key (name)
);


/*
 * Inserting default visibility values for posts.
 */
insert into visibility (name, description) values ('public', 'Publicly visible post, implying anyone can see it');
insert into visibility (name, description) values ('protected', 'Protected post, implying only authenticated and authorised users can see it');
insert into visibility (name, description) values ('moderated', 'Moderated post, implying post was explicitly moderated and removed by a moderator');


/*
 * Actual table for posts.
 */
create table posts (
  id int(8) not null auto_increment,
  topic varchar(25) null,
  visibility varchar(25) not null,
  parent int(8) null,
  content text not null,
  created datetime not null default current_timestamp,
  user varchar(256) not null,
  path varchar(2048) null,
  index idx_created (created),
  index idx_user (user),
  index idx_topic (topic),
  unique index idx_path (path),
  primary key (id)
);


/*
 * Likes for posts.
 */
create table likes (
  post_id int(8) not null,
  user varchar(256) not null,
  primary key (post_id, user)
);


/*
 * Pages for site.
 */
create table pages (
  url varchar(256) not null,
  name varchar(256) not null,
  content text not null,
  primary key (url)
);


/*
 * Adding referential integrity for posts pointing towards topics, visibility and parent.
 */
alter table posts add foreign key(topic) references topics(name) on delete cascade on update cascade;
alter table posts add foreign key(visibility) references visibility(name) on delete cascade on update cascade;
alter table posts add foreign key(parent) references posts(id) on delete cascade on update cascade;


/*
 * Adding referential integrity for likes pointing towards posts.
 */
alter table likes add foreign key(post_id) references posts(id) on delete cascade on update cascade;
