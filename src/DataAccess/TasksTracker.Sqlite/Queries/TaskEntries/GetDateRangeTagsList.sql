SELECT 
    DISTINCT t.* 
FROM TaskEntry t
INNER JOIN TasksTags tt on t.Id = tt.TaskEntryId
INNER JOIN Tags tg ON tt.TagId = tg.Id
WHERE t.CreatedAt BETWEEN @FromDate AND @ToDate
AND tg.Title COLLATE NOCASE IN @Tags 
ORDER BY t.CreatedAt DESC;