create table TaskEntry
(
    Id         TEXT constraint TaskEntry_pk primary key,
    Title      TEXT    not null,
    Slug       TEXT    not null,
    Status     integer default 0 not null,
    Notes      TEXT,
    DeletedAt  integer,
    CreatedAt  integer not null,
    UpdatedAt  integer not null
);

create index TaskEntry__slug on TaskEntry (Slug);