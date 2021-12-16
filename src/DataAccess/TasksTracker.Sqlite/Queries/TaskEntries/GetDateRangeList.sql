SELECT 
   * 
FROM TaskEntry 
WHERE CreatedAt BETWEEN @FromDate AND @ToDate 
ORDER BY CreatedAt DESC;