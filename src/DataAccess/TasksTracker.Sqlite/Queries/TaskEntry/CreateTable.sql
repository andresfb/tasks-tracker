create table TaskEntry
(
    Id         TEXT constraint TaskEntry_pk primary key,
    CategoryId TEXT    not null constraint fk_Categories references Categories,
    Title      TEXT    not null,
    Status     integer default 0 not null,
    Notes      TEXT,
    DeletedAt  integer,
    CreatedAt  integer not null,
    UpdatedAt  integer not null
);