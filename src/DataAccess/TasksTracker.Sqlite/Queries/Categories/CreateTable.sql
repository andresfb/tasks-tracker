create table Categories
(
    Id        TEXT constraint categories_pk primary key,
    Name      TEXT    not null,
    DeletedAt integer,
    CreatedAt integer not null,
    UpdatedAt integer not null
);

create unique index categories_Id_uindex on Categories (Id);