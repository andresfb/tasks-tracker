create table Categories
(
    Id        TEXT constraint categories_pk primary key,
    Name      TEXT    not null,
    DeletedAt integer,
    CreatedAt integer not null,
    UpdatedAt integer not null
);

create unique index Categories_Name_uindex on Categories (Name COLLATE NOCASE);