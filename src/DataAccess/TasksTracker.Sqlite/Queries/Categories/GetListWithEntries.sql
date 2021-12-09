SELECT 
    c.*,
    (SELECT COUNT(ID) FROM TaskEntry WHERE CategoryId == c.Id) as EntriesCount
FROM Categories c 
WHERE EntriesCount > 0
ORDER BY c.UpdatedAt DESC