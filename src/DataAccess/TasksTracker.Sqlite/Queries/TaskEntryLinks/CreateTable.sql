create table TaskEntryLinks
(
    Id          TEXT    not null constraint TaskEntryLink_pk primary key,
    TaskEntryId TEXT    not null constraint TaskEntryLink___fk_TaskEntry references TaskEntry on delete cascade,
    Link        TEXT    not null,
    DeletedAt   INTEGER,
    CreatedAt   INTEGER not null,
    UpdatedAt   INTEGER not null
);