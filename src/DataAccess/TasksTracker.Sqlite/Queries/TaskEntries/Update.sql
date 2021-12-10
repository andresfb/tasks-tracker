UPDATE TaskEntry SET 
    CategoryId = @CategoryId, 
    Title = @Title, 
    Slug = @Slug, 
    Status = @Status, 
    Notes = @Notes,
    UpdatedAt = @UpdatedAt 
WHERE Id = @Id;