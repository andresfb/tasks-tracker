create table Tags
(
    Id        TEXT    not null constraint Tags_pk primary key,
    Title     TEXT    not null,
    IsDefault INTEGER default 0,
    DeletedAt INTEGER,
    CreatedAt INTEGER not null,
    UpdatedAt INTEGER not null
);

create index Tags__Title on Tags (Title);

create table TasksTags
(
    Id          TEXT not null constraint TasksTags_pk primary key,
    TaskEntryId TEXT not null constraint TasksTags___fk_TaskEntry references TaskEntry on delete cascade,
    TagId       TEXT not null constraint TasksTags___fk_Tags references Tags on delete cascade
);

create unique index TasksTags__Ids on TasksTags (TaskEntryId, TagId);