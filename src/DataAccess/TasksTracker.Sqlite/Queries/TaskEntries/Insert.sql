INSERT INTO TaskEntry
(Id, CategoryId, Title, Slug, Notes, CreatedAt, UpdatedAt) 
VALUES
(@Id, @CategoryId, @Title, @Slug, @Notes, @CreatedAt, @UpdatedAt);