SELECT Id
FROM TaskEntry
WHERE Slug = @Slug COLLATE NOCASE
AND CreatedAt BETWEEN @FromDate AND @ToDate
ORDER BY CreatedAt DESC;