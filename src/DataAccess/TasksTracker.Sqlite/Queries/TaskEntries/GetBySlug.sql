SELECT * FROM TaskEntry WHERE Slug = @Slug COLLATE NOCASE ORDER BY UpdatedAt DESC; 